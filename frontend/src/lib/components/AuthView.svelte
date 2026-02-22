<script lang="ts">
	import { AuthStore } from '$lib/ts/stores/auth-store.svelte';
	import type { Snippet } from 'svelte';

	interface Props {
		children: Snippet<[AuthStore.AuthState]>;
	}
	let { children }: Props = $props();
	let authState = AuthStore.Get();
</script>

{@render children(authState)}
