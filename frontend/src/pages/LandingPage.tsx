import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Rocket, Search, Sparkles, Shield, ArrowRight, BookOpen, Zap } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';

export default function LandingPage() {
  const navigate = useNavigate();
  const { token, initializing } = useAuth();

  React.useEffect(() => {
    if (!initializing && token) navigate('/app', { replace: true });
  }, [initializing, token, navigate]);

  return (
    <div className="min-h-screen bg-stone-950">
      <header className="border-b border-amber-900/30 bg-stone-950/70 backdrop-blur">
        <div className="max-w-6xl mx-auto px-6 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-11 h-11 bg-amber-600 rounded-2xl flex items-center justify-center">
              <Rocket size={20} className="text-stone-950" />
            </div>
            <div>
              <h1 className="font-serif text-lg font-bold text-amber-100 leading-tight">BookWise</h1>
              <p className="text-xs text-amber-600/70">Catálogo inteligente. Sem desculpa.</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Link to="/login" className="btn-secondary hidden sm:inline-flex">
              Entrar
            </Link>
            <Link to="/login" className="btn-primary inline-flex items-center gap-2">
              Começar agora <ArrowRight size={16} />
            </Link>
          </div>
        </div>
      </header>

      <main>
        <section className="relative overflow-hidden">
          <div className="absolute inset-0 pointer-events-none">
            <div className="absolute -top-24 -left-24 w-96 h-96 bg-amber-600/10 blur-3xl rounded-full" />
            <div className="absolute -bottom-28 -right-28 w-[520px] h-[520px] bg-amber-500/5 blur-3xl rounded-full" />
          </div>

          <div className="max-w-6xl mx-auto px-6 py-16 md:py-20 relative">
            <div className="grid md:grid-cols-2 gap-10 items-center">
              <div>
                <p className="inline-flex items-center gap-2 text-xs font-semibold tracking-wide uppercase text-amber-400 bg-amber-900/20 border border-amber-900/30 rounded-full px-3 py-1">
                  <Zap size={14} />
                  Chega de acervo perdido
                </p>
                <h2 className="font-serif text-4xl md:text-5xl font-bold text-amber-100 mt-4 leading-tight">
                  Organize seus livros. Controle seu catálogo. Pare de improvisar.
                </h2>
                <p className="text-amber-200/70 mt-5 text-base md:text-lg leading-relaxed">
                  BookWise é um catálogo inteligente para quem leva leitura a sério. Busca remota, importação rápida,
                  autores e gêneros em ordem — e I.A. pra acelerar decisões.
                </p>

                <div className="flex flex-col sm:flex-row gap-3 mt-8">
                  <Link to="/login" className="btn-primary inline-flex items-center justify-center gap-2">
                    Quero acesso <ArrowRight size={16} />
                  </Link>
                  <a href="#features" className="btn-secondary inline-flex items-center justify-center gap-2">
                    Ver por que funciona <ArrowRight size={16} />
                  </a>
                </div>

                <div className="mt-8 grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div className="bg-stone-900/30 border border-amber-900/20 rounded-2xl p-4">
                    <p className="text-amber-100 font-semibold">Catálogo</p>
                    <p className="text-xs text-amber-200/60 mt-1">livros, autores, gêneros</p>
                  </div>
                  <div className="bg-stone-900/30 border border-amber-900/20 rounded-2xl p-4">
                    <p className="text-amber-100 font-semibold">Importação</p>
                    <p className="text-xs text-amber-200/60 mt-1">busca remota unificada</p>
                  </div>
                  <div className="bg-stone-900/30 border border-amber-900/20 rounded-2xl p-4">
                    <p className="text-amber-100 font-semibold">I.A.</p>
                    <p className="text-xs text-amber-200/60 mt-1">recomendações e análises</p>
                  </div>
                </div>
              </div>

              <div className="bg-stone-900/20 border border-amber-900/30 rounded-3xl p-6 md:p-8">
                <div className="flex items-center justify-between">
                  <p className="text-sm font-semibold text-amber-100">Antes vs. Depois</p>
                  <p className="text-xs text-amber-200/50">sem drama</p>
                </div>

                <div className="mt-6 space-y-4">
                  <div className="bg-stone-950/40 border border-amber-900/20 rounded-2xl p-4">
                    <p className="text-xs text-amber-200/60">Antes</p>
                    <p className="text-amber-100 font-medium mt-1">
                      “Eu tenho esse livro?” “Qual edição?” “Quem é o autor mesmo?” “Em que gênero eu coloquei?”
                    </p>
                    <p className="text-xs text-amber-200/50 mt-2">
                      Resultado: você compra repetido, perde tempo e deixa o catálogo virar bagunça.
                    </p>
                  </div>

                  <div className="bg-amber-900/20 border border-amber-600/30 rounded-2xl p-4">
                    <p className="text-xs text-amber-200/70">Depois</p>
                    <p className="text-amber-100 font-semibold mt-1">
                      Um lugar só pra consultar, importar, classificar e evoluir sua biblioteca.
                    </p>
                    <div className="mt-3 flex flex-wrap gap-2">
                      <span className="text-xs text-amber-100 bg-stone-950/40 border border-amber-900/30 rounded-full px-3 py-1">
                        Busca rápida
                      </span>
                      <span className="text-xs text-amber-100 bg-stone-950/40 border border-amber-900/30 rounded-full px-3 py-1">
                        Importação em segundos
                      </span>
                      <span className="text-xs text-amber-100 bg-stone-950/40 border border-amber-900/30 rounded-full px-3 py-1">
                        I.A. pra acelerar
                      </span>
                    </div>
                  </div>
                </div>

                <div className="mt-6">
                  <Link to="/login" className="btn-primary w-full inline-flex items-center justify-center gap-2">
                    Entrar e organizar tudo <ArrowRight size={16} />
                  </Link>
                  <p className="text-xs text-amber-200/50 mt-3 text-center">
                    Você só precisa entrar. O resto a gente resolve.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </section>

        <section id="features" className="border-t border-amber-900/20">
          <div className="max-w-6xl mx-auto px-6 py-14">
            <div className="flex items-end justify-between gap-6 flex-wrap">
              <div>
                <h3 className="font-serif text-2xl md:text-3xl font-bold text-amber-100">O que você ganha</h3>
                <p className="text-amber-200/70 mt-2 max-w-2xl">
                  Funcionalidade útil, sem firula. Pra você catalogar rápido e decidir melhor.
                </p>
              </div>
              <Link to="/login" className="btn-secondary inline-flex items-center gap-2">
                Quero ver no app <ArrowRight size={16} />
              </Link>
            </div>

            <div className="grid md:grid-cols-3 gap-4 mt-8">
              <FeatureCard
                icon={<Search size={18} />}
                title="Busca remota e importação"
                description="Encontre livros fora do seu catálogo e importe pro sistema sem perder tempo."
              />
              <FeatureCard
                icon={<BookOpen size={18} />}
                title="Catálogo com estrutura"
                description="Livros, autores e gêneros organizados do jeito certo, pra você consultar em segundos."
              />
              <FeatureCard
                icon={<Sparkles size={18} />}
                title="I.A. de verdade"
                description="Recomendações, sinopses e análises pra você evoluir o catálogo e tomar decisão mais rápido."
              />
            </div>

            <div className="grid md:grid-cols-2 gap-4 mt-4">
              <MiniCard
                icon={<Shield size={18} />}
                title="Acesso protegido"
                description="Rotas do app ficam protegidas; você entra e segue direto pro que importa."
              />
              <MiniCard
                icon={<Rocket size={18} />}
                title="Rápido de começar"
                description="Entre com WhatsApp ou Google e comece a montar seu catálogo hoje."
              />
            </div>
          </div>
        </section>

        <section className="border-t border-amber-900/20">
          <div className="max-w-6xl mx-auto px-6 py-14">
            <div className="bg-stone-900/20 border border-amber-900/30 rounded-3xl p-8 md:p-10 flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
              <div>
                <h4 className="font-serif text-2xl font-bold text-amber-100">Seu catálogo não vai se organizar sozinho.</h4>
                <p className="text-amber-200/70 mt-2 max-w-2xl">
                  Se você quer uma biblioteca que dá orgulho de mostrar (e fácil de manter), entra no BookWise agora.
                </p>
              </div>
              <Link to="/login" className="btn-primary inline-flex items-center gap-2">
                Entrar <ArrowRight size={16} />
              </Link>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
}

function FeatureCard({
  icon,
  title,
  description,
}: {
  icon: React.ReactNode;
  title: string;
  description: string;
}) {
  return (
    <div className="bg-stone-900/20 border border-amber-900/30 rounded-3xl p-6">
      <div className="w-10 h-10 bg-amber-600/20 border border-amber-600/30 rounded-2xl flex items-center justify-center text-amber-200">
        {icon}
      </div>
      <p className="text-amber-100 font-semibold mt-4">{title}</p>
      <p className="text-sm text-amber-200/70 mt-2 leading-relaxed">{description}</p>
    </div>
  );
}

function MiniCard({
  icon,
  title,
  description,
}: {
  icon: React.ReactNode;
  title: string;
  description: string;
}) {
  return (
    <div className="bg-stone-900/10 border border-amber-900/20 rounded-3xl p-6 flex gap-4 items-start">
      <div className="w-10 h-10 bg-stone-950/40 border border-amber-900/30 rounded-2xl flex items-center justify-center text-amber-200">
        {icon}
      </div>
      <div>
        <p className="text-amber-100 font-semibold">{title}</p>
        <p className="text-sm text-amber-200/70 mt-1 leading-relaxed">{description}</p>
      </div>
    </div>
  );
}
