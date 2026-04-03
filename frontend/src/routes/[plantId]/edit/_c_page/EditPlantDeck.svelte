<script lang="ts">
	import type { PlantToEdit } from '../shared_types';
	import CardEditing from './_c_edit_card_deck/CardEditing.svelte';
	import CardEmptyState from './_c_edit_card_deck/CardEmptyState.svelte';
	import CardLoadingState from './_c_edit_card_deck/CardLoadingState.svelte';
	import CardsListSidebar from './_c_edit_card_deck/CardsListSidebar.svelte';
	import UnsavedChangesWarningDialog from './_c_edit_card_deck/UnsavedChangesWarningDialog.svelte';
	import { EditPlantPageState } from './edit-plant-page-state.svelte';

	interface Props {
		plant: PlantToEdit;
	}

	let { plant }: Props = $props();
	let unsavedChangesWarningDialog: UnsavedChangesWarningDialog = $state()!;

	let pageState = new EditPlantPageState(plant, {
		activate: (cardId) => unsavedChangesWarningDialog.open(cardId)
	});

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
<UnsavedChangesWarningDialog
	bind:this={unsavedChangesWarningDialog}
	onStay={() => unsavedChangesWarningDialog.close()}
	onDiscardAndContinue={(newCardId) => {
		pageState.resetCurrentCardChanges();
		pageState.selectCard(newCardId, { ignoreUnsavedChangesGuard: true });
		unsavedChangesWarningDialog.close();
	}}
/>

<div class="edit-page">
	<CardsListSidebar
		plantName={pageState.plantName}
		cardsCount={pageState.cardsCount}
		plantDeckCardsPreview={pageState.plantDeckCardsPreview}
		{selectedCardId}
		selectCard={(cardId) => pageState.selectCard(cardId, { ignoreUnsavedChangesGuard: false })}
		addNewCard={() => pageState.addNewCard()}
	/>
	<section class="editor-shell">
		{#if pageState.cardEditingState.state === 'NoCardSelected'}
			<CardEmptyState
				icon="✿"
				header="Select a card to start editing"
				text="Choose a card on the left and edit its front and back sides."
				button={{
					text: pageState.firstCardId ? 'Open first card' : 'Create first card',
					onClick: () => {
						if (pageState.firstCardId) {
							pageState.selectCard(pageState.firstCardId, { ignoreUnsavedChangesGuard: false });
							return;
						}
						pageState.addNewCard();
					}
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
			<CardLoadingState />
		{:else if pageState.cardEditingState.state === 'CardEditing'}
			<CardEditing
				bind:card={pageState.cardEditingState.card}
				saveCardChanges={() => pageState.saveCurrentCardChanges()}
				resetCardChanges={() => pageState.resetCurrentCardChanges()}
				cardHasUnsavedChanges={pageState.anyUnsavedChangesOnTheCurrentCard}
			/>
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
	.edit-page {
		display: grid;
		grid-template-columns: 22rem minmax(0, 1fr);
		gap: 1.5rem;
		block-size: 100vh;
		padding: 1.5rem;
		overflow: hidden;
	}

	.editor-shell {
		min-height: 0;
		overflow: hidden;
		background: var(--primary-foreground);
		border: 0.125rem solid var(--color-sage);
		border-radius: 1.5rem;
		box-shadow: var(--shadow);
		display: grid;
		grid-template-rows: auto minmax(0, 1fr);
		gap: 1.25rem;
		padding: 1.5rem;
	}
</style>
