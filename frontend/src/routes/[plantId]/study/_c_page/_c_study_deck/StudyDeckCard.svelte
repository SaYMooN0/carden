<script lang="ts">
	import { toast } from 'svelte-sonner';

	interface Props {
		frontTexts: string[];
		backTexts: string[];
		currentSide: 'Front' | 'Back';
		onFlip: () => void;
	}

	let { frontTexts, backTexts, currentSide, onFlip }: Props = $props();

	const isBackShown = $derived(currentSide === 'Back');
</script>

<button class="study-card-shell" type="button" onclick={onFlip}>
	<div class:flipped={isBackShown} class="study-card-inner">
		<section class="study-card-face study-card-face-front">
			<div class="study-card-face-header">
				<span class="face-pill">Лицевая</span>
				<span class="face-hint">Нажмите, чтобы перевернуть</span>
			</div>

			<div class="study-card-content">
				{#each frontTexts as text}
					<p>{text}</p>
				{/each}
			</div>
		</section>

		<section class="study-card-face study-card-face-back">
			<div class="study-card-face-header">
				<span class="face-pill">Обратная</span>
				<span class="face-hint">Нажмите, чтобы перевернуть</span>
			</div>

			<div class="study-card-content">
				{#each backTexts as text}
					<p>{text}</p>
				{/each}
			</div>
		</section>
	</div>
</button>

<style>
	.study-card-shell {
		width: 100%;
		min-height: 24rem;
		padding: 0;
		border: none;
		background: transparent;
		cursor: pointer;
		perspective: 90rem;
	}

	.study-card-inner {
		position: relative;
		width: 100%;
		min-height: 24rem;
		transform-style: preserve-3d;
		transition: transform 0.5s ease;
	}

	.study-card-inner.flipped {
		transform: rotateY(180deg);
	}

	.study-card-face {
		position: absolute;
		inset: 0;
		display: flex;
		flex-direction: column;
		gap: 1.25rem;
		padding: 1.5rem;
		border: 0.0625rem solid var(--color-sage);
		border-radius: 2rem;
		background: var(--color-paper);
		box-shadow: var(--shadow);
		backface-visibility: hidden;
	}

	.study-card-face-back {
		transform: rotateY(180deg);
	}

	.study-card-face-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 1rem;
	}

	.face-pill {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-width: 4.5rem;
		padding: 0.5rem 0.875rem;
		border-radius: 999rem;
		background: var(--color-sage-soft);
		color: var(--text);
		font-size: 0.875rem;
		font-weight: 700;
	}

	.face-hint {
		font-size: 0.875rem;
		color: var(--color-text-light);
	}

	.study-card-content {
		flex: 1;
		display: flex;
		flex-direction: column;
		justify-content: center;
		gap: 1rem;
		text-align: center;
	}

	.study-card-content p {
		font-size: 1.5rem;
		line-height: 1.5;
		word-break: break-word;
	}
</style>
