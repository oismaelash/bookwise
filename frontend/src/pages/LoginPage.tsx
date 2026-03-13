import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Rocket, Shield, MessageSquareText } from 'lucide-react';
import { authApi, ApiError } from '../services/api';
import { useAuth } from '../auth/AuthContext';

declare global {
  interface Window {
    google?: any;
  }
}

type Mode = 'otp' | 'google';

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { setSession, bootstrapWithToken, token } = useAuth();

  const redirectTo = (location.state as { from?: string } | null)?.from || '/app';

  const [mode, setMode] = React.useState<Mode>('otp');
  const [ddi, setDdi] = React.useState('+55');
  const [phoneNumber, setPhoneNumber] = React.useState('');
  const [code, setCode] = React.useState('');
  const [otpSent, setOtpSent] = React.useState(false);
  const [loading, setLoading] = React.useState(false);

  const apiUrl = (import.meta.env.VITE_API_URL as string | undefined) || 'http://localhost:5000/api/v1';

  React.useEffect(() => {
    if (token) navigate(redirectTo, { replace: true });
  }, [token, navigate, redirectTo]);

  React.useEffect(() => {
    const params = new URLSearchParams(window.location.hash.replace(/^#/, ''));
    const accessToken = params.get('access_token');
    if (!accessToken) return;

    window.history.replaceState(null, '', window.location.pathname + window.location.search);

    (async () => {
      await bootstrapWithToken(accessToken);
      navigate(redirectTo, { replace: true });
    })();
  }, [bootstrapWithToken, navigate, redirectTo]);

  function buildE164(): string | null {
    const rawDdi = ddi.trim().replace(/\s+/g, '');
    const ddiDigits = rawDdi.replace(/[^\d]/g, '');
    const normalizedDdi = ddiDigits ? `+${ddiDigits}` : '';

    const numDigits = phoneNumber.replace(/[^\d]/g, '');
    if (!normalizedDdi || !numDigits) return null;
    return `${normalizedDdi}${numDigits}`;
  }

  function handleGoogleRedirect() {
    const returnUrl = `${window.location.origin}/login`;
    const url = `${apiUrl}/auth/google/start?returnUrl=${encodeURIComponent(returnUrl)}`;
    window.location.assign(url);
  }

  async function handleSendOtp(e: React.FormEvent) {
    e.preventDefault();
    const destinationNumber = buildE164();
    if (!destinationNumber) {
      toast.error('Informe DDI e número.');
      return;
    }
    setLoading(true);
    try {
      const res = await authApi.requestOtp(destinationNumber);
      if (!res.success) throw new Error(res.message || 'Falha ao enviar código');
      setOtpSent(true);
      toast.success('Código enviado no WhatsApp.');
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : 'Falha ao enviar código.');
    } finally {
      setLoading(false);
    }
  }

  async function handleVerifyOtp(e: React.FormEvent) {
    e.preventDefault();
    const destinationNumber = buildE164();
    if (!destinationNumber) {
      toast.error('Informe DDI e número.');
      return;
    }
    setLoading(true);
    try {
      const res = await authApi.verifyOtp(destinationNumber, code);
      if (!res.success || !res.data) throw new Error(res.message || 'Falha ao validar código');
      setSession(res.data);
      toast.success('Login realizado.');
      navigate(redirectTo, { replace: true });
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : 'Falha ao validar código.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-screen bg-stone-950 flex items-center justify-center p-6">
      <div className="w-full max-w-md">
        <div className="flex items-center justify-center gap-3 mb-8">
          <div className="w-12 h-12 bg-amber-600 rounded-2xl flex items-center justify-center">
            <Rocket size={22} className="text-stone-950" />
          </div>
          <div>
            <h1 className="font-serif text-2xl font-bold text-amber-100">BookWise</h1>
            <p className="text-sm text-amber-600/70">Entrar na sua conta</p>
          </div>
        </div>

        <div className="bg-stone-900/40 border border-amber-900/30 rounded-2xl p-6">
          <div className="flex gap-2 mb-6">
            <button
              type="button"
              onClick={() => setMode('otp')}
              className={`flex-1 px-4 py-2 rounded-xl text-sm font-medium transition ${
                mode === 'otp'
                  ? 'bg-amber-600 text-stone-950'
                  : 'bg-stone-950/40 text-amber-200/70 hover:bg-stone-950/60'
              }`}
            >
              <span className="inline-flex items-center justify-center gap-2">
                <MessageSquareText size={16} />
                WhatsApp
              </span>
            </button>
            <button
              type="button"
              onClick={() => setMode('google')}
              className={`flex-1 px-4 py-2 rounded-xl text-sm font-medium transition ${
                mode === 'google'
                  ? 'bg-amber-600 text-stone-950'
                  : 'bg-stone-950/40 text-amber-200/70 hover:bg-stone-950/60'
              }`}
            >
              <span className="inline-flex items-center justify-center gap-2">
                <Shield size={16} />
                Google
              </span>
            </button>
          </div>

          {mode === 'otp' ? (
            <div>
              <p className="text-sm text-amber-200/70 mb-4">
                Enviaremos um código via WhatsApp para confirmar o acesso.
              </p>

              {!otpSent ? (
                <form onSubmit={handleSendOtp} className="space-y-4">
                  <div>
                    <label className="block text-xs font-medium text-amber-200/70 mb-2">Número</label>
                    <div className="flex gap-3">
                      <div className="w-28">
                        <input
                          value={ddi}
                          onChange={(e) => {
                            const raw = e.target.value;
                            const digits = raw.replace(/[^\d]/g, '');
                            setDdi(digits ? `+${digits}` : '+');
                          }}
                          inputMode="tel"
                          placeholder="+55"
                          className="w-full px-4 py-3 rounded-xl bg-stone-950/40 border border-amber-900/30 text-amber-100 placeholder:text-amber-200/30 focus:outline-none focus:ring-2 focus:ring-amber-600/60"
                        />
                      </div>
                      <div className="flex-1">
                        <input
                          value={phoneNumber}
                          onChange={(e) => setPhoneNumber(e.target.value.replace(/[^\d]/g, ''))}
                          inputMode="tel"
                          placeholder="11999999999"
                          className="w-full px-4 py-3 rounded-xl bg-stone-950/40 border border-amber-900/30 text-amber-100 placeholder:text-amber-200/30 focus:outline-none focus:ring-2 focus:ring-amber-600/60"
                        />
                      </div>
                    </div>
                  </div>
                  <button
                    disabled={loading}
                    className="w-full px-4 py-3 rounded-xl bg-amber-600 text-stone-950 font-semibold hover:bg-amber-500 disabled:opacity-60 disabled:cursor-not-allowed"
                  >
                    {loading ? 'Enviando...' : 'Enviar código'}
                  </button>
                </form>
              ) : (
                <form onSubmit={handleVerifyOtp} className="space-y-4">
                  <div>
                    <label className="block text-xs font-medium text-amber-200/70 mb-2">Código</label>
                    <input
                      value={code}
                      onChange={(e) => setCode(e.target.value)}
                      placeholder="000000"
                      className="w-full px-4 py-3 rounded-xl bg-stone-950/40 border border-amber-900/30 text-amber-100 placeholder:text-amber-200/30 focus:outline-none focus:ring-2 focus:ring-amber-600/60"
                    />
                  </div>
                  <div className="flex gap-3">
                    <button
                      type="button"
                      onClick={() => {
                        setOtpSent(false);
                        setCode('');
                        setDdi('+55');
                        setPhoneNumber('');
                      }}
                      className="flex-1 px-4 py-3 rounded-xl bg-stone-950/40 text-amber-100 font-semibold hover:bg-stone-950/60 border border-amber-900/30"
                    >
                      Trocar número
                    </button>
                    <button
                      disabled={loading}
                      className="flex-1 px-4 py-3 rounded-xl bg-amber-600 text-stone-950 font-semibold hover:bg-amber-500 disabled:opacity-60 disabled:cursor-not-allowed"
                    >
                      {loading ? 'Validando...' : 'Entrar'}
                    </button>
                  </div>
                </form>
              )}
            </div>
          ) : (
            <div>
              <p className="text-sm text-amber-200/70 mb-4">Use sua conta Google para entrar.</p>

              <button
                type="button"
                onClick={handleGoogleRedirect}
                disabled={loading}
                className="w-full px-4 py-3 rounded-xl bg-stone-950/40 text-amber-100 font-semibold hover:bg-stone-950/60 border border-amber-900/30 disabled:opacity-60 disabled:cursor-not-allowed"
              >
                Continuar com Google
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
