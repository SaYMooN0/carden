<script lang="ts">
	interface Props {
		onclick: () => void;
		text: string;
		isLoading: boolean;
	}
	let { onclick, text, isLoading }: Props = $props();
</script>

<button
	class="confirm-btn unselectable"
	onclick={() => onclick()}
	type="submit"
	disabled={isLoading}
	class:loading={isLoading}
>
	{#if isLoading}
		Loading...
	{:else}
		{text}
	{/if}
</button>

<style>
	.confirm-btn {
		display: flex;
		flex-direction: row;
		justify-content: center;
		align-items: center;
		width: 100%;
		padding: 0.875rem 1rem;
		margin: 1.5rem auto 0;
		border: none;
		border-radius: 0.5rem;
		background-color: var(--primary);
		color: var(--primary-foreground);
		font-size: 1.125rem;
		font-weight: 500;
		box-shadow: var(--shadow);
		transition: all 0.2s ease;
		cursor: pointer;
	}

	.confirm-btn:not(.loading):hover {
		background-color: var(--primary-hov);
		transform: translateY(-1px);
	}

	.confirm-btn:not(.loading):active {
		transform: translateY(1px);
		box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
	}

	.confirm-btn.loading {
		opacity: 0.7;
		cursor: wait;
	}
</style>
