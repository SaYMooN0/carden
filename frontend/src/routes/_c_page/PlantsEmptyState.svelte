<script lang="ts">
	import AddPlantButton from './AddPlantButton.svelte';

	interface Props {
		onAddPlant?: () => void;
	}

	let { onAddPlant }: Props = $props();

	const lifecycle = [
		{
			title: 'Choose a plant',
			text: 'Pick a plant species and a pot type to start your new little growing project.'
		},
		{
			title: 'Add cards to the deck',
			text: 'Each plant comes with its own deck. As you add cards, the pot levels up and becomes more developed.'
		},
		{
			title: 'Repeat the deck',
			text: 'Studying the deck helps the plant grow. The more you return to it, the healthier and bigger it becomes.'
		}
	];
</script>

<section class="plants-empty-state" aria-labelledby="plants-empty-state-title">
	<div class="hero-card">
		<div class="hero-copy">
			<div class="eyebrow">Your garden starts here</div>

			<h2 id="plants-empty-state-title">You do not have any plants yet</h2>

			<p class="lead">
				In Garden, a plant is not just decoration — it is a learning journey. Choose a plant and a
				pot, give it a deck, add cards to level up the pot, and repeat the deck to help the plant
				grow.
			</p>

			<div class="loop-pills" aria-label="Plant gameplay loop">
				<span>Plant</span>
				<span>Pot</span>
				<span>Deck</span>
				<span>Cards</span>
				<span>Growth</span>
			</div>

			<div class="actions">
				<AddPlantButton onAdd={onAddPlant} />
				<p class="hint">Start with one plant and grow your collection naturally.</p>
			</div>
		</div>

		<div class="visual-card" aria-hidden="true">
			<div class="glow glow-left"></div>
			<div class="glow glow-right"></div>

			<div class="plant-illustration">
				<div class="plant-badge">Deck-powered growth</div>

				<div class="icon-shell">
					<svg
						xmlns="http://www.w3.org/2000/svg"
						fill="none"
						viewBox="0 0 16 16"
						class="plant-icon"
					>
						<g>
							<path
								fill="var(--color-sage)"
								d="m11.5 9.5 -0.913125 4.10875c-0.02475 0.111 -0.0865625 0.21025 -0.1753125 0.281375 -0.08875 0.071125 -0.1990625 0.109875 -0.3128125 0.109875H5.90125c-0.11373125 0 -0.22406875 -0.03875 -0.31280625 -0.109875 -0.08874375 -0.071125 -0.15058125 -0.170375 -0.17531875 -0.281375L4.5 9.5h7Z"
							/>
							<path
								fill="var(--text)"
								d="M12.5 9h-4.79375l0.146875 -0.146875 1.25375 -1.25375c0.506375 0.255 1.0643125 0.391 1.63125 0.3975 0.570625 0.0015 1.130625 -0.154625 1.618125 -0.45125 1.4825 -0.8975 2.276875 -2.97500625 2.125 -5.55750625 -0.007125 -0.12231875 -0.0589375 -0.23775625 -0.1455625 -0.3244 -0.0866875 -0.08664375 -0.202125 -0.13845625 -0.3244375 -0.1456 -2.5825 -0.15125 -4.66 0.6425 -5.558125 2.125 -0.584375 0.965625 -0.599375 2.131875 -0.05375 3.25000625L7.5 7.7925l-0.763125 -0.763125c0.375 -0.82813125 0.348125 -1.68750625 -0.086875 -2.40500625 -0.67936875 -1.124375 -2.23749375 -1.724375 -4.16686875 -1.610625 -0.1221125 0.00728125 -0.23731875 0.05906875 -0.32381875 0.1455625 -0.08649375 0.0865 -0.13828125 0.20170625 -0.14555625 0.3238125 -0.11375 1.929375 0.48625 3.48688125 1.61125 4.16688125 0.37636875 0.2298125 0.8090125 0.3509375 1.25 0.35 0.40006875 -0.004 0.79468125 -0.09325 1.1575 -0.261875L6.793125 8.5l-0.5 0.5H3.50000625c-0.1326125 0 -0.2597875 0.0526875 -0.35355625 0.1464375 -0.09376875 0.09375 -0.14644375 0.2209375 -0.14644375 0.3535625s0.052675 0.2598125 0.14644375 0.3535625c0.09376875 0.09375 0.22094375 0.1464375 0.35355625 0.1464375h0.599375l0.825625 3.716875c0.04818125 0.22275 0.1714875 0.422125 0.34924375 0.5646875 0.17775 0.1425625 0.39913125 0.2196875 0.62700625 0.2184375H10.099375c0.2278125 0.0010625 0.4490625 -0.0761875 0.6268125 -0.2186875 0.1776875 -0.1425625 0.3010625 -0.3418125 0.3494375 -0.5644375L11.90125 10H12.5c0.132625 0 0.2598125 -0.0526875 0.3535625 -0.1464375S13 9.632625 13 9.5s-0.0526875 -0.2598125 -0.1464375 -0.3535625S12.632625 9 12.5 9Zm-3.1875 -4.83875625c0.65375 -1.07875 2.201875 -1.6875 4.1875 -1.660625 0.025625 1.988125 -0.581875 3.53625 -1.660625 4.18750625 -0.719375 0.4375 -1.5875 0.40875 -2.455 -0.07375 -0.483125 -0.86438125 -0.509375 -1.73375625 -0.071875 -2.45313125ZM5.75688125 6.756875c-0.575 0.3075 -1.144375 0.321875 -1.614375 0.0375 -0.71875 -0.435625 -1.133125 -1.46250625 -1.1425 -2.79438125 1.331875 0.009375 2.359375 0.42375 2.794375 1.1425 0.285 0.47 0.268125 1.039375 -0.0375 1.61438125ZM10.099375 13.5H5.90125625l-0.77625 -3.5H10.875l-0.775625 3.5Z"
							/>
						</g>
					</svg>
				</div>

				<div class="mini-stats">
					<div class="mini-stat">
						<span class="mini-stat-label">Pot level</span>
						<strong>Upgrades with new cards</strong>
					</div>
					<div class="mini-stat">
						<span class="mini-stat-label">Plant growth</span>
						<strong>Comes from repetition</strong>
					</div>
				</div>
			</div>
		</div>
	</div>

	<div class="explanation-grid">
		{#each lifecycle as item}
			<article class="step-card">
				<div class="step-index" aria-hidden="true"></div>
				<h3>{item.title}</h3>
				<p>{item.text}</p>
			</article>
		{/each}
	</div>
</section>

<style>
	.plants-empty-state {
		--surface: color-mix(in srgb, var(--color-cream) 84%, var(--color-sage) 16%);
		--surface-strong: color-mix(in srgb, var(--color-sage) 34%, var(--color-cream) 66%);
		--surface-soft: color-mix(in srgb, var(--color-sage) 18%, var(--color-cream) 82%);
		--border-color: color-mix(in srgb, var(--color-sage) 64%, var(--color-cream) 36%);
		--accent-soft: color-mix(in srgb, var(--color-terracotta) 16%, var(--color-cream) 84%);
		--text-soft: var(--color-text-light);

		display: grid;
		margin: 4rem 8rem;
		gap: 1.5rem;
	}

	.hero-card {
		position: relative;
		display: grid;
		grid-template-columns: minmax(0, 1.35fr) minmax(18rem, 1fr);
		gap: 1.25rem;
		padding: 1.5rem;
		border: 0.0625rem solid var(--border-color);
		border-radius: 2rem;
		background:
			radial-gradient(
				circle at top left,
				color-mix(in srgb, var(--color-sage) 32%, transparent),
				transparent 48%
			),
			linear-gradient(145deg, var(--surface), var(--surface-soft));
		box-shadow: var(--shadow);
		overflow: hidden;
	}

	.hero-copy {
		position: relative;
		z-index: 1;
		display: grid;
		align-content: start;
		gap: 1rem;
		padding: 0.25rem;
	}

	.eyebrow {
		inline-size: fit-content;
		padding: 0.5rem 0.75rem;
		border-radius: 999rem;
		background: var(--accent-soft);
		color: var(--primary);
		font-size: 0.875rem;
		font-weight: 700;
		letter-spacing: 0.01em;
	}

	h2 {
		font-size: 2rem;
		line-height: 1.05;
		color: var(--text);
	}

	.lead {
		max-inline-size: 40rem;
		font-size: 1rem;
		line-height: 1.65;
		color: var(--text-soft);
	}

	.loop-pills {
		display: flex;
		flex-wrap: wrap;
		gap: 0.625rem;
	}

	.loop-pills span {
		padding: 0.5rem 0.75rem;
		border: 0.0625rem solid var(--border-color);
		border-radius: 999rem;
		background: color-mix(in srgb, var(--color-cream) 72%, transparent);
		color: var(--text);
		font-size: 0.875rem;
		font-weight: 700;
	}

	.actions {
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		gap: 0.875rem 1rem;
		padding-top: 0.25rem;
	}

	.hint {
		color: var(--text-soft);
		font-size: 0.9375rem;
		line-height: 1.5;
	}

	.visual-card {
		position: relative;
		display: grid;
		align-items: stretch;
		min-block-size: 100%;
	}

	.plant-illustration {
		position: relative;
		display: grid;
		align-content: space-between;
		gap: 1rem;
		padding: 1.25rem;
		border: 0.0625rem solid var(--border-color);
		border-radius: 1.75rem;
		background: linear-gradient(180deg, var(--surface-strong), var(--surface));
		min-block-size: 100%;
	}

	.plant-badge {
		inline-size: fit-content;
		padding: 0.5rem 0.75rem;
		border-radius: 999rem;
		background: color-mix(in srgb, var(--color-terracotta) 18%, var(--color-cream) 82%);
		color: var(--primary);
		font-size: 0.8125rem;
		font-weight: 700;
	}

	.icon-shell {
		display: grid;
		place-items: center;
		inline-size: 100%;
		min-block-size: 10rem;
		padding: 1rem;
		border-radius: 1.5rem;
		background:
			radial-gradient(
				circle at 50% 35%,
				color-mix(in srgb, var(--color-sage) 44%, transparent),
				transparent 56%
			),
			color-mix(in srgb, var(--color-cream) 82%, var(--color-sage) 18%);
		border: 0.0625rem solid color-mix(in srgb, var(--color-sage) 74%, var(--color-cream) 26%);
	}

	.plant-icon {
		inline-size: 6rem;
		block-size: 6rem;
	}

	.mini-stats {
		display: grid;
		gap: 0.75rem;
	}

	.mini-stat {
		display: grid;
		gap: 0.25rem;
		padding: 0.875rem 1rem;
		border-radius: 1.25rem;
		background: color-mix(in srgb, var(--color-cream) 78%, transparent);
		border: 0.0625rem solid var(--border-color);
	}

	.mini-stat-label {
		font-size: 0.8125rem;
		font-weight: 700;
		color: var(--text-soft);
		text-transform: uppercase;
		letter-spacing: 0.04em;
	}

	.mini-stat strong {
		font-size: 0.9375rem;
		line-height: 1.4;
		color: var(--text);
	}

	.glow {
		position: absolute;
		border-radius: 999rem;
		filter: blur(1.25rem);
		opacity: 0.7;
		pointer-events: none;
	}

	.glow-left {
		inline-size: 6rem;
		block-size: 6rem;
		inset-block-start: 0.75rem;
		inset-inline-start: -1.5rem;
		background: color-mix(in srgb, var(--color-sage) 56%, transparent);
	}

	.glow-right {
		inline-size: 7rem;
		block-size: 7rem;
		inset-block-end: -0.75rem;
		inset-inline-end: -1.25rem;
		background: color-mix(in srgb, var(--color-terracotta-light) 34%, transparent);
	}

	.explanation-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr));
		gap: 1rem;
	}

	.step-card {
		display: grid;
		gap: 0.75rem;
		padding: 1.25rem;
		border: 0.0625rem solid var(--border-color);
		border-radius: 1.5rem;
		background: color-mix(in srgb, var(--color-cream) 88%, var(--color-sage) 12%);
		box-shadow: var(--shadow);
	}

	.step-index {
		inline-size: 2.25rem;
		block-size: 0.375rem;
		border-radius: 999rem;
		background: linear-gradient(
			90deg,
			var(--color-terracotta),
			color-mix(in srgb, var(--color-terracotta) 48%, var(--color-sage) 52%)
		);
	}

	.step-card h3 {
		font-size: 1.125rem;
		line-height: 1.2;
		color: var(--text);
	}

	.step-card p {
		color: var(--text-soft);
		font-size: 0.9375rem;
		line-height: 1.6;
	}
</style>
