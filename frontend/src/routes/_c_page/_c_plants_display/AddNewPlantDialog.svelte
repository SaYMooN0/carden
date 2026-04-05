<script lang="ts">
	import { goto } from '$app/navigation';
	import DialogWithCloseButton from '$lib/components/dialogs/DialogWithCloseButton.svelte';
	import DefaultErrBlock from '$lib/components/errs/DefaultErrBlock.svelte';
	import { Backend, RJO } from '$lib/ts/backend';
	import { AllPlantSpicies, AllPotTypes, type PlantSpecie, type PotType } from '$lib/ts/base-types';
	import type { Err } from '$lib/ts/err';
	import { SpritesManager } from '$lib/ts/sprites-manager';
	import SpriteCarousel from './_c_new_plant_dialog/SpriteCarousel.svelte';

	let dialog: DialogWithCloseButton = $state()!;
	let selectedPlantIndex = $state(0);
	let selectedPotIndex = $state(0);
	let plantName = $state('');
	let creatingErrs: Err[] = $state([]);
	let isLoading = $state(false);

	const plantNameMaxLength = 40;

	const selectedPlant: PlantSpecie = $derived(AllPlantSpicies[selectedPlantIndex]);
	const selectedPot: PotType = $derived(AllPotTypes[selectedPotIndex]);
	const canSubmit = $derived(plantName.trim().length > 0 && !isLoading);

	function prettify(value: string): string {
		return value.replace(/([a-z0-9])([A-Z])/g, '$1 $2');
	}

	function resetForm() {
		selectedPlantIndex = 0;
		selectedPotIndex = 0;
		plantName = '';
		creatingErrs = [];
		isLoading = false;
	}

	export function open() {
		resetForm();
		dialog.open();
	}

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();

		if (!canSubmit) {
			return;
		}

		creatingErrs = [];
		isLoading = true;

		const response = await Backend.fetchJsonResponse<{ id: string }>(
			'/plants/create',
			RJO.POST({
				name: plantName.trim(),
				plantSpecie: selectedPlant,
				potType: selectedPot
			})
		);

		isLoading = false;

		if (response.isSuccess) {
			dialog.close();
			resetForm();
			await goto(`/${response.data.id}/edit`);
			return;
		}

		creatingErrs = response.errs;
	}
</script>

<DialogWithCloseButton bind:this={dialog} dialogId="add-new-plant-dialog">
	<form class="root" onsubmit={handleSubmit}>
		<div class="header">
			<h2>Добавить новое растение</h2>
			<p>Выберите вид растения, горшок и придумайте название.</p>
		</div>

		<div class="selection-summary" aria-live="polite">
			<div class="summary-chip">
				<span class="summary-label">Растение</span>
				<span class="summary-value">{prettify(selectedPlant)}</span>
			</div>

			<div class="summary-chip">
				<span class="summary-label">Горшок</span>
				<span class="summary-value">{prettify(selectedPot)}</span>
			</div>
		</div>

		<SpriteCarousel
			title="Вид растения"
			items={AllPlantSpicies}
			bind:selectedIndex={selectedPlantIndex}
			getKey={(specie) => specie}
			getLabel={prettify}
			getImageSrc={(specie) => SpritesManager.getLvl5PlantSprite(specie)}
			itemAspectRatio={0.8}
			itemWidth="10rem"
			previousAriaLabel="Предыдущее растение"
			nextAriaLabel="Следующее растение"
		/>
		<SpriteCarousel
			title="Горшок"
			items={AllPotTypes}
			bind:selectedIndex={selectedPotIndex}
			getKey={(potType: PotType) => potType}
			getLabel={(potType: PotType) => prettify(potType)}
			getImageSrc={(potType: PotType) => SpritesManager.getLvl3PotSprite(potType)}
			itemAspectRatio={1}
			itemWidth="10rem"
			previousAriaLabel="Предыдущий горшок"
			nextAriaLabel="Следующий горшок"
		/>

		<div class="section">
			<label class="name-label" for="plant-name-input">Название растения</label>
			<input
				id="plant-name-input"
				class="name-input"
				type="text"
				bind:value={plantName}
				maxlength={plantNameMaxLength}
				placeholder="Введите название растения"
				autocomplete="off"
			/>
		</div>

		<DefaultErrBlock errs={creatingErrs} />

		<div class="actions">
			<button class="submit-button" type="submit" disabled={!canSubmit}>
				{#if isLoading}
					Загрузка...
				{:else}
					Создать растение
				{/if}
			</button>
		</div>
	</form>
</DialogWithCloseButton>

<style>
	.root {
		display: flex;
		flex-direction: column;
		gap: 1.25rem;
		color: var(--text);
	}

	.header {
		display: flex;
		flex-direction: column;
		gap: 0.375rem;
	}

	.header h2 {
		margin: 0;
		font-size: 1.625rem;
		font-weight: 700;
		line-height: 1.2;
	}

	.header p {
		margin: 0;
		font-size: 0.95rem;
		line-height: 1.5;
		color: var(--color-text-light);
	}

	.selection-summary {
		display: flex;
		flex-wrap: wrap;
		gap: 0.75rem;
	}

	.summary-chip {
		flex: 1 1 12rem;
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
		padding: 0.875rem 1rem;
		border-radius: 1rem;
		border: 0.125rem solid var(--color-sage);
		background: var(--color-sage-hover);
		box-shadow: var(--shadow);
	}

	.summary-label {
		font-size: 0.75rem;
		font-weight: 600;
		color: var(--color-text-light);
	}

	.summary-value {
		font-size: 0.95rem;
		font-weight: 700;
		line-height: 1.3;
	}

	.section {
		display: flex;
		flex-direction: column;
		gap: 0.625rem;
	}

	.name-label {
		font-size: 1rem;
		font-weight: 600;
	}

	.name-input {
		width: 100%;
		padding: 1rem 1.125rem;
		border-radius: 1rem;
		border: 0.125rem solid var(--color-sage);
		background: var(--color-cream);
		color: var(--text);
		font-size: 1rem;
		line-height: 1.3;
		outline: none;
		transition:
			border-color 0.18s ease,
			background-color 0.18s ease,
			box-shadow 0.18s ease;
	}

	.name-input:focus {
		border-color: var(--primary);
		background: var(--color-sage-hover);
	}

	.actions {
		display: flex;
		justify-content: flex-end;
	}

	.submit-button {
		min-width: 10.5rem;
		padding: 0.875rem 1.25rem;
		border: none;
		border-radius: 999rem;
		background: var(--primary);
		color: var(--primary-foreground);
		font-size: 1rem;
		font-weight: 700;
		box-shadow: var(--shadow);
		cursor: pointer;
		transition:
			transform 0.18s ease,
			opacity 0.18s ease,
			background-color 0.18s ease;
	}

	.submit-button:hover:enabled {
		transform: translateY(-0.125rem);
		background: var(--primary-hov);
	}

	.submit-button:disabled {
		opacity: 0.55;
		cursor: not-allowed;
	}
</style>
