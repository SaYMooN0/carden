<script lang="ts">
	import type { PlantPreview } from '$lib/ts/base-types';
	import { SpritesManager } from '$lib/ts/sprites-manager';

	interface Props {
		plant: PlantPreview;
	}

	let { plant }: Props = $props();

	let plantSpriteWithAttachments = $derived(
		SpritesManager.calculatePlantSpritesBasedOnStudyProgress(plant.plantSpecie, 222)
	);

	let plantsYOffsetPercent = $derived(
		SpritesManager.getPotYOffsetBasedOnCardsCount(plant.potType, plant.cardsCount)
	);
</script>

<div class="plant-container" style:--plants-y-offset-percent={plantsYOffsetPercent}>
	<div class="sprites-stage">
		<div class="plant-shell">
			<img
				class="plant-sprite main-plant-sprite"
				src={plantSpriteWithAttachments.mainSprite}
				alt={plant.name}
			/>

			{#each plantSpriteWithAttachments.attachments as attachment}
				<img class="plant-attachment" src={attachment} alt="" aria-hidden="true" />
			{/each}
		</div>

		<img
			class="pot-sprite"
			src={SpritesManager.calculatePotSpryteBasedOnCardsCount(plant.potType, plant.cardsCount)}
			alt={`Pot for ${plant.name}`}
		/>
	</div>

	<div class="actions-container">
		<a class="action-btn action-btn--primary" href={`/${plant.id}/study`}>Study</a>
		<a class="action-btn action-btn--secondary" href={`/${plant.id}/edit`}>Edit</a>
	</div>
</div>

<style>
	.plant-container {
		--plant-card-bg: var(--color-sage-hover);
		--plant-card-border: var(--color-sage);
		--plant-stage-bg: var(--color-cream);
		--plant-stage-ring: var(--color-sage);
		--plant-ground: var(--color-sage);
		--plant-ground-shadow: var(--color-terracotta-light);

		width: 100%;
		display: grid;
		gap: 1rem;
		padding: 1rem;
		border: 0.125rem solid var(--plant-card-border);
		border-radius: 1.5rem;
		background: var(--plant-card-bg);
		box-shadow: var(--shadow);
		max-width: 16rem;
	}

	.sprites-stage {
		position: relative;
		width: 100%;
		border: 0.125rem solid var(--plant-stage-ring);
		border-radius: 1.25rem;
		background: var(--plant-stage-bg);
		overflow: hidden;
		width: 100%;
		aspect-ratio: 1/2.25;
	}

	.plant-shell {
		position: absolute;
		display: block;
		width: 100%;
		aspect-ratio: 1 / 1.25;
		z-index: 2;
		pointer-events: none;
		top: 0;
		transform: translateY(calc(var(--plants-y-offset-percent) * 1%));
	}

	.pot-sprite {
		position: absolute;
		bottom: 0;
		width: 100%;
		aspect-ratio: 1 / 1;
		z-index: 1;
		pointer-events: none;
	}

	.main-plant-sprite,
	.plant-attachment,
	.pot-sprite {
		object-fit: contain;
		user-select: none;
		-webkit-user-drag: none;
		width: 100%;
	}

	.plant-shell > img {
		position: absolute;
		inset: 0;
	}

	.actions-container {
		display: grid;
		grid-template-columns: 1fr;
		gap: 0.75rem;
	}

	.action-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		min-height: 2.75rem;
		padding: 0.75rem 1rem;
		border-radius: 999rem;
		border: 0.125rem solid transparent;
		font-size: 1rem;
		font-weight: 700;
		text-decoration: none;
		cursor: pointer;
		transition:
			transform 0.2s ease,
			background-color 0.2s ease,
			border-color 0.2s ease;
	}

	.action-btn:hover {
		transform: translateY(-0.125rem);
	}

	.action-btn--primary {
		background: var(--primary);
		color: var(--primary-foreground);
	}

	.action-btn--primary:hover {
		background: var(--primary-hov);
	}

	.action-btn--secondary {
		background: var(--color-cream);
		color: var(--text);
		border-color: var(--color-sage);
	}

	.action-btn--secondary:hover {
		background: var(--color-sage-hover);
	}
</style>
