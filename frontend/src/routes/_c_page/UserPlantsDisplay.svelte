<script lang="ts">
	import type { PlantPreview } from '../../lib/ts/base-types';
	import AddPlantButton from './_c_plants_display/AddPlantButton.svelte';
	import AddNewPlantDialog from './_c_plants_display/AddNewPlantDialog.svelte';
	import PlantsEmptyState from './_c_plants_display/PlantsEmptyState.svelte';
	import SinglePlantDisplay from './_c_plants_display/SinglePlantDisplay.svelte';

	interface Props {
		loadedPlants: PlantPreview[];
	}

	let { loadedPlants }: Props = $props();
	let addNewPlantDialog: AddNewPlantDialog = $state()!;
</script>

<AddNewPlantDialog bind:this={addNewPlantDialog} />
{#if loadedPlants.length === 0}
	<PlantsEmptyState openAddNewPlantDialog={() => addNewPlantDialog.open()} />
{:else}
	<div class="plants-list">
		{#each loadedPlants as plant}
			<SinglePlantDisplay {plant} />
		{/each}
	</div>
	<div class="add-plant-button-wrapper">
		<AddPlantButton onClick={() => addNewPlantDialog.open()} />
	</div>
{/if}

<style>
	.add-plant-button-wrapper {
		position: fixed;
		bottom: 1rem;
		right: 1rem;
	}
	.plants-list {
		display: flex;
		flex-wrap: wrap;
		gap: 1rem;
	}
</style>
