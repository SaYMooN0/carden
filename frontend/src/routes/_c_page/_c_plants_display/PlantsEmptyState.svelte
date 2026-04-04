<script lang="ts">
	import AddPlantButton from './AddPlantButton.svelte';

	interface Props {
		openAddNewPlantDialog: () => void;
	}

	let { openAddNewPlantDialog }: Props = $props();

	const steps = [
		{
			title: 'Создайте растение',
			text: 'Выберите вид растения и горшок. После этого для него появится своя колода карточек.'
		},
		{
			title: 'Добавьте карточки',
			text: 'Чем больше карточек в колоде, тем выше становится уровень горшка и тем заметнее развивается растение.'
		},
		{
			title: 'Повторяйте материал',
			text: 'Возвращайтесь к колоде регулярно. Повторение помогает растению расти и показывает ваш прогресс.'
		}
	];

	const facts = [
		{
			label: 'Растение',
			value: 'Показывает ваш общий прогресс'
		},
		{
			label: 'Горшок',
			value: 'Развивается при добавлении карточек'
		},
		{
			label: 'Колода',
			value: 'Хранит материал по одной теме'
		}
	];
</script>

<section class="plants-empty-state" aria-labelledby="plants-empty-state-title">
	<div class="intro-card">
		<div class="intro-main">
			<p class="section-label">Сад</p>

			<h2 id="plants-empty-state-title">У вас пока нет растений</h2>

			<p class="intro-text">
				Здесь каждое растение связано с колодой карточек. Добавляйте карточки, развивайте горшок и
				возвращайтесь к повторению, чтобы растение постепенно росло.
			</p>

			<div class="intro-actions">
				<AddPlantButton onClick={openAddNewPlantDialog} />
				<p class="intro-note">
					Для начала достаточно одного растения — так проще понять, как всё устроено.
				</p>
			</div>
		</div>

		<aside class="info-card" aria-label="Краткое описание механики">
			<h3>Как это работает</h3>

			<div class="info-list">
				{#each facts as fact}
					<div class="info-row">
						<span class="info-label">{fact.label}</span>
						<strong>{fact.value}</strong>
					</div>
				{/each}
			</div>
		</aside>
	</div>

	<div class="steps-grid">
		{#each steps as step, index}
			<article class="step-card">
				<div class="step-number" aria-hidden="true">{index + 1}</div>
				<h3>{step.title}</h3>
				<p>{step.text}</p>
			</article>
		{/each}
	</div>
</section>

<style>
	.plants-empty-state {
		--panel-bg: var(--color-cream);
		--panel-muted: var(--color-sage-hover);
		--panel-border: var(--color-sage);
		--text-soft: var(--color-text-light);

		display: grid;
		gap: 1.5rem;
		max-inline-size: 80rem;
		margin: 3rem auto;
		padding: 0 3rem;
	}

	.intro-card {
		display: grid;
		grid-template-columns: minmax(0, 1.75fr) minmax(18rem, 1fr);
		gap: 1.25rem;
		padding: 1.5rem;
		border: 0.0625rem solid var(--panel-border);
		border-radius: 1.5rem;
		background: var(--panel-bg);
		box-shadow: var(--shadow);
	}

	.intro-main {
		display: grid;
		align-content: start;
		gap: 1rem;
		min-inline-size: 0;
	}

	.section-label {
		font-size: 0.875rem;
		font-weight: 700;
		line-height: 1.2;
		letter-spacing: 0.04em;
		text-transform: uppercase;
		color: var(--primary);
	}

	h2 {
		font-size: 2rem;
		line-height: 1.1;
		color: var(--text);
	}

	.intro-text {
		max-inline-size: 42rem;
		font-size: 1rem;
		line-height: 1.625;
		color: var(--text-soft);
	}

	.intro-actions {
		display: grid;
		gap: 0.75rem;
		align-content: start;
		padding-top: 0.25rem;
	}

	.intro-note {
		font-size: 0.9375rem;
		line-height: 1.5;
		color: var(--text-soft);
	}

	.info-card {
		display: grid;
		align-content: start;
		gap: 1rem;
		padding: 1.25rem;
		border: 0.0625rem solid var(--panel-border);
		border-radius: 1.25rem;
		background: var(--panel-muted);
	}

	.info-card h3 {
		font-size: 1.125rem;
		line-height: 1.2;
		color: var(--text);
	}

	.info-list {
		display: grid;
		gap: 0.75rem;
	}

	.info-row {
		display: grid;
		gap: 0.25rem;
		padding: 0.875rem 1rem;
		border: 0.0625rem solid var(--panel-border);
		border-radius: 1rem;
		background: var(--panel-bg);
	}

	.info-label {
		font-size: 0.75rem;
		font-weight: 700;
		line-height: 1.2;
		letter-spacing: 0.04em;
		text-transform: uppercase;
		color: var(--text-soft);
	}

	.info-row strong {
		font-size: 0.9375rem;
		line-height: 1.4;
		color: var(--text);
	}

	.steps-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr));
		gap: 1rem;
	}

	.step-card {
		display: grid;
		align-content: start;
		gap: 0.75rem;
		padding: 1.25rem;
		border: 0.0625rem solid var(--panel-border);
		border-radius: 1.25rem;
		background: var(--panel-bg);
		box-shadow: var(--shadow);
	}

	.step-number {
		display: grid;
		place-items: center;
		inline-size: 2rem;
		block-size: 2rem;
		border-radius: 999rem;
		background: var(--primary);
		color: var(--primary-foreground);
		font-size: 0.875rem;
		font-weight: 700;
		line-height: 1;
	}

	.step-card h3 {
		font-size: 1.125rem;
		line-height: 1.25;
		color: var(--text);
	}

	.step-card p {
		font-size: 0.9375rem;
		line-height: 1.6;
		color: var(--text-soft);
	}
</style>
