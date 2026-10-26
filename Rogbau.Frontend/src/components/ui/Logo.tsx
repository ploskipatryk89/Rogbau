import type React from "react";
import logoImg from '../../assets/logo.png';

export const Logo: React.FC = () =>{
    return(
        <div className="h-16 flex items-center px-6  border-slate-800/80">
        <img 
          src={logoImg} 
          alt="Rogbau Logo" 
          className="h-16 w-auto max-w-full object-contain" 
        />
      </div>
    );
}