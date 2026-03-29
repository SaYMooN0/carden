<script lang="ts">
	import type { Plant, CardContentItem, Card } from '$lib/ts/base-types';
	import { EditPlantPageState, type CardContentWithStringId } from './edit-plant-page-state.svelte';
	import SortableTextContentList from './SortableTextContentList.svelte';

	type EditableSide = 'contentFront' | 'contentBack';
	type PendingNavigation = { type: 'select-card'; cardId: string } | { type: 'new-card' } | null;

	interface Props {
		plant: Plant;
	}

	let { plant }: Props = $props();
	let pageState = $state(new EditPlantPageState(plant));
	let pendingNavigation = $state<PendingNavigation>(null);

	const editingCard = $derived(
		pageState.cardEditingState.state === 'CardEditing' ? pageState.cardEditingState.card : null
	);

	const selectedCardId: string | null = $derived.by(() => {
		if (pageState.cardEditingState.state === 'CardEditing') {
			return pageState.cardEditingState.card.id;
		}
		if (
			pageState.cardEditingState.state === 'ExpectedCardNotFound' ||
			pageState.cardEditingState.state === 'CardReloading'
		) {
			return pageState.cardEditingState.cardId;
		}
		return null;
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

	function formatDateTime(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) {
			return value;
		}
		return date.toLocaleString();
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

	function moveTextContentItem(
		fromSide: EditableSide,
		toSide: EditableSide,
		fromIndex: number,
		toIndex: number
	) {
		if (!editingCard) {
			return;
		}

		if (fromSide === toSide) {
			const nextItems = [...editingCard[fromSide]];
			const [movedItem] = nextItems.splice(fromIndex, 1);

			if (!movedItem) {
				return;
			}

			nextItems.splice(toIndex, 0, movedItem);
			editingCard[fromSide] = nextItems;
			return;
		}

		const nextSourceItems = [...editingCard[fromSide]];
		const nextTargetItems = [...editingCard[toSide]];
		const [movedItem] = nextSourceItems.splice(fromIndex, 1);

		if (!movedItem) {
			return;
		}

		nextTargetItems.splice(toIndex, 0, movedItem);
		editingCard[fromSide] = nextSourceItems;
		editingCard[toSide] = nextTargetItems;
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
			lastTimeEdited: new Date().toISOString(),
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

	function requestSelectCard(cardId: string) {
		if (selectedCardId === cardId) {
			return;
		}

		if (hasUnsavedChanges) {
			pendingNavigation = { type: 'select-card', cardId };
			return;
		}

		pageState.selectCard(cardId);
	}

	function requestNewCard() {
		if (hasUnsavedChanges) {
			pendingNavigation = { type: 'new-card' };
			return;
		}

		pageState.addNewCard();
	}

	function closePendingNavigation() {
		pendingNavigation = null;
	}

	function applyPendingNavigation() {
		if (!pendingNavigation) {
			return;
		}

		if (pendingNavigation.type === 'select-card') {
			pageState.selectCard(pendingNavigation.cardId);
		} else {
			pageState.addNewCard();
		}

		pendingNavigation = null;
	}

	function discardAndContinue() {
		applyPendingNavigation();
	}

	function saveAndContinue() {
		saveCurrentCard();
		applyPendingNavigation();
	}
</script>

<svelte:head>
	<title>Edit plant deck</title>
</svelte:head>

<div class="edit-page">
	<aside class="sidebar">
		<div class="sidebar__hero">
			<div class="sidebar__eyebrow">plant deck editor</div>
			<h1 class="sidebar__title" title={pageState.plant.name}>{pageState.plant.name}</h1>
			<p class="sidebar__subtitle">
				{pageState.plant.deck.cards.length}
				{pageState.plant.deck.cards.length === 1 ? ' card in deck' : ' cards in deck'}
			</p>
		</div>

		<div class="cards-list">
			{#each pageState.plant.deck.cards as card (card.id)}
				<button
					class:selected={selectedCardId === card.id}
					class="card-tile"
					type="button"
					onclick={() => requestSelectCard(card.id)}
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
					<p>There are no cards in this deck yet.</p>
				</div>
			{/each}
		</div>

		<button class="create-card-button" type="button" onclick={requestNewCard}>
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
						onclick={() => requestSelectCard(pageState.plant.deck.cards[0].id)}
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
					<p class="editor-header__meta">
						last edited {formatDateTime(editingCard.lastTimeEdited)}
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
					group={`card-${editingCard.id}`}
					listId="contentFront"
					items={editingCard.contentFront}
					title="front side"
					subtitle="Question, prompt or title"
					onAddItem={() => addTextContentItem('contentFront')}
					onRemoveItem={(itemId) => removeTextContentItem('contentFront', itemId)}
					onMoveItem={moveTextContentItem}
					onUpdateText={(itemId, nextText) =>
						updateTextContentItem('contentFront', itemId, nextText)}
				/>

				<SortableTextContentList
					group={`card-${editingCard.id}`}
					listId="contentBack"
					items={editingCard.contentBack}
					title="back side"
					subtitle="Answer, explanation or extra note"
					onAddItem={() => addTextContentItem('contentBack')}
					onRemoveItem={(itemId) => removeTextContentItem('contentBack', itemId)}
					onMoveItem={moveTextContentItem}
					onUpdateText={(itemId, nextText) =>
						updateTextContentItem('contentBack', itemId, nextText)}
				/>
			</div>
		{/if}
	</section>

	{#if pendingNavigation}
		<div class="pending-dialog-backdrop" role="presentation">
			<div aria-modal="true" class="pending-dialog" role="dialog">
				<h3 class="pending-dialog__title">Unsaved changes</h3>
				<p class="pending-dialog__text">
					If you switch the card now, your current changes will be lost. Save them first or discard
					them and continue.
				</p>

				<div class="pending-dialog__actions">
					<button class="secondary-button" type="button" onclick={closePendingNavigation}>
						stay here
					</button>
					<button class="danger-button" type="button" onclick={discardAndContinue}>
						discard changes
					</button>
					<button class="primary-button" type="button" onclick={saveAndContinue}>
						save and continue
					</button>
				</div>
			</div>
		</div>
	{/if}
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
		block-size: 100vh;
		padding: 1.5rem;
		overflow: hidden;
	}

	.sidebar,
	.editor-shell {
		min-height: 0;
		overflow: hidden;
		background: var(--editor-surface);
		border: 0.0625rem solid var(--editor-border);
		border-radius: 1.5rem;
		box-shadow: var(--shadow);
	}

	.sidebar {
		display: grid;
		grid-template-rows: auto minmax(0, 1fr) auto;
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
		text-overflow: ellipsis;
		overflow: hidden;
		white-space: nowrap;
	}

	.sidebar__subtitle,
	.editor-header__meta {
		font-size: 0.9375rem;
		line-height: 1.4;
		color: var(--editor-text-soft);
	}

	.editor-header__meta {
		margin-top: 0.375rem;
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
		width: 100%;
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
	.secondary-button,
	.danger-button {
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

	.danger-button {
		background: var(--red-1);
		color: var(--red-5);
		border: 0.0625rem solid var(--red-3);
	}

	.danger-button:hover {
		background: var(--red-2);
		transform: translateY(-0.0625rem);
	}

	.create-card-button:disabled,
	.primary-button:disabled,
	.secondary-button:disabled,
	.danger-button:disabled {
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
		height: 100%;
		overflow: hidden;
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

	.pending-dialog-backdrop {
		position: fixed;
		inset: 0;
		display: grid;
		place-items: center;
		padding: 1.5rem;
		background: color-mix(in srgb, var(--text) 22%, transparent);
	}

	.pending-dialog {
		display: grid;
		gap: 1rem;
		inline-size: min(100%, 32rem);
		padding: 1.5rem;
		background: var(--primary-foreground);
		border: 0.0625rem solid var(--color-sage);
		border-radius: 1.5rem;
		box-shadow: var(--shadow);
	}

	.pending-dialog__title {
		font-size: 1.375rem;
		line-height: 1.2;
	}

	.pending-dialog__text {
		font-size: 1rem;
		line-height: 1.5;
		color: var(--color-text-light);
	}

	.pending-dialog__actions {
		display: flex;
		flex-wrap: wrap;
		justify-content: flex-end;
		gap: 0.75rem;
	}
</style>
