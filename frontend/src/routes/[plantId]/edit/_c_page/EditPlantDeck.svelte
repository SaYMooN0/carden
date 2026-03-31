<script lang="ts">
	import type { Plant, CardContentItem, Card } from '$lib/ts/base-types';
	import CardEditing from './_edit_card_deck/CardEditing.svelte';
	import CardEmptyState from './_edit_card_deck/CardEmptyState.svelte';
	import CardsListSidebar from './_edit_card_deck/CardsListSidebar.svelte';
	import { EditPlantPageState, type CardContentWithStringId } from './edit-plant-page-state.svelte';
	import SortableTextContentList from './SortableTextContentList.svelte';

	type EditableSide = 'contentFront' | 'contentBack';

	interface Props {
		plant: Plant;
	}

	let { plant }: Props = $props();
	let pageState = $state(new EditPlantPageState(plant));

	const selectedCardId = $derived.by(() => {
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
</script>

<svelte:head>
	<title>Edit plant deck</title>
</svelte:head>

<div class="edit-page">
	<CardsListSidebar
		plantName={pageState.plantName}
		cardsCount={pageState.cardsCount}
		plantDeckCardsPreview={pageState.plantDeckCardsPreview}
		{selectedCardId}
		selectCard={(cardId) => pageState.selectCard(cardId)}
		addNewCard={() => pageState.addNewCard()}
	/>
	<section class="editor-shell">
		{#if pageState.cardEditingState.state === 'NoCardSelected'}
			<CardEmptyState
				icon="✿"
				header="Select a card to start editing"
				text="Choose a card on the left and edit its front and back sides."
				button={{
					text: 'Open first card',
					onClick: () => pageState.selectCard(pageState.firstCardId)
				}}
			/>
		{:else if pageState.cardEditingState.state === 'ExpectedCardNotFound'}
			{@const notFoundCardId = pageState.cardEditingState.cardId}
			<CardEmptyState
				icon="!"
				header="Card not found"
				text={`Expected card id: ${notFoundCardId}`}
				button={{
					text: 'Try to load again',
					onClick: () => pageState.reloadCard(notFoundCardId)
				}}
			/>
		{:else if pageState.cardEditingState.state === 'CardReloading'}
			<CardEmptyState
				icon="⏳"
				header="Loading card..."
				text={`Loading card ${pageState.cardEditingState.cardId}...`}
				button={{
					text: 'Cancel',
					onClick: () => pageState.selectCard(pageState.firstCardId)
				}}
			/>
		{:else if pageState.cardEditingState.state === 'CardEditing'}
			<CardEditing />
		{:else}
			<CardEmptyState
				icon="?"
				header="Unknown state"
				text="Something went wrong. Please try to reload the page."
				button={{ text: 'Reload', onClick: () => window.location.reload() }}
			/>
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

	.sidebar__eyebrow {
		font-size: 0.75rem;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--editor-text-soft);
	}

	.sidebar__title {
		font-size: 1.75rem;
		line-height: 1.1;
		color: var(--text);
		text-overflow: ellipsis;
		overflow: hidden;
		white-space: nowrap;
	}

	.sidebar__subtitle {
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
