export interface CurrentUser {
  id:string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  initials: string;
  role: string;
  mustChangePassword: boolean;
}