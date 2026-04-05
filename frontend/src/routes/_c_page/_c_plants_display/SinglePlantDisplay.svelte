<script lang="ts">
	import { SpritesManager } from '$lib/ts/sprites-manager';
	import type { PlantPreview } from '../shared_types';

	interface Props {
		plant: PlantPreview;
	}

	let { plant }: Props = $props();

	let plantSpriteWithAttachments = $derived(
		SpritesManager.calculatePlantSpritesBasedOnStudyProgress(plant.plantSpecie, plant.studyProgress)
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

	<div class="plant-info">
		<h2 class="plant-name" title={plant.name}>{plant.name}</h2>

		<div class="plant-meta">
			<span class="meta-pill">Карточек: {plant.cardsCount}</span>
			<span class="meta-pill">Прогресс: {plant.studyProgress}</span>
		</div>
	</div>

	<div class="actions-container">
		<a class="action-btn action-btn--primary" href={`/${plant.id}/study`}>Учить</a>
		<a class="action-btn action-btn--secondary" href={`/${plant.id}/edit`}>Редактировать</a>
	</div>
</div>

<style>
	.plant-container {
		width: 100%;
		max-width: 16rem;
		display: grid;
		gap: 1rem;
		padding: 1rem;
		border: 0.125rem solid var(--color-sage);
		border-radius: 1.5rem;
		background: var(--color-cream);
		box-shadow: var(--shadow);
		transition:
			transform 0.2s ease,
			border-color 0.2s ease,
			background-color 0.2s ease;
	}

	.plant-container:hover {
		transform: translateY(-0.125rem);
		border-color: var(--color-terracotta-light);
		background: var(--color-cream);
	}

	.sprites-stage {
		position: relative;
		width: 100%;
		aspect-ratio: 1 / 2.25;
		border-radius: 1.25rem;
		overflow: hidden;
		background: var(--color-cream);
	}
	.plant-shell {
		position: absolute;
		top: 0;
		z-index: 2;
		display: block;
		width: 100%;
		aspect-ratio: 1 / 1.25;
		pointer-events: none;
		transform: translateY(calc(var(--plants-y-offset-percent) * 1%));
	}

	.pot-sprite {
		position: absolute;
		bottom: 0;
		z-index: 1;
		width: 100%;
		aspect-ratio: 1 / 1;
		pointer-events: none;
	}

	.main-plant-sprite,
	.plant-attachment,
	.pot-sprite {
		width: 100%;
		object-fit: contain;
		user-select: none;
		-webkit-user-drag: none;
	}

	.plant-shell > img {
		position: absolute;
		inset: 0;
	}

	.plant-info {
		display: grid;
		gap: 0.5rem;
	}

	.plant-name {
		overflow: hidden;
		font-size: 1.125rem;
		line-height: 1.375rem;
		font-weight: 700;
		color: var(--text);
		text-align: center;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.plant-meta {
		display: flex;
		flex-wrap: wrap;
		justify-content: center;
		gap: 0.5rem;
	}

	.meta-pill {
		padding: 0.375rem 0.75rem;
		border-radius: 999rem;
		background: var(--color-sage-hover);
		border: 0.125rem solid var(--color-sage);
		font-size: 0.75rem;
		font-weight: 600;
		line-height: 1rem;
		color: var(--color-text-light);
	}

	.actions-container {
		display: grid;
		grid-template-columns: 1fr;
		gap: 0.625rem;
	}

	.action-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		min-height: 2.75rem;
		padding: 0.75rem 1rem;
		border: 0.125rem solid transparent;
		border-radius: 999rem;
		font-size: 1rem;
		font-weight: 700;
		text-decoration: none;
		cursor: pointer;
		transition:
			transform 0.2s ease,
			background-color 0.2s ease,
			border-color 0.2s ease,
			color 0.2s ease;
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
		border-color: var(--color-sage);
		background: var(--color-cream);
		color: var(--text);
	}

	.action-btn--secondary:hover {
		background: var(--color-sage-hover);
	}
</style>
