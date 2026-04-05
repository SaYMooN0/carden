<script lang="ts">
	interface Props {
		plantId: string;
		cardsCount: number;
	}

	let { plantId, cardsCount }: Props = $props();

	const cardsLabel = $derived.by(() => {
		const rule = new Intl.PluralRules('ru-RU').select(cardsCount);
		if (rule === 'one') return 'карточка';
		if (rule === 'few') return 'карточки';
		return 'карточек';
	});
</script>

<div class="state-card">
	<div class="state-copy">
		<div class="state-eyebrow">обучение недоступно</div>
		<h2 class="state-title">Недостаточно карточек для начала обучения</h2>
		<p class="state-description">
			В этой колоде пока недостаточно карточек. Сейчас в ней
			<span class="state-count">{cardsCount} {cardsLabel}</span>.
		</p>
	</div>

	<div class="state-actions">
		<a class="edit-link" href={`/${plantId}/edit`}>Редактировать колоду</a>
	</div>
</div>

<style>
	.state-card {
		--state-bg: var(--color-sage-hover);
		--state-accent-bg: var(--red-1);

		display: flex;
		flex-direction: column;
		gap: 1rem;
		padding: 1.5rem;
		border-radius: 1rem;
		background: var(--state-bg);
		box-shadow: var(--shadow);
		margin: 7rem;
	}

	.state-copy {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}

	.state-eyebrow {
		font-size: 0.75rem;
		line-height: 1rem;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--red-3);
	}

	.state-title {
		font-size: 1.5rem;
		line-height: 1.875rem;
		font-weight: 700;
		color: var(--text);
	}

	.state-description {
		font-size: 1rem;
		line-height: 1.5rem;
		color: var(--color-text-light);
	}

	.state-count {
		display: inline-block;
		padding: 0.125rem 0.5rem;
		border-radius: 999rem;
		background: var(--state-accent-bg);
		color: var(--red-3);
		font-weight: 700;
	}

	.state-actions {
		display: flex;
		align-items: center;
		gap: 0.75rem;
	}

	.edit-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 2.75rem;
		padding: 0.75rem 1rem;
		border-radius: 0.75rem;
		background: var(--primary);
		color: var(--primary-foreground);
		font-size: 1rem;
		font-weight: 600;
		transition:
			background-color 0.2s ease,
			transform 0.2s ease;
	}

	.edit-link:hover {
		background: var(--primary-hov);
		transform: translateY(-0.0625rem);
	}
</style>
