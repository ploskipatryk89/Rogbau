import { post } from "./apiService";
import type { LoginRequest } from "../types/request/LoginRequest";
import type { LoginResponse } from "../types/response/LoginResponse";
import { get } from "./apiService";
import type { CurrentUser } from "../types/currentUser";

export async function login(data: LoginRequest): Promise<LoginResponse> {
  return await post<LoginRequest, LoginResponse>("/api/auth/login", data);
}
export async function getCurrentUser(): Promise<CurrentUser> {
  return await get<CurrentUser>("/api/auth/me");
}

