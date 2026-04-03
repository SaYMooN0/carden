import { Backend, type BackendResponse } from '$lib/ts/backend';
import type { PageLoad } from './$types';
import type { StudyDeckLoadResponse } from './shared_types';
export const prerender = false;

export const load: PageLoad = async ({ fetch, params }): Promise<BackendResponse<StudyDeckLoadResponse>> => {
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
	return await Backend.serverFetchJsonResponse<StudyDeckLoadResponse>(fetch, `/plants/${plantId}/load-study`, { method: "GET" });
};