using System.Net.Http.Json;
using System.Text.Json;
using BookWise.Application.DTOs.Responses;
using BookWise.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
 
namespace BookWise.Application.Services;
 
public class RemoteBookSearchService : IRemoteBookSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemoteBookSearchService> _logger;
    private readonly string? _googleApiKey;
 
    public RemoteBookSearchService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<RemoteBookSearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _googleApiKey = configuration["GoogleBooks:ApiKey"];
    }
 
    public async Task<ApiResponse<IEnumerable<RemoteBookResultViewModel>>> SearchAsync(
        string term,
        IEnumerable<string>? sources = null,
        int limit = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return ApiResponse<IEnumerable<RemoteBookResultViewModel>>.Ok([]);
 
        limit = Math.Clamp(limit, 1, 40);
 
        var requestedSources = (sources ?? ["google", "openlibrary"])
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();
 
        if (requestedSources.Count == 0)
            requestedSources = ["google", "openlibrary"];
 
        var tasks = new List<Task<(string Source, List<RemoteBookResultViewModel> Results, string? Error)>>();
 
        foreach (var source in requestedSources)
        {
            tasks.Add(source switch
            {
                "google" or "googlebooks" => Wrap("google", () => SearchGoogleAsync(term, limit, ct)),
                "openlibrary" or "open-library" or "ol" => Wrap("openlibrary", () => SearchOpenLibraryAsync(term, limit, ct)),
                _ => Task.FromResult((source, new List<RemoteBookResultViewModel>(), "Fonte não suportada."))
            });
        }
 
        var results = await Task.WhenAll(tasks);
        var merged = results.SelectMany(r => r.Results).ToList();
        var errors = results.Where(r => !string.IsNullOrWhiteSpace(r.Error))
            .Select(r => $"{r.Source}: {r.Error}")
            .ToList();
 
        var message = errors.Count > 0 ? $"Resultados parciais. Falhas: {string.Join(" | ", errors)}" : null;
        return ApiResponse<IEnumerable<RemoteBookResultViewModel>>.Ok(merged, message);
    }
 
    private async Task<(string Source, List<RemoteBookResultViewModel> Results, string? Error)> Wrap(
        string source,
        Func<Task<List<RemoteBookResultViewModel>>> action)
    {
        try
        {
            var res = await action();
            return (source, res, null);
        }
        catch (OperationCanceledException)
        {
            return (source, [], "Operação cancelada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Remote search failed for {Source}", source);
            return (source, [], "Erro ao consultar serviço remoto.");
        }
    }
 
    private async Task<List<RemoteBookResultViewModel>> SearchGoogleAsync(string term, int limit, CancellationToken ct)
    {
        var fields =
            "items(id,volumeInfo/title,volumeInfo/authors,volumeInfo/publishedDate,volumeInfo/description,volumeInfo/imageLinks/thumbnail,volumeInfo/industryIdentifiers)";
 
        var url =
            $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(term)}&maxResults={limit}&printType=books&fields={Uri.EscapeDataString(fields)}";
 
        if (!string.IsNullOrWhiteSpace(_googleApiKey))
            url += $"&key={Uri.EscapeDataString(_googleApiKey)}";
 
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
 
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        if (json.ValueKind != JsonValueKind.Object)
            return [];
 
        if (!json.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            return [];
 
        var list = new List<RemoteBookResultViewModel>();
 
        foreach (var item in items.EnumerateArray())
        {
            var sourceId = item.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(sourceId))
                continue;
 
            if (!item.TryGetProperty("volumeInfo", out var v) || v.ValueKind != JsonValueKind.Object)
                continue;
 
            var title = v.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(title))
                continue;
 
            var authors = new List<string>();
            if (v.TryGetProperty("authors", out var authorsEl) && authorsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var a in authorsEl.EnumerateArray())
                {
                    var name = a.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                        authors.Add(name);
                }
            }
 
            int? year = null;
            if (v.TryGetProperty("publishedDate", out var dateEl))
                year = ExtractYear(dateEl.GetString());
 
            var description = v.TryGetProperty("description", out var descEl) ? descEl.GetString() : null;
            var cover = ExtractGoogleThumbnail(v);
            var isbn = ExtractGoogleIsbn(v);
 
            list.Add(new RemoteBookResultViewModel(
                "google",
                sourceId!,
                title!,
                authors.Count > 0 ? authors : ["Autor desconhecido"],
                year,
                isbn,
                description,
                cover
            ));
        }
 
        return list;
    }
 
    private async Task<List<RemoteBookResultViewModel>> SearchOpenLibraryAsync(string term, int limit, CancellationToken ct)
    {
        var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(term)}&limit={limit}";
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
 
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        if (json.ValueKind != JsonValueKind.Object)
            return [];
 
        if (!json.TryGetProperty("docs", out var docs) || docs.ValueKind != JsonValueKind.Array)
            return [];
 
        var list = new List<RemoteBookResultViewModel>();
 
        foreach (var doc in docs.EnumerateArray())
        {
            var key = doc.TryGetProperty("key", out var keyEl) ? keyEl.GetString() : null;
            var title = doc.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(title))
                continue;
 
            int? year = null;
            if (doc.TryGetProperty("first_publish_year", out var yEl) && yEl.ValueKind == JsonValueKind.Number && yEl.TryGetInt32(out var y))
                year = y;
 
            var authors = new List<string>();
            if (doc.TryGetProperty("author_name", out var authorsEl) && authorsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var a in authorsEl.EnumerateArray())
                {
                    var name = a.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                        authors.Add(name);
                }
            }
 
            string? isbn = null;
            if (doc.TryGetProperty("isbn", out var isbnEl) && isbnEl.ValueKind == JsonValueKind.Array)
                isbn = isbnEl.EnumerateArray().Select(i => i.GetString()).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
 
            string? cover = null;
            if (doc.TryGetProperty("cover_i", out var coverEl) && coverEl.ValueKind == JsonValueKind.Number && coverEl.TryGetInt32(out var coverId))
                cover = $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg";
 
            list.Add(new RemoteBookResultViewModel(
                "openlibrary",
                key!,
                title!,
                authors.Count > 0 ? authors : ["Autor desconhecido"],
                year,
                isbn,
                null,
                cover
            ));
        }
 
        return list;
    }
 
    private static int? ExtractYear(string? publishedDate)
    {
        if (string.IsNullOrWhiteSpace(publishedDate))
            return null;
 
        if (publishedDate.Length >= 4 && int.TryParse(publishedDate[..4], out var year))
            return year;
 
        return null;
    }
 
    private static string? ExtractGoogleThumbnail(JsonElement volumeInfo)
    {
        if (!volumeInfo.TryGetProperty("imageLinks", out var img) || img.ValueKind != JsonValueKind.Object)
            return null;
 
        if (!img.TryGetProperty("thumbnail", out var thumbEl))
            return null;
 
        var url = thumbEl.GetString();
        if (string.IsNullOrWhiteSpace(url))
            return null;
 
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            ? "https://" + url["http://".Length..]
            : url;
    }
 
    private static string? ExtractGoogleIsbn(JsonElement volumeInfo)
    {
        if (!volumeInfo.TryGetProperty("industryIdentifiers", out var ids) || ids.ValueKind != JsonValueKind.Array)
            return null;
 
        string? isbn13 = null;
        string? isbn10 = null;
 
        foreach (var id in ids.EnumerateArray())
        {
            var type = id.TryGetProperty("type", out var t) ? t.GetString() : null;
            var val = id.TryGetProperty("identifier", out var v) ? v.GetString() : null;
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(val))
                continue;
 
            if (type.Equals("ISBN_13", StringComparison.OrdinalIgnoreCase))
                isbn13 ??= val;
            else if (type.Equals("ISBN_10", StringComparison.OrdinalIgnoreCase))
                isbn10 ??= val;
        }
 
        return isbn13 ?? isbn10;
    }
}
