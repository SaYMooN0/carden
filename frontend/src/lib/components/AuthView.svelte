<script lang="ts">
	import { AuthStore } from '$lib/ts/stores/auth-store.svelte';
	import { onMount, type Snippet } from 'svelte';

	interface Props {
		children: Snippet<[AuthStore.AuthState]>;
	}
	let { children }: Props = $props();
	let authState: AuthStore.AuthState = $state({ name: 'loading', isAuthenticated: false });
	onMount(() => {
		authState = AuthStore.Get();
	});
</script>

{@render children(authState)}
