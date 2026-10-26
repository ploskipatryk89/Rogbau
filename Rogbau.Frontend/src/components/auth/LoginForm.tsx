import { useState } from 'react';
import { useNavigate } from 'react-router-dom'; // 👈 Dodany import
import { FormField } from '../ui/FormField';
import { login } from '../../api/authService';
import { useAuth } from '../../context/AuthContext';
import { getCurrentUser } from '../../api/authService';
import { ChangePasswordByUserModal } from '../users/ChangePasswordByUserModal';

export function LoginForm() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const {setUser, logout} = useAuth();
  const [isChangePasswordModalOpen, setIsChangePasswordModalOpen] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  const navigate = useNavigate(); // 👈 Inicjalizacja hooka do nawigacji
const handlePasswordChangeSuccess = () => {
    logout();
    setIsChangePasswordModalOpen(false);
    setEmail('');
    setPassword('');
    setSuccessMessage("Hasło zostało zmienione. Możesz teraz zalogować się przy użyciu nowego hasła.");
};
async function handleLogin(e: React.FormEvent) {
  e.preventDefault();

  try {
    setErrorMessage("");
    setIsLoading(true);

    const response = await login({
      email,
      password
    });

    sessionStorage.setItem("accessToken", response.token);

    const currentUser = await getCurrentUser();

    setUser(currentUser);

   if (currentUser.mustChangePassword) {
    setIsChangePasswordModalOpen(true);
    return;
}
    navigate("/dashboard");

  } catch (error) {
    console.error("LOGIN ERROR:", error);

    if (error instanceof Error) {
      setErrorMessage(error.message);
    }
  } finally {
    setIsLoading(false);
  }
}

  return (
    <div className="flex w-full items-center justify-center p-8 lg:w-1/2">
      <div className="w-full max-w-md space-y-8">
        
        {/* Nagłówek */}
        <div className="space-y-2 text-left">
          <h2 className="text-3xl font-extrabold tracking-tight text-slate-900">
            Zaloguj się
          </h2>
          <p className="text-sm text-slate-500">
            Wprowadź swoje dane, aby uzyskać dostęp do panelu magazynowego.
          </p>
        </div>

        {/* Błąd logowania */}
        {errorMessage && (
          <div className="rounded-lg bg-red-50 p-4 text-sm text-red-600 border border-red-200">
            {errorMessage}
          </div>
        )}

         {successMessage && (
          <div className="rounded-lg bg-green-50 p-4 text-sm text-green-600 border border-green-200">
            {successMessage}
          </div>
        )}

        {/* Formularz */}
        <form onSubmit={handleLogin} className="space-y-6">
          
          {/* Pole Email */}
          <FormField
            label="Adres E-mail / Login"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="jan.kowalski@rogbau.pl"
            required
          />

          {/* Pole Hasło */}
          <FormField
            label="Hasło"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            required
          />

          {/* Przycisk */}
          <button
            type="submit"
            disabled={isLoading}
            className="w-full rounded-xl bg-amber-400 py-3.5 text-sm font-bold text-slate-950 shadow-md transition-all hover:bg-amber-300 active:scale-[0.99] disabled:opacity-50 cursor-pointer"
          >
            {isLoading ? 'Logowanie...' : 'Zaloguj się do systemu'}
          </button>
        </form>

      </div>

        <ChangePasswordByUserModal
        isOpen = {isChangePasswordModalOpen}
        onClose={() => setIsChangePasswordModalOpen(false)}
        onSuccess={handlePasswordChangeSuccess}
        title='Zmiana hasła'
        subTitle='Przed kontynuowaniem musisz zmienić hasło tymczasowe nadane przez Admina'
        email={email}
        />

    </div>
  );
}
