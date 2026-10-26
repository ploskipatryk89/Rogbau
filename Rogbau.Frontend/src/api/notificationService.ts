import type { NotificationResponse } from "../types/response/NotificationResponse";
import { get, patch, del } from "./apiService";


export async function getMyNotifications(): Promise<NotificationResponse[]> {
  return await get<NotificationResponse[]>('/api/notifications/my');
}

export async function markAsRead(id: string): Promise<void> {
    return await patch<void>(`/api/notifications/${id}/mark-as-read`);
}

export const deleteNotification = (id: string) => del<void>(`/api/notifications/${id}`);
