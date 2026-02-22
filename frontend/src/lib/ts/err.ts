type ErrBase = {
    msg: string;
    code: ErrCode;
    details?: string;
    fixSuggestion?: string;
};

export type Err<T = never> =
    ErrBase & ([T] extends [never] ? {} : { additionalData: T });

export type ErrCode =
    | BackendErrCode
    //frontend exclusive
    | 'unexpected_backend_response'


type BackendErrCode =
    | 'unspecified'
    | 'not_implemented'
    | 'program_bug'
    | 'incorrect_format'
    | 'no_matching_endpoint'
export namespace ErrUtils {


    export function createUnknown(details?: string): Err {
        return {
            msg: "Unknown error",
            code: 'unspecified',
            details: details
        };

    }
    export function fromPlain(obj: any): Err | Err<unknown> {
        const base: Err = {
            msg: String(obj?.message ?? "Unknown error"),
            code: typeof obj?.code === "string" ? (obj.code as ErrCode) : "unspecified",
            details: typeof obj?.details === "string" ? obj.details : undefined,
            fixSuggestion: typeof obj?.fixSuggestion === "string" ? obj.fixSuggestion : undefined,
        };

        if ("additionalData" in (obj ?? {}) && obj.additionalData !== undefined) {
            return { ...base, additionalData: obj.additionalData } as Err<unknown>;
        }

        return base;
    }
    export function createWithStatusCode(msg: string, statusCode: number, fixSuggestion?: string): Err<{ statusCode: number }> {
        return {
            msg: msg,
            code: 'unexpected_backend_response',
            fixSuggestion: fixSuggestion,
            additionalData: { statusCode }
        };
    }
    export function ensureAdditionalData(err: ErrBase): err is Err<unknown> {
        return "additionalData" in err;
    }
}