import type { Err } from "$lib/ts/err";
import { Backend, RJO } from "../backend";

export namespace AuthStore {
    export type AuthState =
        | { name: "loading"; isAuthenticated: false; }
        | { name: "error"; isAuthenticated: false; errs: Err[] }
        | { name: "authenticated"; isAuthenticated: true; userId: string }
        | { name: "unauthenticated"; isAuthenticated: false };

    const TTL_MS = 2 * 60 * 1000;

    const value = $state<AuthState>({ name: "loading", isAuthenticated: false });
    let expiresAt = 0;
    let inflight: Promise<void> | null = null;


    export function Get(): AuthState {
        ensureFresh(false);
        return value;
    }

    export async function GetWithForceRefresh(): Promise<AuthState> {
        ensureFresh(true);
        if (inflight) {
            await inflight;
        }
        return value;
    }

    function ensureFresh(force: boolean): void {
        if (inflight) {
            return;
        }

        const now = Date.now();
        const isAuthedAndFresh = value.name === "authenticated" && now < expiresAt;

        if (!force && isAuthedAndFresh) {
            return;
        }

        value.name = "loading";
        inflight = fetchAndApply().finally(() => { inflight = null; });
    }

    async function fetchAndApply(): Promise<void> {
        const response = await Backend.fetchJsonResponse<
            { userId: string; isAuthenticated: boolean }
        >("auth/ping", RJO.POST({}));

        if (response.isSuccess) {
            const { userId, isAuthenticated } = response.data;
            if (isAuthenticated && userId?.trim()) {
                value.name = "authenticated";
                (value as any).isAuthenticated = true;
                (value as any).userId = userId.trim();
                delete (value as any).errs;
                expiresAt = Date.now() + TTL_MS;
            } else {
                value.name = "unauthenticated";
                (value as any).isAuthenticated = false;
                delete (value as any).userId;
                delete (value as any).errs;
                expiresAt = 0;
            }
        } else {
            value.name = "error";
            (value as any).isAuthenticated = false;
            (value as any).errs = response.errs as Err[];
            delete (value as any).userId;
            expiresAt = 0;
        }
    }
}