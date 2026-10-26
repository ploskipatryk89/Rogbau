import { get, post } from './apiService';

export interface OrderItem {
  equipmentId: string;
  equipmentName: string;
  unit: string;
  quantity: number;
  sentQuantity: number;
  quantityAvailable: number;
  weightKg: number;
}

export interface OrderDetails {
  id: string;
  orderNumber: string;
  siteId: string;
  siteName: string;
  note: string | null;
  date: string;
  status: number;
  items: OrderItem[];
  transfers: { id: string; number: string; status: number }[];
}

export const orderStatusLabels: Record<number, string> = {
  1: 'Oczekuje na wysyłkę', 2: 'Wysłane w całości', 3: 'Anulowane', 4: 'Częściowo wysłane',
};

export const getOrders = () => get<OrderDetails[]>('/api/orders');
export const getOrderById = (id: string) => get<OrderDetails>(`/api/orders/${id}`);
export const createOrder = (data: {
  siteId: string; note?: string; items: { equipmentId: string; quantity: number }[];
}) => post<typeof data, string>('/api/orders/createOrder', data);
