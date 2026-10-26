import type { CreateUserRequest } from "../types/request/CreateUserRequest";
import type { ChangePasswordByAdminRequest } from "../types/request/ChangePasswordByAdminRequest";
import type { ChangePasswordByUserRequest } from "../types/request/ChangePasswordByUserRequest";
import type { UserResponseDto } from "../types/response/UserResponseDto";

import { get, post, put, del } from "./apiService";
import type { EditUserRequest } from "../types/request/EditUserRequest";

export async function getAll(): Promise<UserResponseDto[]> {
    return get<UserResponseDto[]>("/api/users");
}

export async function create(data: CreateUserRequest): Promise<string> {
    return post<CreateUserRequest, string>("/api/users/create", data);
}

export async function deleteUser(id: string): Promise<void> {
    return del<void>(`/api/users/${id}`);
}

export async function changePassword(
    data: ChangePasswordByUserRequest
): Promise<void> {
    return put<ChangePasswordByUserRequest, void>(
        "/api/users/change-password",
        data
    );
}

export async function resetPassword(
    id: string,
    data: ChangePasswordByAdminRequest
): Promise<void> {
    return put<ChangePasswordByAdminRequest, void>(
        `/api/users/${id}/reset-password`,
        data
    );
}

export async function editUser(id: string, data: EditUserRequest) : Promise<void>{
    return put<EditUserRequest, void>(`/api/users/${id}`, data)
}