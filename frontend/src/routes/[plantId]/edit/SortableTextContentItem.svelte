<script lang="ts">
	import type { CardContentWithStringId } from './edit-plant-page-state.svelte';

	interface Props {
		item: CardContentWithStringId;
		index: number;
		group: string;
		onTextInput: (nextText: string) => void;
		onRemove: () => void;
	}

	let { item, index, group, onTextInput, onRemove }: Props = $props();
</script>

<div class="content-item">
	<div class="content-item__toolbar">
		<div class="content-item__left">
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

			<div class="content-item__label">text item</div>
		</div>

		<button class="remove-button" type="button" onclick={onRemove}> remove </button>
	</div>

	<textarea
		oninput={(event) => onTextInput((event.currentTarget as HTMLTextAreaElement).value)}
		placeholder="Write text here..."
		rows="7"
		value={item.type === 'TextContentItem' ? item.text : ''}
	></textarea>
</div>

<style>
	.content-item {
		display: grid;
		gap: 0.75rem;
		padding: 0.875rem;
		background: var(--primary-foreground);
		border: 0.0625rem solid var(--color-sage);
		border-radius: 1rem;
		box-shadow: var(--shadow);
		transition:
			transform 0.18s ease,
			border-color 0.18s ease,
			opacity 0.18s ease;
	}

	.content-item.dragging {
		opacity: 0.72;
		border-color: var(--color-terracotta);
	}

	.content-item.source {
		border-color: var(--color-terracotta-light);
	}

	.content-item__toolbar {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.75rem;
	}

	.content-item__left {
		display: flex;
		align-items: center;
		gap: 0.75rem;
	}

	.content-item__label {
		font-size: 0.8125rem;
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
		border: 0.0625rem solid var(--color-sage);
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
		padding-inline: 0.875rem;
		background: var(--red-1);
		color: var(--red-5);
		border: 0.0625rem solid var(--red-3);
		border-radius: 0.875rem;
		font-size: 0.875rem;
		font-weight: 700;
		transition:
			transform 0.18s ease,
			background 0.18s ease;
	}

	.remove-button:hover {
		background: var(--red-2);
		transform: translateY(-0.0625rem);
	}

	textarea {
		inline-size: 100%;
		min-block-size: 10rem;
		padding: 1rem;
		resize: vertical;
		background: var(--color-cream);
		border: 0.0625rem solid var(--color-sage);
		border-radius: 0.875rem;
		outline: none;
		font-size: 1rem;
		line-height: 1.5;
	}

	textarea:focus {
		border-color: var(--color-terracotta);
	}
</style>
