<script lang="ts">
	import AddPlantButton from './_c_plants_display/AddPlantButton.svelte';
	import AddNewPlantDialog from './_c_plants_display/AddNewPlantDialog.svelte';
	import PlantsEmptyState from './_c_plants_display/PlantsEmptyState.svelte';
	import SinglePlantDisplay from './_c_plants_display/SinglePlantDisplay.svelte';
	import type { PlantPreview } from './shared_types';

	interface Props {
		loadedPlants: PlantPreview[];
	}

	let { loadedPlants }: Props = $props();
	let addNewPlantDialog: AddNewPlantDialog = $state()!;
</script>

<AddNewPlantDialog bind:this={addNewPlantDialog} />

<section class="plants-page">
	{#if loadedPlants.length === 0}
		<div class="plants-page__empty">
			<PlantsEmptyState openAddNewPlantDialog={() => addNewPlantDialog.open()} />
		</div>
	{:else}
		<div class="plants-page__inner">
			<header class="plants-page__header">
				<div class="plants-page__header-copy">
					<h1 class="plants-page__title">Ваши растения ({loadedPlants.length})</h1>
					<p class="plants-page__description">
						Выберите растение для обучения или перейдите к редактированию его карточек.
					</p>
				</div>
			</header>
			<div class="plants-list">
				{#each loadedPlants as plant}
					<SinglePlantDisplay {plant} />
				{/each}
			</div>
		</div>
		<div class="plants-page__fab">
			<AddPlantButton onClick={() => addNewPlantDialog.open()} />
		</div>
	{/if}
</section>

<style>
	.plants-page {
		width: 100%;
		padding: 1.5rem 1rem 6rem;
	}

	.plants-page__inner {
		width: 100%;
		max-width: 82rem;
		margin: 0 auto;
		display: grid;
		gap: 1.5rem;
	}

	.plants-page__header {
		padding: 1.5rem;
		border: 0.125rem solid var(--color-sage);
		border-radius: 1.5rem;
		background: var(--color-sage-hover);
		box-shadow: var(--shadow);
	}

	.plants-page__header-copy {
		display: grid;
		gap: 0.5rem;
	}

	.plants-page__title {
		font-size: 2rem;
		line-height: 2.5rem;
		font-weight: 700;
		color: var(--text);
	}

	.plants-page__description {
		max-width: 40rem;
		font-size: 1rem;
		line-height: 1.5rem;
		color: var(--color-text-light);
	}

	.plants-page__empty {
		padding-top: 0.5rem;
	}

	.plants-list {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(17rem, 1fr));
		gap: 1.25rem;
		align-items: start;
	}

	.plants-page__fab {
		position: fixed;
		right: 1rem;
		bottom: 1rem;
		z-index: 10;
	}
</style>
