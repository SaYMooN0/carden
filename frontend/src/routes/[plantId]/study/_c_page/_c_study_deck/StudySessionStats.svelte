<script lang="ts">
	interface Props {
		plantName: string;
		newCardsLeft: number;
		reviewCardsLeft: number;
		cardsStillInSessionCount: number;
		totalAnswersCount: number;
		uniqueCardsSeenCount: number;
	}

	let {
		plantName,
		newCardsLeft,
		reviewCardsLeft,
		cardsStillInSessionCount,
		totalAnswersCount,
		uniqueCardsSeenCount
	}: Props = $props();
</script>

<section class="stats-shell">
	<div class="stats-title-block">
		<p class="stats-eyebrow">Study session</p>
		<h1>{plantName}</h1>
	</div>

	<div class="stats-grid">
		<article class="stat-card">
			<span class="stat-label">Still in session</span>
			<strong>{cardsStillInSessionCount}</strong>
		</article>

		<article class="stat-card">
			<span class="stat-label">New left</span>
			<strong>{newCardsLeft}</strong>
		</article>

		<article class="stat-card">
			<span class="stat-label">Review left</span>
			<strong>{reviewCardsLeft}</strong>
		</article>

		<article class="stat-card">
			<span class="stat-label">Answers</span>
			<strong>{totalAnswersCount}</strong>
		</article>

		<article class="stat-card stat-card-wide">
			<span class="stat-label">Unique cards seen</span>
			<strong>{uniqueCardsSeenCount}</strong>
		</article>
	</div>
</section>

<style>
	.stats-shell {
		display: flex;
		flex-direction: column;
		gap: 1rem;
		padding: 1.25rem;
		border: 0.0625rem solid var(--color-sage);
		border-radius: 1.75rem;
		background: var(--color-paper);
		box-shadow: var(--shadow);
	}

	.stats-title-block {
		display: flex;
		flex-direction: column;
		gap: 0.375rem;
	}

	.stats-eyebrow {
		font-size: 0.875rem;
		font-weight: 700;
		letter-spacing: 0.04em;
		text-transform: uppercase;
		color: var(--color-text-light);
	}

	.stats-title-block h1 {
		font-size: 1.625rem;
		line-height: 1.25;
	}

	.stats-grid {
		display: grid;
		grid-template-columns: repeat(4, minmax(0, 1fr));
		gap: 0.75rem;
	}

	.stat-card {
		display: flex;
		flex-direction: column;
		gap: 0.375rem;
		padding: 1rem;
		border-radius: 1.25rem;
		background: var(--color-cream-strong);
	}

	.stat-card-wide {
		grid-column: span 4;
	}

	.stat-label {
		font-size: 0.875rem;
		color: var(--color-text-light);
	}

	.stat-card strong {
		font-size: 1.5rem;
		line-height: 1.125;
	}
</style>
