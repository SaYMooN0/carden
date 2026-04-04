<script lang="ts">
	import type { CardContentWithStringId } from '../../edit-plant-page-state.svelte';

	interface Props {
		onRemove: () => void;
		onTextChange: (text: string) => void;
		item: CardContentWithStringId;
	}

	let { onRemove, onTextChange, item }: Props = $props();
</script>

<div class="content-item-wrap" data-item-id={item.stringId}>
	<div class="content-item">
		<div class="content-item-toolbar">
			<div class="content-item-left">
				<button
					aria-label="Move content item"
					class="drag-handle"
					title="Move content item"
					type="button"
				>
					<span></span>
					<span></span>
					<span></span>
					<span></span>
					<span></span>
					<span></span>
				</button>

				<div class="content-item-label">text item</div>
			</div>

			<button class="remove-button" type="button" onclick={onRemove}>remove</button>
		</div>

		<textarea
			placeholder="Write text here..."
			rows="2"
			value={item.text}
			oninput={(e) => onTextChange((e.currentTarget as HTMLTextAreaElement).value)}
		></textarea>
	</div>
</div>

<style>
	.content-item-wrap {
		position: relative;
		user-select: none;
	}

	.content-item {
		display: grid;
		gap: 0.75rem;
		padding: 0.875rem;
		background: var(--primary-foreground);
		border: 0.125rem solid var(--color-sage);
		border-radius: 1rem;
		box-shadow: var(--shadow);
		transition:
			transform 0.18s ease,
			border-color 0.18s ease,
			box-shadow 0.18s ease,
			background 0.18s ease;
	}

	.content-item-toolbar {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.75rem;
	}

	.content-item-left {
		display: flex;
		align-items: center;
		gap: 0.75rem;
	}

	.content-item-label {
		font-size: 0.875rem;
		font-weight: 700;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: var(--color-text-light);
	}

	.drag-handle {
		display: grid;
		grid-template-columns: repeat(2, 0.25rem);
		gap: 0.25rem;
		place-content: center;
		inline-size: 2.5rem;
		block-size: 2.5rem;
		background: var(--color-cream);
		border: 0.125rem solid var(--color-sage);
		border-radius: 0.875rem;
		cursor: grab;
	}

	.drag-handle:active {
		cursor: grabbing;
	}

	.drag-handle span {
		inline-size: 0.25rem;
		block-size: 0.25rem;
		border-radius: 999rem;
		background: var(--color-terracotta);
	}

	.remove-button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 2.25rem;
		padding: 0.5rem 0.75rem;
		background: var(--red-1);
		color: var(--red-4);
		border: 0.125rem solid var(--red-4);
		border-radius: 1000rem;
		font-size: 0.875rem;
		font-weight: 700;
		transition:
			transform 0.18s ease,
			background 0.18s ease,
			opacity 0.18s ease;
		cursor: pointer;
	}

	.remove-button:hover {
		background: var(--red-2);
		border-color: var(--red-5);
		color: var(--red-5);
	}

	textarea {
		inline-size: 100%;
		min-block-size: 10rem;
		padding: 1rem;
		resize: vertical;
		background: var(--color-cream);
		border: 0.125rem solid var(--color-sage);
		border-radius: 0.875rem;
		outline: none;
		font-size: 1rem;
		line-height: 1.5;
	}

	textarea:focus {
		border-color: var(--color-terracotta);
	}

	:global(.sortable-ghost .content-item) {
		background: var(--color-sage-hover);
		border-color: var(--color-terracotta-light);
	}

	:global(.sortable-chosen .content-item) {
		border-color: var(--color-terracotta);
	}

	:global(.sortable-drag .content-item) {
		box-shadow: var(--shadow);
	}
</style>
