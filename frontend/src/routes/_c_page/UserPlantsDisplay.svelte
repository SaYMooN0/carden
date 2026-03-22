<script lang="ts">
	import type { PlantPreview } from '../types';
	import PlantsEmptyState from './PlantsEmptyState.svelte';

	interface Props {
		loadedPlants: PlantPreview[];
	}

	let { loadedPlants }: Props = $props();
</script>

{#if loadedPlants.length === 0}
	<PlantsEmptyState />
{:else}
	<div class="plants-list">
		{#each loadedPlants as plant}
			<p>{plant.name}</p>
		{/each}
		<div>Add New Plant</div>
	</div>
{/if}
