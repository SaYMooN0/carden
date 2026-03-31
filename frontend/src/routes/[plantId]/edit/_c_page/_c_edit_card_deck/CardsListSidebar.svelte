<script lang="ts">
	import type { CardPreview } from '../edit-plant-page-state.svelte';

	interface Props {
		plantName: string;
		cardsCount: number;
		plantDeckCardsPreview: CardPreview[];
		selectedCardId: string | null;
		selectCard: (cardId: string) => void;
		addNewCard: () => void;
	}
	let {
		plantName,
		cardsCount,
		plantDeckCardsPreview,
		selectedCardId,
		selectCard,
		addNewCard
	}: Props = $props();
	const subtitleText = $derived(
		`${cardsCount} ${cardsCount === 1 ? ' card in deck' : ' cards in deck'}`
	);
</script>

<aside class="sidebar">
	<div class="sidebar-hero">
		<div class="sidebar-eyebrow">plant deck editor</div>
		<h1 class="sidebar-title" title={plantName}>{plantName}</h1>
		<p class="sidebar-subtitle">{subtitleText}</p>
	</div>

	<div class="cards-list">
		{#each plantDeckCardsPreview as card (card.id)}
			<button
				class:selected={selectedCardId === card.id}
				class="card-tile"
				type="button"
				onclick={() => selectCard(card.id)}
			>
				<div class="card-tile-top-row">
					<span class="card-tile-index">card {card.number}</span>
					{#if selectedCardId === card.id}
						<span class="card-tile-selected-badge">editing</span>
					{/if}
				</div>

				<div class="card-tile-preview-group">
					<div class="card-tile-side-label">front</div>
					<p class="card-tile-preview">{card.frontTextPreview}</p>
				</div>

				<div class="card-tile-preview-group">
					<div class="card-tile-side-label">back</div>
					<p class="card-tile-preview">{card.backTextPreview}</p>
				</div>
			</button>
		{:else}
			<div class="cards-list-empty">
				<p>There are no cards in this deck yet.</p>
			</div>
		{/each}
	</div>

	<button class="create-card-button" type="button" onclick={() => addNewCard()}>
		Add new card
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
		padding: 0.375rem;
	}

	.sidebar-eyebrow {
		font-size: 0.75rem;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--color-text-light);
	}

	.sidebar-title {
		font-size: 1.75rem;
		line-height: 1.1;
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
		overflow: auto;
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
		transform: translateY(-0.125rem);
		border-color: var(--color-terracotta-light);
		box-shadow: var(--shadow);
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

	.card-tile-selected-badge {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 1.75rem;
		padding-inline: 0.75rem;
		border-radius: 999rem;
		font-size: 0.875rem;
		font-weight: 600;
		background: var(--color-terracotta);
		color: var(--primary-foreground);
	}

	.card-tile-preview-group {
		display: grid;
		gap: 0.25rem;
	}

	.card-tile-preview {
		font-size: 1rem;
		line-height: 1.5;
		color: var(--text);
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
