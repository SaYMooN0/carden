<script lang="ts">
	import HomeButton from '$lib/components/HomeButton.svelte';
	import type { CardPreview } from '../edit-plant-page-state.svelte';

	interface Props {
		plantName: string;
		cardsCount: number;
		plantDeckCardsPreview: CardPreview[];
		selectedCardId: string | null;
		selectCard: (cardId: string) => void;
		addNewCard: () => void;
		openConfirmCardDeleteDialog: (cardId: string) => void;
	}
	let {
		plantName,
		cardsCount,
		plantDeckCardsPreview,
		selectedCardId,
		selectCard,
		addNewCard,
		openConfirmCardDeleteDialog
	}: Props = $props();
	const subtitleText = $derived.by(() => {
		const rule = new Intl.PluralRules('ru-RU').select(cardsCount);
		let form = 'карточек';
		if (rule === 'one') form = 'карточка';
		else if (rule === 'few') form = 'карточки';
		return `${cardsCount} ${form} в колоде`;
	});
</script>

<aside class="sidebar">
	<div class="sidebar-hero">
		<HomeButton isCompact={false} />
		<h1 class="sidebar-title" title={plantName}>{plantName}</h1>
		<p class="sidebar-subtitle">{subtitleText}</p>
	</div>

	<div class="cards-list">
		{#each plantDeckCardsPreview as card (card.id)}
			<div
				class:selected={selectedCardId === card.id}
				class="card-tile"
				onclick={() => selectCard(card.id)}
			>
				<div class="card-tile-top-row">
					<span class="card-tile-index">карточка {card.number}</span>
					<button
						type="button"
						onclick={(e) => {
							e.stopPropagation();
							openConfirmCardDeleteDialog(card.id);
						}}
						class="delete-button"
						><svg
							xmlns="http://www.w3.org/2000/svg"
							viewBox="0 0 24 24"
							color="currentColor"
							fill="none"
							stroke="currentColor"
							stroke-linecap="round"
							stroke-linejoin="round"
						>
							<path d="M20 12L4 12" />
						</svg></button
					>
				</div>

				<div class="card-tile-preview-group">
					<div class="card-tile-side-label">лицевая</div>
					<p class="card-tile-preview">{card.frontTextPreview}</p>
				</div>

				<div class="card-tile-preview-group">
					<div class="card-tile-side-label">обратная</div>
					<p class="card-tile-preview">{card.backTextPreview}</p>
				</div>
			</div>
		{:else}
			<div class="cards-list-empty">
				<p>В этой колоде пока нет карточек.</p>
			</div>
		{/each}
	</div>

	<button class="create-card-button" type="button" onclick={() => addNewCard()}>
		Добавить новую карточку
	</button>
</aside>

<style>
	.sidebar {
		min-height: 0;
		overflow: hidden;
		background: var(--primary-foreground);
		border: 0.125rem solid var(--color-sage);
		border-radius: 1.5rem;
		box-shadow: var(--shadow);
		display: grid;
		grid-template-rows: auto minmax(0, 1fr) auto;
		gap: 1rem;
		padding: 1.25rem;
	}

	.sidebar-hero {
		display: grid;
		gap: 0.375rem;
	}

	.sidebar-title {
		font-size: 1.75rem;
		color: var(--text);
		text-overflow: ellipsis;
		overflow: hidden;
		white-space: nowrap;
	}

	.sidebar-subtitle {
		font-size: 1rem;
		line-height: 1.4;
		color: var(--color-text-light);
	}

	.cards-list {
		display: grid;
		align-content: start;
		gap: 0.75rem;
		min-height: 0;
		overflow-y: auto;
		overflow-x: hidden;
		min-width: 0;
		padding-right: 0.25rem;
	}

	.card-tile {
		display: grid;
		gap: 0.75rem;
		padding: 1rem;
		text-align: left;
		background: var(--primary-foreground);
		border: 0.125rem solid var(--color-sage);
		border-radius: 1.125rem;
		transition:
			transform 0.18s ease,
			border-color 0.18s ease,
			box-shadow 0.18s ease;
		width: 100%;
	}

	.card-tile:hover {
		border-color: var(--color-terracotta-light);
		box-shadow: var(--shadow);
	}
	.delete-button {
		width: 1.375rem;
		height: 1.375rem;
		border-radius: 0.375rem;
		padding: 0.125rem;
		background: var(--red-3);
		color: var(--primary-foreground);
		border: none;
		opacity: 0;
		transition:
			transform 0.18s ease,
			background 0.18s ease,
			border-color 0.18s ease,
			opacity 0.18s ease;
		stroke-width: 2;
		cursor: pointer;
	}
	.card-tile:hover .delete-button {
		opacity: 1;
	}
	.delete-button:hover {
		opacity: 1;
		background: var(--red-5);
	}

	.card-tile.selected {
		border-color: var(--color-terracotta);
		background: var(--color-cream);
	}

	.card-tile-top-row {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.75rem;
	}

	.card-tile-index,
	.card-tile-side-label {
		font-size: 0.75rem;
		font-weight: 600;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: var(--color-text-light);
	}

	.card-tile-preview-group {
		display: grid;
		gap: 0.25rem;
	}

	.card-tile-preview {
		font-size: 1rem;
		line-height: 1.5;
		color: var(--text);
		display: block;
		text-overflow: ellipsis;
		overflow: hidden;
	}

	.cards-list-empty {
		display: grid;
		place-items: center;
		min-block-size: 9rem;
		padding: 1rem;
		border: 0.125rem dashed var(--color-sage);
		border-radius: 1rem;
		color: var(--color-text-light);
		background: var(--color-cream);
		text-align: center;
	}

	.create-card-button {
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
		background: var(--primary);
		color: var(--primary-foreground);
		border: 0.125rem solid var(--primary);
	}

	.create-card-button:hover {
		background: var(--primary-hov);
		border-color: var(--primary-hov);
		transform: translateY(-0.125rem);
	}

	.create-card-button:disabled {
		opacity: 0.5;
		transform: none;
	}
</style>
