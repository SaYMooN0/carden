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
	<div class="sidebar__hero">
		<div class="sidebar__eyebrow">plant deck editor</div>
		<h1 class="sidebar__title" title={plantName}>{plantName}</h1>
		<p class="sidebar__subtitle">{subtitleText}</p>
	</div>

	<div class="cards-list">
		{#each plantDeckCardsPreview as card (card.id)}
			<button
				class:selected={selectedCardId === card.id}
				class="card-tile"
				type="button"
				onclick={() => selectCard(card.id)}
			>
				<div class="card-tile__top-row">
					<span class="card-tile__index">card {card.number}</span>
					{#if selectedCardId === card.id}
						<span class="card-tile__selected-badge">editing</span>
					{/if}
				</div>

				<div class="card-tile__preview-group">
					<div class="card-tile__side-label">front</div>
					<p class="card-tile__preview">{card.frontTextPreview}</p>
				</div>

				<div class="card-tile__preview-group">
					<div class="card-tile__side-label">back</div>
					<p class="card-tile__preview">{card.backTextPreview}</p>
				</div>
			</button>
		{:else}
			<div class="cards-list__empty">
				<p>There are no cards in this deck yet.</p>
			</div>
		{/each}
	</div>

	<button class="create-card-button" type="button" onclick={() => addNewCard()}>
		Add new card
	</button>
</aside>
