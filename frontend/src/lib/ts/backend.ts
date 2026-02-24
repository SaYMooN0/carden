import { ErrUtils, type Err } from "./err";
import { DateUtils } from "./utils/date-utils";

export type BackendResponse<T> =
    | { isSuccess: true; data: T }
    | { isSuccess: false; errs: Err[] };
export type BackendResponseVoid =
    | { isSuccess: true }
    | { isSuccess: false; errs: Err[] };

export namespace Backend {

    export async function fetchJsonResponse<T>(url: string, options: RequestInit): Promise<BackendResponse<T>> {
        try {

            const response = await fetch("/api" + url, {
                ...options,
                credentials: 'include'
            });
            if (response.ok) {
                const text = await response.text();
                const data = parseWithDates<T>(text);
                return { isSuccess: true, data };
            }
            const errs = await parseErrResponse(response);

            return { isSuccess: false, errs };

        } catch (e: any) {
            return {
                isSuccess: false,
                errs: createErrsFromException(e)
            };
        }
    }

    export async function fetchVoidResponse(url: string, options: RequestInit): Promise<BackendResponseVoid> {
        try {
            console.log(1);
            const response = await fetch("/api" + url, {
                ...options,
                credentials: 'include'
            });
            console.log(response);
            if (response.ok) {
                return { isSuccess: true };
            }

            const errs = await parseErrResponse(response);
            return { isSuccess: false, errs };

        } catch (e: any) {
            return {
                isSuccess: false,
                errs: createErrsFromException(e)
            };
        }
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
            if (response.ok) {
                const text = await response.text();
                const data = parseWithDates<T>(text);
                return { isSuccess: true, data };
            }

            const errs = await parseErrResponse(response);
            return { isSuccess: false, errs };

        } catch (e: any) {
            return {
                isSuccess: false,
                errs: createErrsFromException(e)
            };
        }
    }

    function parseWithDates<T>(json: string): T {
        return JSON.parse(json, (key, value) => {
            if (typeof value === 'string' && DateUtils.isoDateRegex.test(value)) {
                return new Date(value);
            }
            return value;
        });
    }

    async function parseErrResponse(response: Response): Promise<Err[]> {
        const contentType = response.headers.get("content-type");

        if (contentType?.includes("application/json")) {

            try {
                const json = await response.json();
                if (!Array.isArray(json.errs)) {
                    return [{
                        code: 'unexpected_backend_response',
                        msg: "Unexpected error",
                        details: `Expected 'errs' array in response, 'errs' field type: ${typeof json.errs}`,
                        fixSuggestion: "Try again later",
                    }];
                }
                return json.errs.map(ErrUtils.fromPlain);
            } catch {
                return [{
                    code: 'unexpected_backend_response',
                    msg: "Unexpected error",
                    details: "Failed to parse JSON error response",
                    fixSuggestion: "Try again later",
                }];
            }
        }
        // in case of server unexpected error
        const status = response.status;
        if (status === 404) {
            return [ErrUtils.createWithStatusCode(
                "Unexpected error: requested resource was not found (404)",
                status
            )];
        }
        if (status === 403) {
            return [ErrUtils.createWithStatusCode(
                "Unexpected error: no permission to access this resource (403)",
                status
            )];
        }
        if (status === 401) {
            return [ErrUtils.createWithStatusCode(
                "Unexpected error: not authorized (401)",
                status,
                "Please log in"
            )];
        }
        if (status >= 500) {
            return [ErrUtils.createWithStatusCode(
                `Unexpected error: Internal server error (${status})`,
                status,
                "Please try again later"
            )];
        }

        // fallback for everything else
        return [ErrUtils.createWithStatusCode(
            `Unexpected error: Unexpected non-JSON error response (status ${status})`,
            status,
            "Please try again later"
        )];
    }
    function createErrsFromException(e: unknown): Err[] {
        if (e instanceof TypeError && e.message === "Failed to fetch") {
            // Server doesn't respond or no connection
            return [{
                code: 'unexpected_backend_response',
                msg: "Could not connect to the server. Please check your connection or try again later",
                fixSuggestion: "Please try again later"
            }];
        }

        if (e instanceof DOMException && e.name === "AbortError") {
            // Request was aborted
            return [{
                code: 'unexpected_backend_response',
                msg: "The request was aborted",
                fixSuggestion: "Please try again later"
            }];
        }

        return [{
            code: 'unexpected_backend_response',
            msg: "Unexpected error",
            fixSuggestion: "Please try again later"
        }];
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