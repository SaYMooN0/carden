<script>
	import RegistrationForm from '$lib/RegistrationForm.svelte';
	import PlantModal from '$lib/PlantModal.svelte';

	let isAuthenticated = $state(false);

	let myPlantModal = $state();
</script>

{#if isAuthenticated}
	<button onclick={myPlantModal.open}> Новое растение </button>
	<PlantModal bind:this={myPlantModal} />
{:else}
	<RegistrationForm onLogin={() => (isAuthenticated = true)} />
{/if}
