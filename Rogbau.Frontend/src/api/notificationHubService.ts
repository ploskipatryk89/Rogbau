import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState
} from "@microsoft/signalr";

import { API_BASE_URL } from "./config";
import type { NotificationResponse } from "../types/response/NotificationResponse";

class NotificationHubService {

  private connection: HubConnection | null = null;

  public async start(
    onNotificationReceived: (notification: NotificationResponse) => void,
    onReconnected?: () => void
  ): Promise<void> {

    if (
      this.connection &&
      this.connection.state !== HubConnectionState.Disconnected
    ) {
      console.log(
        "SignalR: połączenie już istnieje:",
        this.connection.state
      );

      return;
    }

    const token = sessionStorage.getItem("accessToken");

    if (!token) {
      console.log("SignalR: brak tokena");
      return;
    }

    const serverRootUrl =
      API_BASE_URL.replace(/\/api\/?$/, "");

    const connection = new HubConnectionBuilder()
      .withUrl(`${serverRootUrl}/hubs/notifications`, {
        accessTokenFactory: () =>
          sessionStorage.getItem("accessToken") ?? ""
      })
      .withAutomaticReconnect()
      .build();

    connection.on(
      "ReceiveNotification",
      (notification: NotificationResponse) => {

        console.log(
          "Odebrano powiadomienie przez SignalR:",
          notification
        );

        onNotificationReceived(notification);
      }
    );

    this.connection = connection;
    connection.onreconnected(() => onReconnected?.());

    try {

      await connection.start();

      console.log(
        "SignalR połączony!",
        connection.connectionId
      );

    } catch (error) {

      console.error(
        "Błąd połączenia SignalR:",
        error
      );

      if (this.connection === connection) {
        this.connection = null;
      }
    }
  }

  public async stop(): Promise<void> {

    const connection = this.connection;

    if (!connection) {
      return;
    }

    this.connection = null;

    try {

      await connection.stop();

      console.log("SignalR rozłączony");

    } catch (error) {

      console.error(
        "Błąd rozłączania SignalR:",
        error
      );
    }
  }
}

export const notificationHub =
  new NotificationHubService();
