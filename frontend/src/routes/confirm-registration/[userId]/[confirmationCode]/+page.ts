import { Backend, RJO } from "$lib/ts/backend";
import type { PageLoad } from "./$types";

export const prerender = false;
export const load: PageLoad = async ({ fetch, params }) => {
    return await Backend.serverFetchJsonResponse<{ confirmedEmail: string }>(fetch, "/auth/confirm-registration", RJO.POST({
        userId: params.userId,
        confirmationCode: params.confirmationCode
    }));
};