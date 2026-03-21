<script lang="ts">
	import type { PlantPreview } from '../types';

	interface Props {
		loadedPlants: PlantPreview[];
	}

	let { loadedPlants }: Props = $props();
</script>

{#if loadedPlants.length === 0}
	<p>No plants found</p>
{:else}
	{#each loadedPlants as plant}
		<p>{plant.name}</p>
	{/each}
{/if}
