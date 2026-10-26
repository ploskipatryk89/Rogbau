// src/pages/LoginPage.tsx

import { LogoHeader } from '../components/auth/LogoHeader';
import { LoginForm } from '../components/auth/LoginForm';
import type React from 'react';

export const LoginPage: React.FC = () => {
  return (
    <div className="flex min-h-screen w-full bg-white">
      <LogoHeader />
      <LoginForm />
    </div>
  );
}