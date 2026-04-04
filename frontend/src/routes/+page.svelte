<script lang="ts">
	import SignInFormPage from './_c_page/SignInFormPage.svelte';
	import type { PageProps } from './$types';
	import UserPlantsDisplay from './_c_page/UserPlantsDisplay.svelte';
	import PageLoadErr from '$lib/components/PageLoadErr.svelte';

	let { data }: PageProps = $props();
</script>

<div class="fade-in-with-delay">
	{#if data.isSuccess}
		<UserPlantsDisplay loadedPlants={data.data} />
	{:else if data.statusCode === 401}
		<SignInFormPage />
	{:else}
		<PageLoadErr errs={data.errs} />
	{/if}
</div>

<style>
	.fade-in-with-delay {
		opacity: 0;
		animation: fadeIn 0.2s ease-in-out 0.05s forwards;
	}

	@keyframes fadeIn {
		0% {
			opacity: 0;
		}
		100% {
			opacity: 1;
		}
	}
</style>
