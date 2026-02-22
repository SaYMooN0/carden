import { browser } from "$app/environment";

export namespace DateUtils {

	export const isoDateRegex: RegExp =
		/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?$/;


}
