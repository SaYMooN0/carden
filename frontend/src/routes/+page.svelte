<script>
	import AuthView from '$lib/components/AuthView.svelte';
	import CardenPage from './_c_page/CardenPage.svelte';
	import SignInFormPage from './_c_page/SignInFormPage.svelte';
</script>

<AuthView>
	{#snippet children(authState)}
		{#if authState.isAuthenticated}
			<CardenPage />
		{:else}
			<SignInFormPage />
		{/if}
	{/snippet}
</AuthView>
