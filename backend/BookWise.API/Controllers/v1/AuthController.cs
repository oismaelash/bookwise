using System.Security.Claims;
using BookWise.Application.DTOs.Requests;
using BookWise.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookWise.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpGet("google/start")]
    public IActionResult GoogleStart([FromQuery] string? returnUrl)
    {
        var cfg = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var clientId = cfg["Auth:Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return BadRequest(new { message = "Google ClientId não configurado." });

        var callbackUrl = cfg["Auth:Google:RedirectUri"];
        if (string.IsNullOrWhiteSpace(callbackUrl))
            callbackUrl = Url.ActionLink(nameof(GoogleCallback), "Auth", values: null, protocol: Request.Scheme, host: Request.Host.ToString());
        if (string.IsNullOrWhiteSpace(callbackUrl))
            return BadRequest(new { message = "Callback URL inválida." });

        var state = Guid.NewGuid().ToString("N");
        var allowedOrigins = (cfg["Cors:AllowedOrigins"] ?? "http://localhost:4000")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var safeReturnUrl = BuildSafeReturnUrl(returnUrl, allowedOrigins) ?? $"{allowedOrigins[0].TrimEnd('/')}/login";

        Response.Cookies.Append("g_state", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/v1/auth/callback/google",
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        Response.Cookies.Append("g_return", safeReturnUrl, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/v1/auth/callback/google",
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        var authUrl =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            "?client_id=" + Uri.EscapeDataString(clientId) +
            "&redirect_uri=" + Uri.EscapeDataString(callbackUrl) +
            "&response_type=code" +
            "&scope=" + Uri.EscapeDataString("openid email profile") +
            "&state=" + Uri.EscapeDataString(state) +
            "&prompt=select_account";

        return Redirect(authUrl);
    }

    [AllowAnonymous]
    [HttpGet("callback/google")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error, CancellationToken ct)
    {
        var cfg = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var returnUrl = Request.Cookies["g_return"];
        var stateCookie = Request.Cookies["g_state"];

        Response.Cookies.Delete("g_return", new CookieOptions { Path = "/api/v1/auth/callback/google" });
        Response.Cookies.Delete("g_state", new CookieOptions { Path = "/api/v1/auth/callback/google" });

        if (!string.IsNullOrWhiteSpace(error))
            return Redirect(AppendFragment(returnUrl, $"error={Uri.EscapeDataString(error)}"));

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(stateCookie) || !string.Equals(state, stateCookie, StringComparison.Ordinal))
            return Redirect(AppendFragment(returnUrl, "error=invalid_state"));

        var callbackUrl = cfg["Auth:Google:RedirectUri"];
        if (string.IsNullOrWhiteSpace(callbackUrl))
            callbackUrl = Url.ActionLink(nameof(GoogleCallback), "Auth", values: null, protocol: Request.Scheme, host: Request.Host.ToString());
        if (string.IsNullOrWhiteSpace(callbackUrl))
            return Redirect(AppendFragment(returnUrl, "error=invalid_callback"));

        var result = await _authService.LoginWithGoogleCodeAsync(code, callbackUrl, ct);
        if (!result.Success || result.Data is null)
            return Redirect(AppendFragment(returnUrl, $"error={Uri.EscapeDataString(result.ErrorCode ?? "google_login_failed")}"));

        var fragment =
            "access_token=" + Uri.EscapeDataString(result.Data.AccessToken) +
            "&token_type=" + Uri.EscapeDataString(result.Data.TokenType) +
            "&expires_in=" + Uri.EscapeDataString(result.Data.ExpiresInSeconds.ToString());

        return Redirect(AppendFragment(returnUrl, fragment));
    }

    [AllowAnonymous]
    [HttpPost("otp/request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RequestOtpAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("otp/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.VerifyOtpAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("google")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Google([FromBody] GoogleLoginRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginWithGoogleAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(userIdRaw, out var userId))
            return Unauthorized();

        var result = await _authService.MeAsync(userId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    private static string AppendFragment(string? returnUrl, string fragment)
    {
        var baseUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/login" : returnUrl!;
        var hashIndex = baseUrl.IndexOf('#');
        var clean = hashIndex >= 0 ? baseUrl.Substring(0, hashIndex) : baseUrl;
        return $"{clean}#{fragment}";
    }

    private static string? BuildSafeReturnUrl(string? returnUrl, string[] allowedOrigins)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri)) return null;

        var origin = $"{uri.Scheme}://{uri.Authority}";
        return allowedOrigins.Any(o => string.Equals(o.TrimEnd('/'), origin, StringComparison.OrdinalIgnoreCase))
            ? returnUrl
            : null;
    }
}
