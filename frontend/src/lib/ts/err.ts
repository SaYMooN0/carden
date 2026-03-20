export type BaseErr = {
    msg: string;
    details?: string;
    fixSuggestion?: string;
};

type TypedExtraData<T extends keyof ErrExtraDataMap> = {
    id: T;
    data: ErrExtraDataMap[T];
};
export type Err<T extends keyof ErrExtraDataMap | undefined = undefined> =
    BaseErr &
    (T extends keyof ErrExtraDataMap
        ? { extraData: TypedExtraData<T> }
        : { extraData?: undefined });

export namespace ErrUtils {

    export function createUnknown(details?: string): Err {
        return {
            msg: "Unknown error",
            details: details
        };

    }

    export function hasExtraData(err: BaseErr): boolean {
        return "extraData" in err && err.extraData !== undefined && 'id' in (err.extraData as any);
    }
    export function ensureExtraData<T extends keyof ErrExtraDataMap>(err: BaseErr, id: T): err is Err<T> {
        return hasExtraData(err) && (err as any).extraData.id === id;
    }
}
type ErrExtraDataMap = {
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