<script lang="ts">
	import type { CardContentSide, CardViewToEdit } from '../edit-plant-page-state.svelte';
	import SortableContentList from './_c_card_editing/SortableContentList.svelte';

	interface Props {
		card: CardViewToEdit;
		cardHasUnsavedChanges: boolean;
		saveCardChanges: () => void;
		resetCardChanges: () => void;
	}

	let { card, cardHasUnsavedChanges, saveCardChanges, resetCardChanges }: Props = $props();

	function formatDateTime(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) {
			return value;
		}
		return date.toLocaleString();
	}
	function moveContentItem(
		fromSide: CardContentSide,
		toSide: CardContentSide,
		fromIndex: number,
		toIndex: number
	) {
		if (fromSide === toSide) {
			const nextItems = [...card[fromSide]];
			const [movedItem] = nextItems.splice(fromIndex, 1);

			if (!movedItem) {
				return;
			}

			nextItems.splice(toIndex, 0, movedItem);
			card[fromSide] = nextItems;
			return;
		}

		const nextSourceItems = [...card[fromSide]];
		const nextTargetItems = [...card[toSide]];
		const [movedItem] = nextSourceItems.splice(fromIndex, 1);

		if (!movedItem) {
			return;
		}

		nextTargetItems.splice(toIndex, 0, movedItem);
		card[fromSide] = nextSourceItems;
		card[toSide] = nextTargetItems;
	}
</script>

<div class="editor-header">
	<div>
		<div class="editor-header-eyebrow">editing card</div>
		<h2 class="editor-header-title">Deck card editor</h2>
		<p class="editor-header-meta">
			last edited {formatDateTime(card.lastTimeEdited)}
		</p>
	</div>

	<div class="editor-header-actions">
		<span class:dirty={cardHasUnsavedChanges} class="status-pill">
			{cardHasUnsavedChanges ? 'unsaved changes' : 'saved'}
		</span>

		<button
			class="secondary-button"
			disabled={!cardHasUnsavedChanges}
			type="button"
			onclick={resetCardChanges}
		>
			reset
		</button>

		<button
			class="primary-button"
			disabled={!cardHasUnsavedChanges}
			type="button"
			onclick={saveCardChanges}
		>
			save card
		</button>
	</div>
</div>

<div class="editor-grid">
	<SortableContentList
		group={`card-${card.id}`}
		bind:contentItems={card.contentFront}
		title="front side"
		subtitle="Question, prompt or title"
		side="contentFront"
		onMoveItem={moveContentItem}
	/>

	<SortableContentList
		group={`card-${card.id}`}
		bind:contentItems={card.contentBack}
		title="back side"
		side="contentBack"
		onMoveItem={moveContentItem}
		subtitle="Answer, explanation or extra note"
	/>
</div>

<style>
	.editor-header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 1rem;
		padding-bottom: 0.5rem;
		border-bottom: 0.125rem solid var(--color-sage);
	}

	.editor-header-eyebrow {
		font-size: 0.75rem;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--color-text-light);
	}

	.editor-header-title {
		font-size: 1.75rem;
		line-height: 1.1;
		color: var(--text);
		margin-top: 0.375rem;
	}

	.editor-header-meta {
		margin-top: 0.375rem;
		font-size: 1rem;
		line-height: 1.4;
		color: var(--color-text-light);
	}

	.editor-header-actions {
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		justify-content: flex-end;
		gap: 0.75rem;
	}

	.editor-grid {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
		gap: 1.25rem;
		min-height: 0;
		height: 100%;
		overflow: hidden;
	}

	.status-pill {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 1.75rem;
		padding-inline: 0.75rem;
		border-radius: 999rem;
		font-size: 0.875rem;
		font-weight: 600;
		background: var(--color-sage-hover);
		color: var(--text);
	}

	.status-pill.dirty {
		background: var(--color-terracotta);
		color: var(--primary-foreground);
	}

	.primary-button,
	.secondary-button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 3rem;
		padding-inline: 1.25rem;
		border-radius: 1rem;
		font-size: 1rem;
		font-weight: 700;
		transition:
			transform 0.18s ease,
			background 0.18s ease,
			border-color 0.18s ease,
			opacity 0.18s ease;
	}

	.primary-button {
		background: var(--primary);
		color: var(--primary-foreground);
		border: 0.125rem solid var(--primary);
	}

	.primary-button:hover {
		background: var(--primary-hov);
		border-color: var(--primary-hov);
		transform: translateY(-0.125rem);
	}

	.secondary-button {
		background: var(--primary-foreground);
		color: var(--text);
		border: 0.125rem solid var(--color-terracotta-light);
	}

	.secondary-button:hover {
		background: var(--color-sage-hover);
		transform: translateY(-0.125rem);
	}

	.primary-button:disabled,
	.secondary-button:disabled {
		opacity: 0.5;
		transform: none;
	}
</style>
