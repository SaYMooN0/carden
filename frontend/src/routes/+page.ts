import { Backend, RJO } from '$lib/ts/backend';
import type { PageLoad } from './$types';
import type { PlantPreview } from './types';
export const prerender = false;

export const load: PageLoad = async ({ fetch }) => {
	console.log("--------------------1111111");

	const response = await Backend.serverFetchJsonResponse<PlantPreview[]>(fetch, "/plants/load-all", { method: "GET" });

	console.log("11111111");
	console.log(response);
	return response;
};