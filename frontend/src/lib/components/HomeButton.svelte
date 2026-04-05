<script lang="ts">
	interface Props {
		isCompact: boolean;
	}

	let { isCompact }: Props = $props();
</script>

<a class:compact={isCompact} class="garden-button" href="/" aria-label={'В сад'}>
	<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" stroke="currentColor" fill="none">
		<path
			d="M22 10.5L12.8825 2.82207C12.6355 2.61407 12.3229 2.5 12 2.5C11.6771 2.5 11.3645 2.61407 11.1175 2.82207L2 10.5"
		/>
		<path
			d="M20.5 9.5V16C20.5 18.3456 20.5 19.5184 19.8801 20.3263C19.7205 20.5343 19.5343 20.7205 19.3263 20.8801C18.5184 21.5 17.3456 21.5 15 21.5V17C15 15.5858 15 14.8787 14.5607 14.4393C14.1213 14 13.4142 14 12 14C10.5858 14 9.87868 14 9.43934 14.4393C9 14.8787 9 15.5858 9 17V21.5C6.65442 21.5 5.48164 21.5 4.67372 20.8801C4.46572 20.7205 4.27954 20.5343 4.11994 20.3263C3.5 19.5184 3.5 18.3456 3.5 16V9.5"
		/>
	</svg>

	{#if !isCompact}
		<span class="label">В сад</span>
	{/if}
</a>

<style>
	.garden-button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		gap: 0.5rem;
		min-height: 2.75rem;
		padding: 0 1rem;
		border-radius: 1rem;
		border: 0.0625rem solid var(--color-sage);
		background: var(--primary);
		color: var(--primary-foreground);
		box-shadow: var(--shadow);
		text-decoration: none;
		transition:
			background-color 0.2s ease,
			transform 0.2s ease,
			border-color 0.2s ease;
	}

	.garden-button:hover {
		background: var(--primary-hov);
	}

	.garden-button:active {
		transform: translateY(0.0625rem);
	}

	.garden-button:focus-visible {
		outline: 0.125rem solid var(--color-sage);
		outline-offset: 0.125rem;
	}

	svg {
		display: block;
		stroke-width: 2;
		stroke-linecap: round;
		stroke-linejoin: round;
		height: 1.25rem;
		width: 1.25rem;
	}

	.label {
		font-size: 1rem;
		font-weight: 600;
		line-height: 1;
		white-space: nowrap;
	}

	.compact {
		width: 2.75rem;
		min-width: 2.75rem;
		padding: 0;
		aspect-ratio: 1 / 1;
		border-radius: 0.875rem;
	}
</style>
