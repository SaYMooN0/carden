<script lang="ts">
	import { goto } from '$app/navigation';
	import DialogWithCloseButton from '$lib/components/dialogs/DialogWithCloseButton.svelte';
	import DefaultErrBlock from '$lib/components/errs/DefaultErrBlock.svelte';
	import { Backend, RJO } from '$lib/ts/backend';
	import { AllPlantSpicies, AllPotTypes, type PlantSpecie, type PotType } from '$lib/ts/base-types';
	import type { Err } from '$lib/ts/err';
	import { SpritesManager } from '$lib/ts/sprites-manager';

	let dialog: DialogWithCloseButton = $state()!;
	let selectedPlantIndex = $state(0);
	let selectedPotIndex = $state(0);
	let plantName = $state('');
	let creatingErrs: Err[] = $state([]);
	let isLoading = $state(false);
	const selectedPlant: PlantSpecie = $derived(AllPlantSpicies[selectedPlantIndex]);
	const selectedPot: PotType = $derived(AllPotTypes[selectedPotIndex]);
	const canSubmit = $derived(plantName.trim().length > 0);

	export function open() {
		dialog.open();
	}

	function mod(n: number, m: number): number {
		return ((n % m) + m) % m;
	}

	function prevIndex(current: number, total: number): number {
		return mod(current - 1, total);
	}

	function nextIndex(current: number, total: number): number {
		return mod(current + 1, total);
	}

	function prettify(value: string): string {
		return value.replace(/([a-z0-9])([A-Z])/g, '$1 $2');
	}

	function getRelativeOffset(index: number, selected: number, total: number): number {
		let diff = index - selected;

		if (diff > total / 2) {
			diff -= total;
		}
		if (diff < -total / 2) {
			diff += total;
		}

		return diff;
	}

	function selectPrevPlant() {
		selectedPlantIndex = prevIndex(selectedPlantIndex, AllPlantSpicies.length);
	}

	function selectNextPlant() {
		selectedPlantIndex = nextIndex(selectedPlantIndex, AllPlantSpicies.length);
	}

	function selectPrevPot() {
		selectedPotIndex = prevIndex(selectedPotIndex, AllPotTypes.length);
	}

	function selectNextPot() {
		selectedPotIndex = nextIndex(selectedPotIndex, AllPotTypes.length);
	}

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();

		if (!canSubmit) {
			return;
		}
		creatingErrs = [];
		let isLoading = true;
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
			goto(`/${response.data.id}/edit`);
		} else {
			creatingErrs = response.errs;
		}
	}
</script>

<DialogWithCloseButton bind:this={dialog} dialogId="add-new-plant-dialog">
	<form class="root" onsubmit={handleSubmit}>
		<div class="header">
			<h2>Add new plant</h2>
			<p>Create your next study plant by choosing a specie, a pot, and a name.</p>
		</div>

		<div class="section">
			<div class="section-title">
				<span>Plant specie</span>
			</div>

			<div class="carousel">
				<button
					class="side-hitbox left"
					type="button"
					aria-label="Previous plant"
					onclick={selectPrevPlant}
				></button>

				<div class="carousel-window">
					{#each AllPlantSpicies as specie, index (specie)}
						{@const offset = getRelativeOffset(index, selectedPlantIndex, AllPlantSpicies.length)}

						<button
							type="button"
							class={`carousel-item ${offset === 0 ? 'center' : ''} ${offset === -1 ? 'left' : ''} ${offset === 1 ? 'right' : ''} ${Math.abs(offset) > 1 ? 'hidden' : ''}`}
							aria-pressed={offset === 0}
							onclick={() => (selectedPlantIndex = index)}
						>
							<div class="item-card">
								<img
									class="item-sprite plant-sprite"
									src={SpritesManager.getLvl5PlantSprite(specie)}
									alt={prettify(specie)}
									draggable="false"
								/>
								<span>{prettify(specie)}</span>
							</div>
						</button>
					{/each}
				</div>

				<button
					class="side-hitbox right"
					type="button"
					aria-label="Next plant"
					onclick={selectNextPlant}
				></button>
			</div>
		</div>

		<div class="section">
			<div class="section-title">
				<span>Pot type</span>
			</div>

			<div class="carousel">
				<button
					class="side-hitbox left"
					type="button"
					aria-label="Previous pot"
					onclick={selectPrevPot}
				></button>

				<div class="carousel-window">
					{#each AllPotTypes as potType, index (potType)}
						{@const offset = getRelativeOffset(index, selectedPotIndex, AllPotTypes.length)}

						<button
							type="button"
							class={`carousel-item ${offset === 0 ? 'center' : ''} ${offset === -1 ? 'left' : ''} ${offset === 1 ? 'right' : ''} ${Math.abs(offset) > 1 ? 'hidden' : ''}`}
							aria-pressed={offset === 0}
							onclick={() => (selectedPotIndex = index)}
						>
							<div class="item-card">
								<img
									class="item-sprite pot-sprite"
									src={SpritesManager.getLvl3PotSprite(potType)}
									alt={prettify(potType)}
									draggable="false"
								/>
								<span>{prettify(potType)}</span>
							</div>
						</button>
					{/each}
				</div>

				<button
					class="side-hitbox right"
					type="button"
					aria-label="Next pot"
					onclick={selectNextPot}
				></button>
			</div>
		</div>

		<div class="section">
			<label class="name-label" for="plant-name-input">Plant name</label>
			<input
				id="plant-name-input"
				class="name-input"
				type="text"
				bind:value={plantName}
				maxlength="40"
				placeholder="Enter plant name"
				autocomplete="off"
			/>
		</div>

		<DefaultErrBlock errs={creatingErrs} />
		<div class="actions">
			<button class="submit-button" type="submit" disabled={!canSubmit}>
				{#if isLoading}
					Loading...
				{:else}
					Create plant
				{/if}
			</button>
		</div>
	</form>
</DialogWithCloseButton>

<style>
	.root {
		width: 32rem;
		display: flex;
		flex-direction: column;
		gap: 1.25rem;
		color: var(--text);
	}

	.header {
		display: flex;
		flex-direction: column;
		gap: 0.35rem;
	}

	.header h2 {
		font-size: 1.6rem;
		font-weight: 700;
		line-height: 1.2;
		margin: 0;
	}

	.header p {
		margin: 0;
		font-size: 0.95rem;
		line-height: 1.45;
		color: var(--color-text-light);
	}

	.preview-card {
		display: flex;
		flex-direction: column;
		gap: 0.85rem;
		padding: 1rem;
		border-radius: 1.5rem;
		background: linear-gradient(180deg, var(--color-cream) 0%, var(--color-sage-hover) 100%);
		border: 0.1rem solid var(--color-sage);
		box-shadow: var(--shadow);
	}

	.preview-scene {
		position: relative;
		height: 15rem;
		border-radius: 1.2rem;
		background:
			radial-gradient(circle at top, rgba(255, 255, 255, 0.95) 0%, transparent 60%),
			var(--color-cream);
		overflow: hidden;
	}

	.preview-scene::after {
		content: '';
		position: absolute;
		left: 50%;
		bottom: 1rem;
		transform: translateX(-50%);
		width: 9rem;
		height: 0.9rem;
		border-radius: 999rem;
		background: rgba(0, 0, 0, 0.12);
		filter: blur(0.4rem);
	}

	.plant-preview,
	.pot-preview {
		position: absolute;
		left: 50%;
		transform: translateX(-50%);
		user-select: none;
		pointer-events: none;
	}

	.plant-preview {
		bottom: 4rem;
		width: 9rem;
		max-width: 60%;
		z-index: 1;
	}

	.pot-preview {
		bottom: 1.25rem;
		width: 10rem;
		max-width: 65%;
		z-index: 2;
	}
	.meta-label {
		font-size: 0.75rem;
		color: var(--color-text-light);
	}

	.meta-value {
		font-size: 0.92rem;
		font-weight: 600;
	}

	.section {
		display: flex;
		flex-direction: column;
		gap: 0.65rem;
	}

	.section-title {
		font-size: 1rem;
		font-weight: 600;
	}

	.carousel {
		position: relative;
		height: 12rem;
	}

	.carousel-window {
		position: relative;
		height: 100%;
		overflow: hidden;
		border-radius: 1.35rem;
	}

	.carousel-item {
		position: absolute;
		top: 0.35rem;
		left: 50%;
		width: 10rem;
		transform-origin: center center;
		border: none;
		background: transparent;
		padding: 0;
		cursor: pointer;
		transition:
			transform 0.28s ease,
			opacity 0.28s ease,
			filter 0.28s ease;
	}

	.carousel-item.hidden {
		opacity: 0;
		pointer-events: none;
		transform: translateX(-50%) scale(0.7);
	}

	.carousel-item.center {
		transform: translateX(-50%) scale(1);
		z-index: 3;
		opacity: 1;
	}

	.carousel-item.left {
		transform: translateX(calc(-50% - 7.4rem)) scale(0.82);
		z-index: 2;
		opacity: 0.82;
	}

	.carousel-item.right {
		transform: translateX(calc(-50% + 7.4rem)) scale(0.82);
		z-index: 2;
		opacity: 0.82;
	}

	.item-card {
		width: 100%;
		height: 100%;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: 0.55rem;
		padding: 0.8rem;
		border-radius: 1.35rem;
		background: var(--color-cream);
		border: 0.12rem solid var(--color-sage);
		box-shadow: var(--shadow);
		transition:
			border-color 0.2s ease,
			background-color 0.2s ease;
	}

	.carousel-item.center .item-card {
		border-color: var(--primary);
		background: var(--color-sage-hover);
	}

	.carousel-item:not(.hidden):hover .item-card {
		background: var(--color-sage-hover);
	}

	.item-sprite {
		max-width: 100%;
		user-select: none;
		pointer-events: none;
	}

	.plant-sprite {
		aspect-ratio: 1/1.25;
	}

	.pot-sprite {
		aspect-ratio: 1/1;
	}

	.item-card span {
		font-size: 0.88rem;
		font-weight: 600;
		text-align: center;
		line-height: 1.2;
	}

	.side-hitbox {
		position: absolute;
		top: 0;
		bottom: 0;
		width: 5rem;
		border: none;
		background-color: red;
		cursor: pointer;
		z-index: 4;
	}

	.side-hitbox.left {
		left: 0;
	}

	.side-hitbox.right {
		right: 0;
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
		border: none;
		border-radius: 999rem;
		padding: 0.9rem 1.2rem;
		min-width: 10rem;
		font-size: 0.98rem;
		font-weight: 700;
		background: var(--primary);
		color: var(--primary-foreground);
		cursor: pointer;
		box-shadow: var(--shadow);
		transition:
			transform 0.18s ease,
			opacity 0.18s ease,
			background-color 0.18s ease;
	}

	.submit-button:hover:enabled {
		transform: translateY(-0.08rem);
		background: var(--primary-hov);
	}

	.submit-button:disabled {
		opacity: 0.55;
		cursor: not-allowed;
	}
</style>
