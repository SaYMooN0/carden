<script lang="ts" generics="T extends string">
	interface Props<T extends string> {
		items: readonly T[];
		title: string;
		selectedIndex?: number;
		getKey: (item: T) => string;
		getLabel: (item: T) => string;
		getImageSrc: (item: T) => string;
		itemAspectRatio: number;
		itemWidth?: string;
		previousAriaLabel?: string;
		nextAriaLabel?: string;
	}

	let {
		items,
		title,
		selectedIndex = $bindable(0),
		getKey,
		getLabel,
		getImageSrc,
		itemAspectRatio,
		itemWidth = '10rem',
		previousAriaLabel = 'Предыдущий элемент',
		nextAriaLabel = 'Следующий элемент'
	}: Props<T> = $props();

	function mod(n: number, m: number): number {
		return ((n % m) + m) % m;
	}

	function getRelativeOffset(index: number): number {
		let diff = index - selectedIndex;

		if (diff > items.length / 2) {
			diff -= items.length;
		}
		if (diff < -items.length / 2) {
			diff += items.length;
		}

		return diff;
	}

	function selectPrev() {
		if (items.length <= 1) {
			return;
		}

		selectedIndex = mod(selectedIndex - 1, items.length);
	}

	function selectNext() {
		if (items.length <= 1) {
			return;
		}

		selectedIndex = mod(selectedIndex + 1, items.length);
	}
</script>

<section
	class="section"
	aria-label={title}
	style:--item-width={itemWidth}
	style:--item-aspect-ratio={itemAspectRatio}
>
	<div class="section-title">
		<span>{title}</span>
	</div>

	<div class="carousel">
		<button
			class="nav-button left"
			type="button"
			aria-label={previousAriaLabel}
			onclick={selectPrev}
			disabled={items.length <= 1}
		>
			<svg
				xmlns="http://www.w3.org/2000/svg"
				viewBox="0 0 24 24"
				fill="none"
				stroke="currentColor"
				stroke-width="2"
				stroke-linecap="round"
				stroke-linejoin="round"
			>
				<path d="M15 6C15 6 9.00001 10.4189 9 12C8.99999 13.5812 15 18 15 18" />
			</svg>
		</button>

		<div class="carousel-window">
			{#each items as item, index (getKey(item))}
				{@const offset = getRelativeOffset(index)}

				<button
					type="button"
					class={`carousel-item ${offset === 0 ? 'center' : ''} ${offset === -1 ? 'left' : ''} ${offset === 1 ? 'right' : ''} ${Math.abs(offset) > 1 ? 'hidden' : ''}`}
					aria-pressed={offset === 0}
					aria-label={getLabel(item)}
					onclick={() => (selectedIndex = index)}
				>
					<div class="item-card">
						<div class="item-visual">
							<img
								class="item-sprite"
								src={getImageSrc(item)}
								alt={getLabel(item)}
								draggable="false"
							/>
						</div>

						<span class="item-label">{getLabel(item)}</span>
					</div>
				</button>
			{/each}
		</div>

		<button
			class="nav-button right"
			type="button"
			aria-label={nextAriaLabel}
			onclick={selectNext}
			disabled={items.length <= 1}
		>
			<svg
				xmlns="http://www.w3.org/2000/svg"
				viewBox="0 0 24 24"
				fill="none"
				stroke="currentColor"
				stroke-width="2"
				stroke-linecap="round"
				stroke-linejoin="round"
			>
				<path d="M15 6C15 6 9.00001 10.4189 9 12C8.99999 13.5812 15 18 15 18" />
			</svg>
		</button>
	</div>
</section>

<style>
	.section {
		display: flex;
		flex-direction: column;
		gap: 0.625rem;
		min-width: 40rem;
	}

	.section-title {
		font-size: 1rem;
		font-weight: 600;
	}

	.carousel {
		position: relative;
	}

	.carousel-window {
		position: relative;
		min-height: calc(var(--item-width) / var(--item-aspect-ratio));
		overflow: hidden;
		border-radius: 1.5rem;
	}

	.carousel-item {
		position: absolute;
		top: 0;
		left: 50%;
		width: var(--item-width);
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
		transform: translateX(-50%) scale(0.72);
	}

	.carousel-item.center {
		transform: translateX(-50%) scale(1);
		z-index: 3;
		opacity: 1;
	}

	.carousel-item.left {
		transform: translateX(calc(-50% - (var(--item-width) * 0.75))) scale(0.84);
		z-index: 2;
		opacity: 0.82;
	}

	.carousel-item.right {
		transform: translateX(calc(-50% + (var(--item-width) * 0.75))) scale(0.84);
		z-index: 2;
		opacity: 0.82;
	}

	.item-card {
		position: relative;
		width: 100%;
		aspect-ratio: var(--item-aspect-ratio);
		border-radius: 1.375rem;
		background: var(--color-cream);
		border: 0.125rem solid var(--color-sage);
		box-shadow: var(--shadow);
		overflow: hidden;
		transition:
			border-color 0.2s ease,
			background-color 0.2s ease,
			transform 0.2s ease;
	}

	.item-visual {
		position: absolute;
		inset: 0;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 0.75rem 0.75rem 2.75rem;
	}

	.item-sprite {
		max-width: 100%;
		max-height: 100%;
		user-select: none;
		pointer-events: none;
	}

	.item-label {
		position: absolute;
		left: 0.625rem;
		right: 0.625rem;
		bottom: 0.625rem;
		min-height: 1.875rem;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 0.375rem 0.625rem;
		border-radius: 0.875rem;
		background: var(--color-sage-hover);
		font-size: 0.875rem;
		font-weight: 600;
		line-height: 1.125;
		text-align: center;
	}

	.carousel-item.center .item-card {
		border-color: var(--primary);
	}

	.carousel-item.center .item-label {
		background: var(--color-cream);
	}

	.carousel-item:not(.hidden):hover .item-card {
		background: var(--color-sage-hover);
	}

	.nav-button {
		position: absolute;
		top: 50%;
		z-index: 4;
		width: 2.75rem;
		height: 2.75rem;
		display: flex;
		align-items: center;
		justify-content: center;
		transform: translateY(-50%);
		border: 0.125rem solid var(--color-sage);
		border-radius: 999rem;
		background: var(--color-cream);
		color: var(--text);
		box-shadow: var(--shadow);
		cursor: pointer;
		padding: 0;
		transition:
			transform 0.18s ease,
			background-color 0.18s ease,
			border-color 0.18s ease,
			opacity 0.18s ease;
	}

	.nav-button svg {
		width: 1.375rem;
		height: 1.375rem;
		pointer-events: none;
	}

	.nav-button.left {
		left: 0.5rem;
	}

	.nav-button.right {
		right: 0.5rem;
		transform: translateY(-50%) rotate(180deg);
	}

	.nav-button:hover:enabled {
		background: var(--color-sage-hover);
		border-color: var(--primary);
	}

	.nav-button.left:hover:enabled {
		transform: translateY(-50%) scale(1.08);
	}

	.nav-button.right:hover:enabled {
		transform: translateY(-50%) rotate(180deg) scale(1.08);
	}

	.nav-button:active:enabled {
		transition-duration: 0.08s;
	}

	.nav-button.left:active:enabled {
		transform: translateY(-50%) scale(0.94);
	}

	.nav-button.right:active:enabled {
		transform: translateY(-50%) rotate(180deg) scale(0.94);
	}

	.nav-button:disabled {
		opacity: 0.35;
		cursor: not-allowed;
	}
</style>
