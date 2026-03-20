<script lang="ts">
	import CardenPage from './_c_page/CardenPage.svelte';
	import SignInFormPage from './_c_page/SignInFormPage.svelte';
	import type { PageProps } from './$types';
	import DefaultErrBlock from '$lib/components/errs/DefaultErrBlock.svelte';

	let { data }: PageProps = $props();
</script>

<div class="fade-in-with-delay">
	{#if data.isSuccess}
		<CardenPage />
	{:else if data.statusCode === 401}
		<SignInFormPage />
	{:else}
		<DefaultErrBlock errs={data.errs} />
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
