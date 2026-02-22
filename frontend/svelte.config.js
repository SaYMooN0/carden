import adapter from '@sveltejs/adapter-auto';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

const config = {
	preprocess: vitePreprocess(),
	kit: {
		adapter: adapter()
	},
	compilerOptions: {
		warningFilter: (warning) => {
			const ignore = [
				'a11y_media_has_caption',
				'a11y_missing_attribute',
				'a11y_no_redundant_roles',
				'a11y_consider_explicit_label',
				'a11y_no_noninteractive_tabindex',
				'a11y_label_has_associated_control',
				'a11y_click_events_have_key_events',
				'a11y_no_static_element_interactions',
				'a11y_no_noninteractive_element_interactions',
				'a11y_img_redundant_alt',
				'element_invalid_self_closing_tag'

			];
			return !ignore.includes(warning.code);
		},
	},
	vitePlugin: {
		inspector: true
	},
};

export default config;

