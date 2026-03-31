<script lang="ts">
	import { onMount } from 'svelte';
	import SortableTextContentItem from './_c_list/SortableContentItem.svelte';
	import type {
		CardContentSide,
		CardContentWithStringId
	} from '../../edit-plant-page-state.svelte';
	import Sortable from 'sortablejs';

	interface Props {
		title: string;
		subtitle: string;
		group: string;
		contentItems: CardContentWithStringId[];
		side: CardContentSide;
		onMoveItem: (
			fromSide: CardContentSide,
			toSide: CardContentSide,
			fromIndex: number,
			toIndex: number
		) => void;
	}

	let { title, subtitle, group, contentItems = $bindable(), side, onMoveItem }: Props = $props();

	let listElement = $state<HTMLDivElement | null>(null);

	onMount(() => {
		if (!listElement) {
			return;
		}

		const sortable = Sortable.create(listElement, {
			group,
			animation: 150,
			handle: '.drag-handle',
			ghostClass: 'sortable-ghost',
			chosenClass: 'sortable-chosen',
			dragClass: 'sortable-drag',
			fallbackOnBody: true,
			swapThreshold: 0.65,
			onEnd: (event) => {
				if (event.oldIndex == null || event.newIndex == null) {
					return;
				}

				const fromSide = event.from.getAttribute('data-list-id') as CardContentSide | null;
				const toSide = event.to.getAttribute('data-list-id') as CardContentSide | null;

				if (!fromSide || !toSide) {
					return;
				}

				onMoveItem(fromSide, toSide, event.oldIndex, event.newIndex);
			}
		});

		return () => {
			sortable.destroy();
		};
	});

	function addContentItem() {
		contentItems = [...contentItems, { stringId: crypto.randomUUID(), text: '' }];
	}

	function removeContentItem(itemId: string) {
		contentItems = contentItems.filter((item) => item.stringId !== itemId);
	}
</script>

<section class="content-column">
	<div class="content-column-header">
		<div>
			<div class="content-column-title">{title}</div>
			<p class="content-column-subtitle">{subtitle}</p>
		</div>

		<div class="content-column-count">
			{contentItems.length}
			{contentItems.length === 1 ? ' item' : ' items'}
		</div>
	</div>

	<div class="content-column-scroll-area">
		<div bind:this={listElement} class="content-column-list" data-list-id={side}>
			{#each contentItems as item (item.stringId)}
				<SortableTextContentItem
					onRemove={() => removeContentItem(item.stringId)}
					text={item.text}
					stringId={item.stringId}
				/>
			{:else}
				<div class="content-column-empty">
					<p>This side has no content items yet.</p>
					<p>Drag items here from the other side or add a new one.</p>
				</div>
			{/each}
		</div>
	</div>

	<button class="content-column-add-button" type="button" onclick={() => addContentItem()}>
		add content item
	</button>
</section>

<style>
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

	.content-column-list {
		display: grid;
		align-content: start;
		gap: 0.875rem;
		min-height: 100%;
		max-height: 100%;
		overflow: auto;
		padding-right: 0.25rem;
	}

	.content-column-empty {
		display: grid;
		gap: 0.375rem;
		place-items: center;
		min-block-size: 10rem;
		padding: 1rem;
		border: 0.125rem dashed var(--color-sage);
		border-radius: 1rem;
		background: var(--color-cream);
		text-align: center;
		color: var(--color-text-light);
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
</style>
