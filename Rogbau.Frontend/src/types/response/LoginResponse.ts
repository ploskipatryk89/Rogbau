export interface LoginResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  mustChangePassword: boolean;
  fullName: string;
  initials: string;
  role: string;
}