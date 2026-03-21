<script lang="ts">
	import favicon from '$lib/assets/favicon.svg';
	import AuthView from '$lib/components/AuthView.svelte';
	import AppToaster from './_c_layout/AppToaster.svelte';
	import LayoutAccountButton from './_c_layout/LayoutAccountButton.svelte';

	let { children } = $props();
	let accountButton = $state<LayoutAccountButton | null>(null);
</script>

<svelte:window onclick={() => accountButton?.closeMenu()} />

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>
<AppToaster />
<div id="page-content">
	{@render children()}
	<AuthView>
		{#snippet children(authState)}
			{#if authState.isAuthenticated}
				<LayoutAccountButton bind:this={accountButton} />
			{/if}
		{/snippet}
	</AuthView>
</div>

<style>
	#page-content {
		width: 100%;
		height: 100%;
	}
</style>
