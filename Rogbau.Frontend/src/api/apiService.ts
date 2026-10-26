import { API_BASE_URL } from "./config";

function getToken(): string | null {
    const token = sessionStorage.getItem("accessToken");
    return token;
}

async function handleError(response: Response): Promise<never> {
    if (response.status === 401 && sessionStorage.getItem("accessToken")) {
        sessionStorage.removeItem("accessToken");
        window.dispatchEvent(new Event("session-expired"));
        throw new Error("Sesja wygasła. Zaloguj się ponownie.");
    }
    const contentType = response.headers.get("content-type");

    let errorMessage = `Błąd HTTP ${response.status}`;

    if (contentType?.includes("application/json")) {
        const errorData = await response.json().catch(() => ({}));

        // ASP.NET Core validation errors
        if (
            errorData.errors &&
            typeof errorData.errors === "object"
        ) {
            const messages: string[] = [];

            for (const key of Object.keys(errorData.errors)) {
                const fieldErrors = errorData.errors[key];

                if (Array.isArray(fieldErrors)) {
                    messages.push(...fieldErrors);
                } else if (typeof fieldErrors === "string") {
                    messages.push(fieldErrors);
                }
            }

            if (messages.length > 0) {
                errorMessage = messages.join(" | ");
            }
        } else {
            errorMessage =
                errorData.detail ||
                errorData.title ||
                errorData.message ||
                JSON.stringify(errorData);
        }
    } else {
        const textError = await response.text();

        if (textError) {
            errorMessage = textError;
        }
    }

    throw new Error(errorMessage);
}

async function parseResponse<TResponse>(
    response: Response
): Promise<TResponse> {

    if (!response.ok) {
        await handleError(response);
    }

    // 204 No Content
    if (response.status === 204) {
        return undefined as TResponse;
    }

    const text = await response.text();

    if (!text) {
        return {} as TResponse;
    }

    return JSON.parse(text) as TResponse;
}

export async function get<TResponse>(
    endpoint: string
): Promise<TResponse> {

    const token = getToken();
    const url = `${API_BASE_URL}${endpoint}`;


    try {
        const response = await fetch(url, {
            method: "GET",

            headers: {
                Accept: "application/json",

                ...(token
                    ? {
                        Authorization: `Bearer ${token}`
                    }
                    : {})
            }
        });


        return await parseResponse<TResponse>(response);

    } catch (error) {
        console.error("GET error:", error);

        if (error instanceof TypeError) {
            throw new Error(
                `Nie można połączyć się z API: ${url}`, { cause: error }
            );
        }

        throw error;
    }
}

export async function post<TRequest, TResponse>(
    endpoint: string,
    data: TRequest
): Promise<TResponse> {

    const token = getToken();
    const url = `${API_BASE_URL}${endpoint}`;


    try {
        const response = await fetch(url, {
            method: "POST",

            headers: {
                "Content-Type": "application/json",
                Accept: "application/json",

                ...(token
                    ? {
                        Authorization: `Bearer ${token}`
                    }
                    : {})
            },

            body: JSON.stringify(data)
        });


        return await parseResponse<TResponse>(response);

    } catch (error) {
        console.error("POST error:", error);

        if (error instanceof TypeError) {
            throw new Error(
                `Nie można połączyć się z API: ${url}`, { cause: error }
            );
        }

        throw error;
    }
}

export async function del<TResponse = void>(
    endpoint: string
): Promise<TResponse> {

    const token = getToken();
    const url = `${API_BASE_URL}${endpoint}`;


    try {
        const response = await fetch(url, {
            method: "DELETE",

            headers: {
                Accept: "application/json",

                ...(token
                    ? {
                        Authorization: `Bearer ${token}`
                    }
                    : {})
            }
        });


        return await parseResponse<TResponse>(response);

    } catch (error) {
        console.error("DELETE error:", error);

        if (error instanceof TypeError) {
            throw new Error(
                `Nie można połączyć się z API: ${url}`, { cause: error }
            );
        }

        throw error;
    }
}

export async function patch<TRequest = void, TResponse = void>(
    endpoint: string,
    data?: TRequest
): Promise<TResponse> {

    const token = getToken();
    const url = `${API_BASE_URL}${endpoint}`;


    try {
        const response = await fetch(url, {
            method: "PATCH",

            headers: {
                ...(data !== undefined
                    ? { "Content-Type": "application/json" }
                    : {}),

                Accept: "application/json",

                ...(token
                    ? {
                        Authorization: `Bearer ${token}`
                    }
                    : {})
            },

            ...(data !== undefined
                ? { body: JSON.stringify(data) }
                : {})
        });


        return await parseResponse<TResponse>(response);

    } catch (error) {
        console.error("PATCH error:", error);

        if (error instanceof TypeError) {
            throw new Error(
                `Nie można połączyć się z API: ${url}`, { cause: error }
            );
        }

        throw error;
    }
}

export async function put<TRequest, TResponse>(
    endpoint: string,
    data: TRequest
): Promise<TResponse> {

    const token = getToken();
    const url = `${API_BASE_URL}${endpoint}`;


    try {
        const response = await fetch(url, {
            method: "PUT",

            headers: {
                "Content-Type": "application/json",
                Accept: "application/json",

                ...(token
                    ? {
                        Authorization: `Bearer ${token}`
                    }
                    : {})
            },

            body: JSON.stringify(data)
        });


        return await parseResponse<TResponse>(response);

    } catch (error) {
        console.error("PUT error:", error);

        if (error instanceof TypeError) {
            throw new Error(
                `Nie można połączyć się z API: ${url}`, { cause: error }
            );
        }

        throw error;
    }
}
