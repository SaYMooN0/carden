<script lang="ts">
	import { PointerSensor, DragDropProvider } from '@dnd-kit-svelte/svelte';
	import SortableTextContentItem from './SortableTextContentItem.svelte';
	import type { CardContentWithStringId } from './edit-plant-page-state.svelte';

	interface Props {
		title: string;
		subtitle: string;
		group: string;
		items: CardContentWithStringId[];
		onUpdateText: (itemId: string, nextText: string) => void;
		onRemoveItem: (itemId: string) => void;
		onAddItem: () => void;
		onReorder: (fromIndex: number, toIndex: number) => void;
	}

	let { title, subtitle, group, items, onUpdateText, onRemoveItem, onAddItem, onReorder }: Props =
		$props();

	function handleDragEnd(event: {
		operation: {
			source: { index: number };
			target?: { index: number } | null;
		};
	}) {
		const sourceIndex = event.operation.source.index;
		const targetIndex = event.operation.target?.index;

		if (targetIndex === undefined || sourceIndex === targetIndex) {
			return;
		}

		onReorder(sourceIndex, targetIndex);
	}
</script>

<section class="content-column">
	<div class="content-column__header">
		<div>
			<div class="content-column__title">{title}</div>
			<p class="content-column__subtitle">{subtitle}</p>
		</div>

		<div class="content-column__count">
			{items.length}
			{items.length === 1 ? ' item' : ' items'}
		</div>
	</div>

	<DragDropProvider>
		<div class="content-column__list">
			{#each items as item, index (item.stringId)}
				<SortableTextContentItem
					{group}
					{index}
					{item}
					onRemove={() => onRemoveItem(item.stringId)}
					onTextInput={(nextText) => onUpdateText(item.stringId, nextText)}
				/>
			{:else}
				<div class="content-column__empty">
					<p>This side has no content items yet.</p>
				</div>
			{/each}
		</div>
	</DragDropProvider>

	<button class="content-column__add-button" type="button" onclick={onAddItem}>
		add content item
	</button>
</section>

<style>
	.content-column {
		display: grid;
		grid-template-rows: auto minmax(0, 1fr) auto;
		gap: 1rem;
		min-height: 0;
		padding: 1rem;
		background: var(--primary-foreground);
		border: 0.0625rem solid var(--color-sage);
		border-radius: 1.25rem;
	}

	.content-column__header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 1rem;
	}

	.content-column__title {
		font-size: 1.25rem;
		font-weight: 700;
		line-height: 1.2;
		color: var(--text);
		text-transform: lowercase;
	}

	.content-column__subtitle {
		margin-top: 0.25rem;
		font-size: 0.875rem;
		line-height: 1.4;
		color: var(--color-text-light);
	}

	.content-column__count {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 2rem;
		padding-inline: 0.875rem;
		border-radius: 999rem;
		background: var(--color-sage-hover);
		font-size: 0.8125rem;
		font-weight: 700;
		color: var(--text);
		white-space: nowrap;
	}

	.content-column__list {
		display: grid;
		align-content: start;
		gap: 0.875rem;
		min-height: 0;
		overflow: auto;
		padding-right: 0.25rem;
	}

	.content-column__empty {
		display: grid;
		place-items: center;
		min-block-size: 10rem;
		padding: 1rem;
		border: 0.0625rem dashed var(--color-sage);
		border-radius: 1rem;
		background: var(--color-cream);
		text-align: center;
		color: var(--color-text-light);
	}

	.content-column__add-button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 3rem;
		padding-inline: 1rem;
		background: var(--color-sage-hover);
		color: var(--text);
		border: 0.0625rem dashed var(--color-terracotta-light);
		border-radius: 1rem;
		font-size: 1rem;
		font-weight: 700;
		transition:
			background 0.18s ease,
			transform 0.18s ease,
			border-color 0.18s ease;
	}

	.content-column__add-button:hover {
		background: var(--color-sage);
		border-color: var(--color-terracotta);
		transform: translateY(-0.0625rem);
	}
</style>
