<script lang="ts">
	interface Props {
		sizeRem?: number;
		strokePx?: number;
		speedSec?: number;
		color: string;
		class?: string;
	}
	let { sizeRem = 3, strokePx = 3, speedSec = 1.5, color, class: className }: Props = $props();
</script>

<div
	class="container {className}"
	style="
		--size: {sizeRem}rem;
		--speed: {speedSec}s;
		--line-stroke: {strokePx}px;
		--loader-color: {color};
		"
>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
	<div class="line"></div>
</div>

<style>
	.container {
		position: relative;
		display: flex;
		justify-content: flex-start;
		align-items: center;
		width: var(--size);
		height: var(--size);
	}

	.line {
		position: absolute;
		top: 0;
		left: calc(50% - var(--line-stroke) / 2);
		display: flex;
		align-items: flex-start;
		width: var(--line-stroke);
		height: 100%;
	}

	.line::before {
		width: 100%;
		height: 22%;
		border-radius: calc(var(--line-stroke) / 2);
		background-color: var(--loader-color);
		transition: background-color 0.3s ease;
		animation: pulse calc(var(--speed)) ease-in-out infinite;
		content: '';
		transform-origin: center bottom;
	}

	.line:nth-child(1) {
		transform: rotate(calc(360deg / -12 * 1));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 1);
		}
	}

	.line:nth-child(2) {
		transform: rotate(calc(360deg / -12 * 2));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 2);
		}
	}

	.line:nth-child(3) {
		transform: rotate(calc(360deg / -12 * 3));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 3);
		}
	}

	.line:nth-child(4) {
		transform: rotate(calc(360deg / -12 * 4));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 4);
		}
	}

	.line:nth-child(5) {
		transform: rotate(calc(360deg / -12 * 5));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 5);
		}
	}

	.line:nth-child(6) {
		transform: rotate(calc(360deg / -12 * 6));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 6);
		}
	}

	.line:nth-child(7) {
		transform: rotate(calc(360deg / -12 * 7));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 7);
		}
	}

	.line:nth-child(8) {
		transform: rotate(calc(360deg / -12 * 8));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 8);
		}
	}

	.line:nth-child(9) {
		transform: rotate(calc(360deg / -12 * 9));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 9);
		}
	}

	.line:nth-child(10) {
		transform: rotate(calc(360deg / -12 * 10));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 10);
		}
	}

	.line:nth-child(11) {
		transform: rotate(calc(360deg / -12 * 11));

		&::before {
			animation-delay: calc(var(--speed) / -12 * 11);
		}
	}

	@keyframes pulse {
		0%,
		80%,
		100% {
			opacity: 0;
			transform: scaleY(0.75);
		}

		20% {
			opacity: 1;
			transform: scaleY(1);
		}
	}
</style>
