<script lang="ts">
	import type { PlantPreview } from '../../lib/ts/base-types';
	import AddNewPlantDialog from './_c_plants_display/AddNewPlantDialog.svelte';
	import PlantsEmptyState from './_c_plants_display/PlantsEmptyState.svelte';

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
			<p>{plant.name}</p>
		{/each}
		<div>Add New Plant</div>
	</div>
{/if}
