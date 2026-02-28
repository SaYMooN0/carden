import { type BaseErr, type Err } from "./err";

export type BackendResponse<T> =
    | { isSuccess: true; data: T }
    | { isSuccess: false; errs: BaseErr[] };

export type BackendResponseVoid = BackendResponse<void>;

export namespace Backend {
    export async function fetchJsonResponse<T>(url: string, options: RequestInit): Promise<BackendResponse<T>> {
        return serverFetchJsonResponse<T>(fetch, url, options);
    }

    export async function serverFetchJsonResponse<T>(
        fetchFunc: typeof fetch,
        url: string,
        options: RequestInit
    ): Promise<BackendResponse<T>> {
        try {

            const response = await fetchFunc("/api" + url, {
                ...options,
                credentials: 'include'
            });
            const text = await response.text();
            console.log(text);
            try {
                const data = JSON.parse(text) as BackendResponse<T>;
                return data;
            } catch (e: any) {
                return {
                    isSuccess: false,
                    errs: [createErrFromResponseWhenNotDeserializable(response)]
                };
            }

        } catch (e: any) {
            console.log(e);

            return {
                isSuccess: false,
                errs: [createErrFromException(e)]
            };
        }
    }
    function createErrFromResponseWhenNotDeserializable(response: Response): Err<"FAILED_TO_FETCH"> {

        const status = response.status;
        if (status === 404) {
            return {
                msg: "Unexpected error: requested resource was not found (404)",
                fixSuggestion: "Please try again later",
                extraData: {
                    id: "FAILED_TO_FETCH",
                    data: { exception: response.statusText }
                }
            };
        }
        if (status === 403) {
            return {
                msg: "Unexpected error: no permission to access this resource (403)",
                fixSuggestion: "Please try again later",
                extraData: {
                    id: "FAILED_TO_FETCH",
                    data: { exception: response.statusText }
                }
            };
        }
        if (status === 401) {
            return {
                msg: "Unexpected error: not authorized (401)",
                fixSuggestion: "Please log in",
                extraData: {
                    id: "FAILED_TO_FETCH",
                    data: { exception: response.statusText }
                }
            };
        }
        if (status >= 500) {
            return {
                msg: `Unexpected error: Internal server error (${status})`,
                fixSuggestion: "Please try again later",
                extraData: {
                    id: "FAILED_TO_FETCH",
                    data: { exception: response.statusText }
                }
            };
        }

        // fallback for everything else
        return {
            msg: `Unexpected error: Unexpected non-JSON error response (status ${status})`,
            fixSuggestion: "Please try again later",
            extraData: {
                id: "FAILED_TO_FETCH",
                data: { exception: response.statusText }
            }
        };

    }
    function createErrFromException(e: unknown): Err<"FAILED_TO_FETCH"> {
        if (e instanceof TypeError && e.message === "Failed to fetch") {
            // Server doesn't respond or no connection
            return {
                msg: "Could not connect to the server. Please check your connection or try again later",
                fixSuggestion: "Please try again later",
                extraData: {
                    id: "FAILED_TO_FETCH",
                    data: { exception: e.message }
                }
            };
        }

        if (e instanceof DOMException && e.name === "AbortError") {
            // Request was aborted
            return {
                msg: "The request was aborted",
                fixSuggestion: "Please try again later",
                extraData: {
                    id: "FAILED_TO_FETCH",
                    data: { exception: e.message }
                }
            };
        }

        return {
            msg: "Unexpected error",
            fixSuggestion: "Please try again later",
            extraData: {
                id: "FAILED_TO_FETCH",
                data: { exception: String(e) }
            }
        };
    }

}
export const RJO = {
    POST: (data: any): RequestInit => formRequestJsonOptions(data, "POST"),
    PUT: (data: any): RequestInit => formRequestJsonOptions(data, "PUT"),
    PATCH: (data: any): RequestInit => formRequestJsonOptions(data, "PATCH"),
    DELETE: (data: any): RequestInit => formRequestJsonOptions(data, "DELETE"),
};

function formRequestJsonOptions(data: any, method: string): RequestInit {
    return {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    };
}