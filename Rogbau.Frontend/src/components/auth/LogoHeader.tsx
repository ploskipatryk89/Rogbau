import loginBg from '../../assets/login-bg.jpg';

import { Logo } from '../ui/Logo';

export function LogoHeader() {
  return (
    <div className="relative hidden w-1/2 flex-col justify-between overflow-hidden p-12 lg:flex">
      <img
        src={loginBg}
        alt="Magazyn Rogbau"
        className="absolute inset-0 h-full w-full object-cover object-center"
      />
      <div className="absolute inset-0 bg-[linear-gradient(to_top,var(--color-slate-950),rgba(15,23,42,0.8),rgba(15,23,42,0.6))]" />

      <div className="relative z-10 flex items-center gap-3">
        <Logo />
      </div>

      <div className="relative z-10 space-y-6 max-w-xl">
        <div className="inline-flex items-center gap-2 rounded-full bg-amber-400/10 px-3.5 py-1.5 text-xs font-semibold text-amber-400 border border-amber-400/20 backdrop-blur-md">
          <span className="h-2 w-2 rounded-full bg-amber-400 animate-pulse" />
          System Zarządzania Magazynem, Sprzętem i budowami        </div>

        <h1 className="text-4xl font-extrabold tracking-tight text-white leading-tight lg:text-5xl">
          Zarządzaj magazynem i budowami <span className="text-amber-400">w jednym miejscu.</span>
        </h1>

        <p className="text-base text-slate-300 leading-relaxed">
          Kompleksowy system obsługi dokumentów WZ/PZ, ewidencji parku maszynowego oraz kontroli wydań i przyjęćna inwestycjach budowlanych.
        </p>

        <div className="grid grid-cols-2 gap-4 pt-4 border-t border-slate-800/80">
          <div>
            <p className="text-xl font-bold text-white">100%</p>
            <p className="text-xs text-slate-400">Kontroli nad sprzętem</p>
          </div>
          <div>
            <p className="text-xl font-bold text-white">Szybkie WZ i PZ</p>
            <p className="text-xs text-slate-400">Generowanie wydań i przyjęć</p>
          </div>
        </div>
      </div>

      <div className="relative z-10 text-xs text-slate-400">
        © {new Date().getFullYear()} Rogbau Sp. z o.o. Wszelkie prawa zastrzeżone.
      </div>
    </div>
  );
}