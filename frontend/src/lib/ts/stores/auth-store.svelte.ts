import type { Err } from "$lib/ts/err";
import { Backend, RJO } from "../backend";

export namespace AuthStore {
    export type AuthState =
        | { name: "loading"; isAuthenticated: false; }
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
        const response = await Backend.fetchJsonResponse<{ userId: string }>("/auth/ping", RJO.POST({}));
        if (response.isSuccess) {
            const userId = response.data.userId;
            value.name = "authenticated";
            (value as any).isAuthenticated = true;
            (value as any).userId = userId.trim();
            expiresAt = Date.now() + TTL_MS;
        } else {
            value.name = "unauthenticated";
            (value as any).isAuthenticated = false;
            delete (value as any).userId;
            expiresAt = 0;
        }
    }
}