<script lang="ts">
	import { onMount, tick } from 'svelte';
	import Sortable from 'sortablejs';
	import type { CardContentSide, CardInEditing } from '../edit-plant-page-state.svelte';
	import SortableContentItem from './_c_card_editing/SortableContentItem.svelte';
	import { toast } from 'svelte-sonner';

	interface Props {
		card: CardInEditing;
		cardHasUnsavedChanges: boolean;
		saveCardChanges: () => Promise<{ isSuccess: true } | { isSuccess: false; errMsg: string }>;
		resetCardChanges: () => void;
	}

	let {
		card = $bindable(),
		cardHasUnsavedChanges,
		saveCardChanges,
		resetCardChanges
	}: Props = $props();
	let isSaving = $state(false);

	let frontListElement = $state<HTMLDivElement | null>(null);
	let backListElement = $state<HTMLDivElement | null>(null);

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
		toIndex: number,
		itemId?: string
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

		const nextSourceItems = card[fromSide];
		const nextTargetItems = card[toSide];
		const movedItemIndex = itemId
			? nextSourceItems.findIndex((item) => item.stringId === itemId)
			: fromIndex;

		if (movedItemIndex < 0) {
			return;
		}

		const [movedItem] = nextSourceItems.splice(movedItemIndex, 1);
		if (!movedItem) {
			return;
		}

		nextTargetItems.splice(toIndex, 0, movedItem);
		card[fromSide] = nextSourceItems;
		card[toSide] = nextTargetItems;
	}

	function removeContentItem(side: CardContentSide, itemId: string) {
		card[side] = card[side].filter((item) => item.stringId !== itemId);
	}

	async function addContentItemAndFocus(side: CardContentSide) {
		const newItemId = crypto.randomUUID();
		card[side] = [...card[side], { stringId: newItemId, text: '' }];
		await tick();
		const element = document.querySelector<HTMLTextAreaElement>(
			`[data-item-id="${newItemId}"] textarea`
		);
		element?.focus();
	}

	function createSortable(side: CardContentSide, element: HTMLDivElement) {
		return Sortable.create(element, {
			group: `card-${card.id}`,
			animation: 150,
			handle: '.drag-handle',
			ghostClass: 'sortable-ghost',
			chosenClass: 'sortable-chosen',
			dragClass: 'sortable-drag',
			fallbackOnBody: true,
			swapThreshold: 0.65,
			removeCloneOnHide: true,
			onUpdate: (event) => {
				if (event.oldIndex == null || event.newIndex == null) {
					return;
				}
				moveContentItem(side, side, event.oldIndex, event.newIndex);
			},
			onAdd: (event) => {
				if (event.oldIndex == null || event.newIndex == null) {
					return;
				}

				const fromSide = event.from.getAttribute('data-list-id') as CardContentSide | null;
				const itemId = event.item.getAttribute('data-item-id');

				if (!fromSide || !itemId) {
					return;
				}
				event.item.remove();
				moveContentItem(fromSide, side, event.oldIndex, event.newIndex, itemId);
			}
		});
	}

	onMount(() => {
		if (!frontListElement || !backListElement) {
			return;
		}
		const frontSortable = createSortable('contentFront', frontListElement);
		const backSortable = createSortable('contentBack', backListElement);

		return () => {
			frontSortable.destroy();
			backSortable.destroy();
		};
	});

	async function saveChanges() {
		if (isSaving || !cardHasUnsavedChanges) {
			return;
		}

		isSaving = true;
		try {
			await saveCardChanges().then((res) => {
				if (!res.isSuccess) {
					toast.error(res.errMsg);
				}
			});
		} finally {
			isSaving = false;
		}
	}
</script>

<div class="editor-header">
	<div>
		<div class="editor-header-eyebrow">editing card</div>
		<h2 class="editor-header-title">Deck card editor</h2>
		<p class="editor-header-meta">last edited {formatDateTime(card.lastTimeEdited)}</p>
	</div>

	<div class="editor-header-actions">
		<span class:dirty={cardHasUnsavedChanges} class="status-pill">
			{cardHasUnsavedChanges ? 'unsaved changes' : 'saved'}
		</span>

		<button
			class="secondary-button"
			disabled={!cardHasUnsavedChanges || isSaving}
			type="button"
			onclick={resetCardChanges}
		>
			reset
		</button>

		<button
			class="primary-button"
			disabled={!cardHasUnsavedChanges || isSaving}
			type="button"
			onclick={saveChanges}
		>
			{isSaving ? 'saving...' : 'save card'}
		</button>
	</div>
</div>

<div class="editor-grid">
	<section class="content-column">
		<div class="content-column-header">
			<div>
				<div class="content-column-title">front side</div>
				<p class="content-column-subtitle">Question, prompt or title</p>
			</div>

			<div class="content-column-count">
				{card.contentFront.length}
				{card.contentFront.length === 1 ? ' item' : ' items'}
			</div>
		</div>

		<div class="content-column-scroll-area">
			<div
				class="content-column-list-wrap"
				class:is-empty={card.contentFront.length === 0}
				data-empty-text="No content items"
			>
				<div bind:this={frontListElement} class="content-column-list" data-list-id="contentFront">
					{#each card.contentFront as item (item.stringId)}
						<SortableContentItem
							{item}
							onRemove={() => removeContentItem('contentFront', item.stringId)}
							onTextChange={(text) => {
								item.text = text;
							}}
						/>
					{/each}
				</div>
			</div>
		</div>

		<button
			class="content-column-add-button"
			type="button"
			onclick={() => addContentItemAndFocus('contentFront')}
		>
			add content item
		</button>
	</section>

	<section class="content-column">
		<div class="content-column-header">
			<div>
				<div class="content-column-title">back side</div>
				<p class="content-column-subtitle">Answer, explanation or extra note</p>
			</div>

			<div class="content-column-count">
				{card.contentBack.length}
				{card.contentBack.length === 1 ? ' item' : ' items'}
			</div>
		</div>

		<div class="content-column-scroll-area">
			<div
				class="content-column-list-wrap"
				class:is-empty={card.contentBack.length === 0}
				data-empty-text="No content items"
			>
				<div bind:this={backListElement} class="content-column-list" data-list-id="contentBack">
					{#each card.contentBack as item (item.stringId)}
						<SortableContentItem
							{item}
							onRemove={() => removeContentItem('contentBack', item.stringId)}
							onTextChange={(text) => {
								item.text = text;
							}}
						/>
					{/each}
				</div>
			</div>
		</div>
		<button
			class="content-column-add-button"
			type="button"
			onclick={() => addContentItemAndFocus('contentBack')}
		>
			add content item
		</button>
	</section>
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

	.content-column {
		display: grid;
		grid-template-rows: auto minmax(0, 1fr) auto;
		gap: 1rem;
		min-height: 0;
		overflow: hidden;
		padding: 1rem;
		background: var(--primary-foreground);
		border: 0.125rem solid var(--color-sage);
		border-radius: 1.25rem;
	}

	.content-column-header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 1rem;
	}

	.content-column-title {
		font-size: 1.25rem;
		font-weight: 700;
		line-height: 1.2;
		color: var(--text);
		text-transform: lowercase;
	}

	.content-column-subtitle {
		margin-top: 0.25rem;
		font-size: 0.875rem;
		line-height: 1.4;
		color: var(--color-text-light);
	}

	.content-column-count {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 2rem;
		padding-inline: 0.875rem;
		border-radius: 999rem;
		background: var(--color-sage-hover);
		font-size: 0.875rem;
		font-weight: 700;
		color: var(--text);
		white-space: nowrap;
	}

	.content-column-scroll-area {
		min-height: 0;
		overflow: hidden;
	}

	.content-column-list-wrap {
		position: relative;
		min-height: 100%;
		block-size: 100%;
	}

	.content-column-list {
		position: relative;
		z-index: 1;
		display: grid;
		align-content: start;
		gap: 0.875rem;
		min-height: 100%;
		max-height: 100%;
		overflow: auto;
		padding-right: 0.25rem;
	}

	.content-column-list-wrap.is-empty::before {
		content: attr(data-empty-text);
		position: absolute;
		inset: 0;
		display: grid;
		place-items: center;
		padding: 1rem;
		border: 0.125rem dashed var(--color-sage);
		border-radius: 1rem;
		background: var(--color-cream);
		color: var(--color-text-light);
		text-align: center;
		pointer-events: none;
		z-index: 0;
	}
	.content-column-add-button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 3rem;
		padding-inline: 1rem;
		background: var(--color-sage-hover);
		color: var(--text);
		border: 0.125rem dashed var(--color-terracotta-light);
		border-radius: 1rem;
		font-size: 1rem;
		font-weight: 700;
		transition:
			background 0.18s ease,
			transform 0.18s ease,
			border-color 0.18s ease;
	}

	.content-column-add-button:hover {
		background: var(--color-sage);
		border-color: var(--color-terracotta);
		transform: translateY(-0.125rem);
	}

	.content-column-scroll-area {
		min-height: 0;
		overflow: hidden;
	}

	.content-column-list-wrap {
		position: relative;
		min-height: 100%;
		block-size: 100%;
	}

	.content-column-list {
		display: grid;
		align-content: start;
		gap: 0.875rem;
		min-height: 100%;
		max-height: 100%;
		overflow: auto;
		padding-right: 0.25rem;
	}
</style>
