<script lang="ts">
	import { ErrUtils, type Err } from '$lib/ts/err';
	import { StringUtils } from '$lib/ts/utils/string-utils';

	const { err }: { err: Err } = $props<{ err: Err }>();
	let showAdditional = $state(false);

	let iconElement = $state<SVGSVGElement>()!;

	function toggleDetails() {
		showAdditional = !showAdditional;
		if (iconElement) {
			iconElement.classList.remove('rotate-down', 'rotate-up');
			iconElement.classList.add(showAdditional ? 'rotate-down' : 'rotate-up');
		}
	}
	let hasDetails = $derived(!StringUtils.isNullOrWhiteSpace(err.details));
	let hasFixSuggestion = $derived(!StringUtils.isNullOrWhiteSpace(err.fixSuggestion));

	let hasSomethingExceptMessage = $derived(hasDetails || hasFixSuggestion);
</script>

<div class="err-container">
	<div class="err-message">
		{err.msg}
		{#if hasSomethingExceptMessage}
			<svg onclick={toggleDetails} bind:this={iconElement}
				><use href="#common-toggle-content-arrow" /></svg
			>
		{/if}
	</div>
	{#if hasDetails}
		<label class="err-additional" class:hidden={!showAdditional}>
			Details: {err.details}
		</label>
	{/if}
	{#if hasFixSuggestion}
		<label class="err-additional" class:hidden={!showAdditional}>
			Fix suggestion: {err.fixSuggestion}
		</label>
	{/if}
</div>

<style>
	.err-container {
		display: flex;
		flex-direction: column;
		box-sizing: border-box;
		padding: 0.125rem 0.5rem;
		border-radius: 0.25rem;
		background-color: var(--red-1);
		color: var(--red-2);
	}

	.err-message {
		display: grid;
		place-items: center left;
		min-width: 16rem;
		min-height: 1.5rem;
		margin: 0;
		color: inherit;
		font-size: 1rem;
		font-weight: 450;
		grid-template-columns: 1fr 1.5rem;
		word-break: normal;
		overflow-wrap: anywhere;
	}

	.err-message > svg {
		width: 1.5rem;
		height: 1.5rem;
		transition: transform 0.17s ease-in;
		transform-origin: center;
		cursor: pointer;
		stroke-width: 2.5;
	}

	.err-message > :global(svg.rotate-down) {
		animation: rotate-down 0.4s ease-in-out forwards;
	}

	.err-message > :global(svg.rotate-up) {
		animation: rotate-up 0.4s ease-in-out forwards;
	}

	.err-additional {
		height: auto;
		box-sizing: border-box;
		margin: 0.125rem 0 0.125rem 0.25rem;
		color: inherit;
		font-size: 1rem;
		font-weight: 420;
		opacity: 1;
		transition: all 0.2s ease;
		interpolate-size: allow-keywords;
	}

	.hidden {
		height: 0;
		margin: 0;
		font-size: 0;
		opacity: 0;
	}

	@keyframes rotate-down {
		from {
			transform: rotate(0deg);
		}

		to {
			transform: rotate(-180deg);
		}
	}

	@keyframes rotate-up {
		from {
			transform: rotate(-180deg);
		}

		to {
			transform: rotate(0deg);
		}
	}
</style>
