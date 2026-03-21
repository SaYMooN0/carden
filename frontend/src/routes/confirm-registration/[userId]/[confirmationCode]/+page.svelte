<script lang="ts">
	import type { PageProps } from './$types';
	import DefaultErrBlock from '$lib/components/errs/DefaultErrBlock.svelte';

	let { data }: PageProps = $props();
</script>

<div class="view-container">
	{#if data.isSuccess}
		<p>
			You have successfully confirmed your email. Now you can login with <span class="email"
				>{data.data.confirmedEmail}</span
			>
		</p>
		<a href="/login">Go to login page</a>
	{:else}
		<h1 class="error-h">Something went wrong</h1>
		<DefaultErrBlock errs={data.errs} />
	{/if}
</div>

<style>
	.view-container {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		margin: 10rem auto 0;
		max-width: 40rem;
		gap: 1rem;
	}
	h1 {
		font-size: 2.5rem;
		font-weight: 500;
		text-align: center;
		letter-spacing: 0.5px;
	}
</style>
