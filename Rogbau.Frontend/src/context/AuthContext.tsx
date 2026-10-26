import { createContext, useState, useContext } from "react";
import type { ReactNode } from "react";
import type { CurrentUser } from "../types/currentUser";
import { useEffect } from "react";
import { getCurrentUser } from "../api/authService";

interface AuthContextType {
  user: CurrentUser | null;
  setUser: (user: CurrentUser | null) => void;
  logout: () => void;
  loading: boolean;
}

export const AuthContext = createContext<AuthContextType>({
  user: null,
  setUser: () => {},
  logout: () => {},
  loading: true
});

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadCurrentUser = async () => {
      const token = sessionStorage.getItem("accessToken");

      if (!token) {
        setLoading(false);
        return;
      }

      try {
        const currentUser = await getCurrentUser();
        setUser(currentUser);
      } catch (error) {
        console.error("Nie udało się pobrać użytkownika:", error);
        sessionStorage.removeItem("accessToken");
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    loadCurrentUser();
  }, []);

  useEffect(() => {
    const expireSession = () => {
      sessionStorage.removeItem("accessToken");
      setUser(null);
    };
    window.addEventListener("session-expired", expireSession);
    return () => window.removeEventListener("session-expired", expireSession);
  }, []);

  const logout = () => {
  sessionStorage.removeItem("accessToken");
  setUser(null);
};
  return (
    <AuthContext.Provider value={{ user, setUser, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  return useContext(AuthContext);
};
