import { Backend, type BackendResponse } from '$lib/ts/backend';
import type { Plant } from '$lib/ts/base-types';
import type { PageLoad } from './$types';
export const prerender = false;

export const load: PageLoad = async ({ fetch, params }): Promise<BackendResponse<Plant>> => {
	const plantId = params.plantId;
	if (!plantId) {
		return {
			isSuccess: false,
			errs: [
				{
					msg: "Invalid plant id",
					fixSuggestion: "Please try again later",
					extraData: {
						id: "INVALID_PLANT_ID",
						data: { plantId: plantId }
					}
				}
			],
			statusCode: 400
		};
	}
	return await Backend.serverFetchJsonResponse<Plant>(fetch, `/plants/load/${plantId}`, { method: "GET" });
};