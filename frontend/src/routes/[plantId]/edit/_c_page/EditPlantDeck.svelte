<script lang="ts">
	import type { Plant, CardContentItem, Card } from '$lib/ts/base-types';
	import {
		EditPlantPageState,
		type CardContentWithStringId
	} from '../edit-plant-page-state.svelte';
	import SortableTextContentList from '../SortableTextContentList.svelte';

	type EditableSide = 'contentFront' | 'contentBack';

	interface Props {
		plant: Plant;
	}

	let { plant }: Props = $props();
	let pageState = $state(new EditPlantPageState(plant));
	let searchValue = $state('');

	const editingCard = $derived(
		pageState.cardEditingState.state === 'CardEditing' ? pageState.cardEditingState.card : null
	);

	const selectedCardId = $derived.by(() => {
		if (pageState.cardEditingState.state === 'CardEditing') {
			return pageState.cardEditingState.card.id;
		}
		if (pageState.cardEditingState.state === 'ExpectedCardNotFound') {
			return pageState.cardEditingState.cardId;
		}
		if (pageState.cardEditingState.state === 'CardReloading') {
			return pageState.cardEditingState.cardId;
		}
		return null;
	});

	const filteredCards = $derived.by(() => {
		const query = searchValue.trim().toLowerCase();

		if (!query) {
			return pageState.plant.deck.cards;
		}

		return pageState.plant.deck.cards.filter((card, index) => {
			const haystack = [
				`card ${index + 1}`,
				getCardPreview(card.contentFront),
				getCardPreview(card.contentBack)
			]
				.join(' ')
				.toLowerCase();

			return haystack.includes(query);
		});
	});

	const hasUnsavedChanges = $derived.by(() => {
		if (!editingCard) {
			return false;
		}

		const originalCard = pageState.plant.deck.cards.find((card) => card.id === editingCard.id);
		if (!originalCard) {
			return false;
		}

		return !(
			pageState.checkIfCardContentItemsListsEqual(
				editingCard.contentFront,
				originalCard.contentFront
			) &&
			pageState.checkIfCardContentItemsListsEqual(editingCard.contentBack, originalCard.contentBack)
		);
	});

	function isTextContentItem(
		item: CardContentItem | CardContentWithStringId
	): item is Extract<CardContentItem, { type: 'TextContentItem' }> &
		Partial<Pick<CardContentWithStringId, 'stringId'>> {
		return item.type === 'TextContentItem';
	}

	function getCardPreview(items: CardContentItem[] | CardContentWithStringId[]): string {
		const firstTextItem = items.find(isTextContentItem);
		const text = firstTextItem?.text?.trim();

		if (!text) {
			return 'Empty side';
		}

		return text.length > 72 ? `${text.slice(0, 72)}…` : text;
	}

	function getCardNumber(cardId: string): number {
		return pageState.plant.deck.cards.findIndex((card) => card.id === cardId) + 1;
	}

	function addTextContentItem(side: EditableSide) {
		if (!editingCard) {
			return;
		}

		const nextItem = {
			type: 'TextContentItem',
			text: '',
			stringId: crypto.randomUUID()
		} as CardContentWithStringId;

		editingCard[side] = [...editingCard[side], nextItem];
	}

	function updateTextContentItem(side: EditableSide, itemId: string, nextText: string) {
		if (!editingCard) {
			return;
		}

		editingCard[side] = editingCard[side].map((item) => {
			if (item.stringId !== itemId || item.type !== 'TextContentItem') {
				return item;
			}

			return {
				...item,
				text: nextText
			};
		});
	}

	function removeTextContentItem(side: EditableSide, itemId: string) {
		if (!editingCard) {
			return;
		}

		editingCard[side] = editingCard[side].filter((item) => item.stringId !== itemId);
	}

	function reorderTextContentItems(side: EditableSide, fromIndex: number, toIndex: number) {
		if (!editingCard || fromIndex === toIndex) {
			return;
		}

		const nextItems = [...editingCard[side]];
		const [movedItem] = nextItems.splice(fromIndex, 1);

		if (!movedItem) {
			return;
		}

		nextItems.splice(toIndex, 0, movedItem);
		editingCard[side] = nextItems;
	}

	function persistEditableItems(items: CardContentWithStringId[]): CardContentItem[] {
		return items.map((item) => {
			if (item.type === 'TextContentItem') {
				return {
					type: 'TextContentItem',
					text: item.text
				};
			}

			return item;
		});
	}

	function saveCurrentCard() {
		if (!editingCard) {
			return;
		}

		const cardIndex = pageState.plant.deck.cards.findIndex((card) => card.id === editingCard.id);
		if (cardIndex === -1) {
			return;
		}

		const savedCard = {
			...editingCard,
			contentFront: persistEditableItems(editingCard.contentFront),
			contentBack: persistEditableItems(editingCard.contentBack)
		} as Card;

		pageState.plant.deck.cards[cardIndex] = savedCard;
		pageState.selectCard(savedCard.id);
	}

	function discardCurrentChanges() {
		if (!editingCard) {
			return;
		}

		pageState.selectCard(editingCard.id);
	}
</script>

<svelte:head>
	<title>Edit plant deck</title>
</svelte:head>

<div class="edit-page">
	<aside class="sidebar">
		<div class="sidebar__hero">
			<div class="sidebar__eyebrow">plant deck editor</div>
			<h1 class="sidebar__title">{pageState.plant.name}</h1>
			<p class="sidebar__subtitle">
				{pageState.plant.deck.cards.length}
				{pageState.plant.deck.cards.length === 1 ? ' card in deck' : ' cards in deck'}
			</p>
		</div>

		<label class="searchbar">
			<span class="searchbar__icon" aria-hidden="true">
				<svg viewBox="0 0 24 24">
					<circle cx="11" cy="11" r="7" />
					<path d="M20 20L16.5 16.5" />
				</svg>
			</span>
			<input bind:value={searchValue} placeholder="search cards" type="text" />
		</label>

		<div class="cards-list">
			{#each filteredCards as card (card.id)}
				<button
					class:selected={selectedCardId === card.id}
					class="card-tile"
					type="button"
					onclick={() => pageState.selectCard(card.id)}
				>
					<div class="card-tile__top-row">
						<span class="card-tile__index">card {getCardNumber(card.id)}</span>
						{#if selectedCardId === card.id}
							<span class="card-tile__selected-badge">editing</span>
						{/if}
					</div>

					<div class="card-tile__preview-group">
						<div class="card-tile__side-label">front</div>
						<p class="card-tile__preview">{getCardPreview(card.contentFront)}</p>
					</div>

					<div class="card-tile__preview-group">
						<div class="card-tile__side-label">back</div>
						<p class="card-tile__preview">{getCardPreview(card.contentBack)}</p>
					</div>
				</button>
			{:else}
				<div class="cards-list__empty">
					<p>No cards match this search.</p>
				</div>
			{/each}
		</div>

		<button class="create-card-button" type="button" onclick={() => pageState.addNewCard()}>
			add new card
		</button>
	</aside>

	<section class="editor-shell">
		{#if pageState.cardEditingState.state === 'NoCardSelected'}
			<div class="empty-state">
				<div class="empty-state__icon">✿</div>
				<h2>Select a card to start editing</h2>
				<p>Choose a card on the left and edit its front and back sides.</p>

				{#if pageState.plant.deck.cards.length > 0}
					<button
						class="primary-button"
						type="button"
						onclick={() => pageState.selectCard(pageState.plant.deck.cards[0].id)}
					>
						open first card
					</button>
				{/if}
			</div>
		{:else if pageState.cardEditingState.state === 'ExpectedCardNotFound'}
			<div class="empty-state empty-state--error">
				<div class="empty-state__icon">!</div>
				<h2>Card not found</h2>
				<p>Expected card id: {pageState.cardEditingState.cardId}</p>
			</div>
		{:else if editingCard}
			<div class="editor-header">
				<div>
					<div class="editor-header__eyebrow">editing card {getCardNumber(editingCard.id)}</div>
					<h2 class="editor-header__title">Deck card editor</h2>
					<p class="editor-header__subtitle">
						Reorder text blocks with the handle and edit each side independently.
					</p>
				</div>

				<div class="editor-header__actions">
					<span class:dirty={hasUnsavedChanges} class="status-pill">
						{hasUnsavedChanges ? 'unsaved changes' : 'saved'}
					</span>

					<button
						class="secondary-button"
						disabled={!hasUnsavedChanges}
						type="button"
						onclick={discardCurrentChanges}
					>
						reset
					</button>

					<button
						class="primary-button"
						disabled={!hasUnsavedChanges}
						type="button"
						onclick={saveCurrentCard}
					>
						save card
					</button>
				</div>
			</div>

			<div class="editor-grid">
				<SortableTextContentList
					group={`front-${editingCard.id}`}
					items={editingCard.contentFront}
					title="front side"
					subtitle="Question, prompt or title"
					onAddItem={() => addTextContentItem('contentFront')}
					onRemoveItem={(itemId) => removeTextContentItem('contentFront', itemId)}
					onReorder={(fromIndex, toIndex) =>
						reorderTextContentItems('contentFront', fromIndex, toIndex)}
					onUpdateText={(itemId, nextText) =>
						updateTextContentItem('contentFront', itemId, nextText)}
				/>

				<SortableTextContentList
					group={`back-${editingCard.id}`}
					items={editingCard.contentBack}
					title="back side"
					subtitle="Answer, explanation or extra note"
					onAddItem={() => addTextContentItem('contentBack')}
					onRemoveItem={(itemId) => removeTextContentItem('contentBack', itemId)}
					onReorder={(fromIndex, toIndex) =>
						reorderTextContentItems('contentBack', fromIndex, toIndex)}
					onUpdateText={(itemId, nextText) =>
						updateTextContentItem('contentBack', itemId, nextText)}
				/>
			</div>
		{/if}
	</section>
</div>

<style>
	:root {
		--editor-border: var(--color-sage);
		--editor-border-strong: var(--color-terracotta-light);
		--editor-surface: var(--primary-foreground);
		--editor-surface-soft: var(--color-cream);
		--editor-text-soft: var(--color-text-light);
	}

	.edit-page {
		display: grid;
		grid-template-columns: 22rem minmax(0, 1fr);
		gap: 1.5rem;
		min-height: 100vh;
		padding: 1.5rem;
	}

	.sidebar,
	.editor-shell {
		min-height: 0;
		background: var(--editor-surface);
		border: 0.0625rem solid var(--editor-border);
		border-radius: 1.5rem;
		box-shadow: var(--shadow);
	}

	.sidebar {
		display: grid;
		grid-template-rows: auto auto minmax(0, 1fr) auto;
		gap: 1rem;
		padding: 1.25rem;
	}

	.sidebar__hero {
		display: grid;
		gap: 0.375rem;
		padding: 0.375rem;
	}

	.sidebar__eyebrow,
	.editor-header__eyebrow {
		font-size: 0.75rem;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--editor-text-soft);
	}

	.sidebar__title,
	.editor-header__title {
		font-size: 1.75rem;
		line-height: 1.1;
		color: var(--text);
	}

	.sidebar__subtitle,
	.editor-header__subtitle {
		font-size: 0.9375rem;
		line-height: 1.4;
		color: var(--editor-text-soft);
	}

	.searchbar {
		display: grid;
		grid-template-columns: auto 1fr;
		align-items: center;
		gap: 0.75rem;
		padding: 0.875rem 1rem;
		background: var(--editor-surface-soft);
		border: 0.0625rem solid var(--editor-border);
		border-radius: 1rem;
	}

	.searchbar__icon {
		inline-size: 1.125rem;
		block-size: 1.125rem;
		color: var(--color-terracotta);
	}

	.searchbar__icon svg {
		inline-size: 100%;
		block-size: 100%;
		stroke: currentColor;
		fill: none;
		stroke-width: 2;
		stroke-linecap: round;
		stroke-linejoin: round;
	}

	.searchbar input {
		border: 0;
		outline: none;
		background: transparent;
		font-size: 1rem;
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
		background: var(--editor-surface);
		border: 0.0625rem solid var(--editor-border);
		border-radius: 1.125rem;
		transition:
			transform 0.18s ease,
			border-color 0.18s ease,
			box-shadow 0.18s ease;
	}

	.card-tile:hover {
		transform: translateY(-0.0625rem);
		border-color: var(--editor-border-strong);
		box-shadow: var(--shadow);
	}

	.card-tile.selected {
		border-color: var(--color-terracotta);
		background: var(--editor-surface-soft);
	}

	.card-tile__top-row {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.75rem;
	}

	.card-tile__index,
	.card-tile__side-label {
		font-size: 0.75rem;
		font-weight: 600;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: var(--editor-text-soft);
	}

	.card-tile__selected-badge,
	.status-pill {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 1.75rem;
		padding-inline: 0.75rem;
		border-radius: 999rem;
		font-size: 0.8125rem;
		font-weight: 600;
		background: var(--color-sage-hover);
		color: var(--text);
	}

	.status-pill.dirty,
	.card-tile__selected-badge {
		background: var(--color-terracotta);
		color: var(--primary-foreground);
	}

	.card-tile__preview-group {
		display: grid;
		gap: 0.25rem;
	}

	.card-tile__preview {
		font-size: 0.9375rem;
		line-height: 1.45;
		color: var(--text);
	}

	.cards-list__empty {
		display: grid;
		place-items: center;
		min-block-size: 9rem;
		padding: 1rem;
		border: 0.0625rem dashed var(--editor-border);
		border-radius: 1rem;
		color: var(--editor-text-soft);
		background: var(--editor-surface-soft);
		text-align: center;
	}

	.create-card-button,
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

	.create-card-button,
	.primary-button {
		background: var(--primary);
		color: var(--primary-foreground);
		border: 0.0625rem solid var(--primary);
	}

	.create-card-button:hover,
	.primary-button:hover {
		background: var(--primary-hov);
		border-color: var(--primary-hov);
		transform: translateY(-0.0625rem);
	}

	.secondary-button {
		background: var(--editor-surface);
		color: var(--text);
		border: 0.0625rem solid var(--editor-border-strong);
	}

	.secondary-button:hover {
		background: var(--color-sage-hover);
		transform: translateY(-0.0625rem);
	}

	.create-card-button:disabled,
	.primary-button:disabled,
	.secondary-button:disabled {
		opacity: 0.5;
		transform: none;
	}

	.editor-shell {
		display: grid;
		grid-template-rows: auto minmax(0, 1fr);
		gap: 1.25rem;
		padding: 1.5rem;
	}

	.editor-header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 1rem;
		padding-bottom: 0.5rem;
		border-bottom: 0.0625rem solid var(--editor-border);
	}

	.editor-header__actions {
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
	}

	.empty-state {
		display: grid;
		justify-items: center;
		align-content: center;
		gap: 0.75rem;
		min-height: 100%;
		padding: 2rem;
		text-align: center;
		background: var(--editor-surface-soft);
		border: 0.0625rem dashed var(--editor-border);
		border-radius: 1.5rem;
	}

	.empty-state--error {
		border-color: var(--red-4);
		background: var(--red-1);
	}

	.empty-state__icon {
		display: grid;
		place-items: center;
		inline-size: 4rem;
		block-size: 4rem;
		border-radius: 999rem;
		background: var(--color-sage-hover);
		font-size: 1.625rem;
		font-weight: 700;
		color: var(--color-terracotta);
	}

	.empty-state h2 {
		font-size: 1.5rem;
		line-height: 1.2;
	}

	.empty-state p {
		max-inline-size: 28rem;
		font-size: 1rem;
		line-height: 1.5;
		color: var(--editor-text-soft);
	}
</style>
