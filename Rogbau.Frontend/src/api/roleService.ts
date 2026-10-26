import { get } from "./apiService";
import type { RoleResponseDto } from "../types/response/RoleDto";

export async function getAllRoles(): Promise<RoleResponseDto[]> {
    return get<RoleResponseDto[]>('/api/roles');
}