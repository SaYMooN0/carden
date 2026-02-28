export type BaseErr = {
    msg: string;
    details?: string;
    fixSuggestion?: string;
};

type TypedAdditionalData<T extends keyof ErrAdditionalDataMap> = {
    id: T;
    data: ErrAdditionalDataMap[T];
};
export type Err<T extends keyof ErrAdditionalDataMap | undefined = undefined> =
    BaseErr &
    (T extends keyof ErrAdditionalDataMap
        ? { extraData: TypedAdditionalData<T> }
        : { extraData?: undefined });

export namespace ErrUtils {


    export function createUnknown(details?: string): Err {
        return {
            msg: "Unknown error",
            details: details
        };

    }
    // export function fromPlain(obj: any): Err | Err<unknown> {
    //     const base: Err = {
    //         msg: String(obj?.message ?? "Unknown error"),
    //         details: typeof obj?.details === "string" ? obj.details : undefined,
    //         fixSuggestion: typeof obj?.fixSuggestion === "string" ? obj.fixSuggestion : undefined,
    //     };

    //     if ("additionalData" in (obj ?? {}) && obj.additionalData !== undefined) {
    //         return { ...base, additionalData: obj.additionalData } as Err<unknown>;
    //     }

    //     return base;
    // }
    // export function createForStatusCode(msg: string, statusCode: number, fixSuggestion?: string): Err<"UNEXPECTED_BACKEND_RESPONSE"> {
    //     return {
    //         msg: msg,
    //         fixSuggestion: fixSuggestion,
    //         additionalData: {
    //             id: "UNEXPECTED_BACKEND_RESPONSE",
    //             data: { statusCode: statusCode }
    //         }
    //     };
    // }
    export function hasAdditionalData(err: BaseErr): boolean {
        return "additionalData" in err && err.additionalData !== undefined && 'id' in (err.additionalData as any);
    }
    export function ensureAdditionalData<T extends keyof ErrAdditionalDataMap>(err: BaseErr, id: T): err is Err<T> {
        return hasAdditionalData(err) && (err as any).additionalData.id === id;
    }
}
type ErrAdditionalDataMap = {
    NO_MATCHED_ENDPOINT: {
        method: string;
        route: string;
    };
    SERVER_EXCEPTION: {
        exception: string;
    };
    FAILED_TO_FETCH: {
        exception: string;
    };
};