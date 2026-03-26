import { Backend } from '$lib/ts/backend';
import type { PageLoad } from './$types';
import type { PlantPreview } from '../lib/ts/base-types';
export const prerender = false;

export const load: PageLoad = async ({ fetch }) => {
	return await Backend.serverFetchJsonResponse<PlantPreview[]>(fetch, "/plants/load-all", { method: "GET" });
};