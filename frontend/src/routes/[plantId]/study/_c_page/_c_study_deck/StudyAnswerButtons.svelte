<script lang="ts">
	import type { CardDifficulty } from '../study-deck-page-state.svelte';

	interface Props {
		isEnabled: boolean;
		onRate: (difficulty: CardDifficulty) => void;
	}

	let { isEnabled, onRate }: Props = $props();

	const buttons: { difficulty: CardDifficulty; label: string; className: string }[] = [
		{ difficulty: 'Again', label: 'Again', className: 'answer-button-again' },
		{ difficulty: 'Hard', label: 'Hard', className: 'answer-button-hard' },
		{ difficulty: 'Good', label: 'Good', className: 'answer-button-good' },
		{ difficulty: 'Easy', label: 'Easy', className: 'answer-button-easy' }
	];
</script>

<div class="answer-buttons-grid" class:show={isEnabled}>
	{#each buttons as button}
		<button
			type="button"
			class={`answer-button ${button.className}`}
			onclick={() => onRate(button.difficulty)}
		>
			{button.label}
		</button>
	{/each}
</div>

<style>
	.answer-buttons-grid {
		display: grid;
		grid-template-columns: repeat(4, minmax(0, 1fr));
		gap: 0.75rem;
		height: 0;
		opacity: 0;
		transition:
			opacity 0.35s ease,
			transform 0.6s ease;
		transform: translateY(2rem);
		interpolate-size: allow-keywords;
	}

	.answer-buttons-grid.show {
		height: auto;
		opacity: 1;
		transform: translateY(0);
	}

	.answer-button {
		min-height: 3.5rem;
		padding: 0.875rem 1rem;
		border: none;
		border-radius: 1.25rem;
		font-size: 1rem;
		font-weight: 700;
		color: var(--primary-foreground);
		box-shadow: var(--shadow);
		transition:
			transform 0.15s ease,
			opacity 0.15s ease;
		cursor: pointer;
	}

	.answer-button:disabled {
		opacity: 0.45;
		cursor: not-allowed;
	}

	.answer-button:not(:disabled):hover {
		transform: translateY(-0.125rem);
	}

	.answer-button-again {
		background: var(--color-answer-again);
	}

	.answer-button-hard {
		background: var(--color-answer-hard);
	}

	.answer-button-good {
		background: var(--color-answer-good);
	}

	.answer-button-easy {
		background: var(--color-answer-easy);
	}
</style>
