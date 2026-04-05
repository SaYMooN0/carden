<script lang="ts">
	import { toast } from 'svelte-sonner';
	import type { StudyDeckLoadResponse } from '../shared_types';
	import { StudyDeckPageState } from './study-deck-page-state.svelte';
	import StudyAnswerButtons from './_c_study_deck/StudyAnswerButtons.svelte';
	import StudyDeckCard from './_c_study_deck/StudyDeckCard.svelte';
	import StudySessionStats from './_c_study_deck/StudySessionStats.svelte';
	import HomeButton from '$lib/components/HomeButton.svelte';

	interface Props {
		studyDeckLoadResponse: StudyDeckLoadResponse;
	}

	let { studyDeckLoadResponse }: Props = $props();
	let pageState = new StudyDeckPageState(studyDeckLoadResponse, (msg) => toast.error(msg));

	function handleCardFlip() {
		console.log(pageState.deckStudyState);
		if (
			pageState.deckStudyState.state === 'Card' &&
			pageState.deckStudyState.currentSide === 'Front'
		) {
			pageState.flipCurrentCardToBack();
		}
	}
</script>

<div class="study-page-shell">
	<StudySessionStats
		plantName={pageState.plantName}
		newCardsLeft={pageState.newCardsLeft}
		reviewCardsLeft={pageState.reviewCardsLeft}
		cardsStillInSessionCount={pageState.cardsStillInSessionCount}
		totalAnswersCount={pageState.totalAnswersCount}
		uniqueCardsSeenCount={pageState.uniqueCardsSeenCount}
	/>

	<div class="study-layout-grid">
		<section class="study-main-panel">
			{#if pageState.deckStudyState.state === 'Card'}
				<StudyDeckCard
					frontTexts={pageState.deckStudyState.currentCard.contentFront}
					backTexts={pageState.deckStudyState.currentCard.contentBack}
					currentSide={pageState.deckStudyState.currentSide}
					onFlip={handleCardFlip}
				/>

				<StudyAnswerButtons
					isEnabled={pageState.deckStudyState.currentSide === 'Back'}
					onRate={(difficulty) => pageState.rateCurrentCardDifficulty(difficulty)}
				/>
			{:else if pageState.deckStudyState.state === 'Finished'}
				<section class="finished-panel">
					<h2>Сеанс обучения завершён</h2>
					<p>
						Вы повторили <strong>{pageState.deckStudyState.totalAnswersCount}</strong> ответов по
						<strong>{pageState.deckStudyState.uniqueCardsSeenCount}</strong> уникальным карточкам.
					</p>
					<HomeButton isCompact={false} />
				</section>
			{/if}
		</section>

		<aside class="study-side-panel">
			<section class="side-card">
				<h2>Как это работает</h2>
				<p>
					Переверните карточку, затем оцените, насколько хорошо вы её помните. Карточки, в которых
					вы ошибаетесь, возвращаются быстрее.
				</p>
			</section>

			<section class="side-card">
				<h2>Текущий прогресс</h2>
				<ul class="info-list">
					<li>
						<span>Осталось новых карточек</span>
						<strong>{pageState.newCardsLeft}</strong>
					</li>
					<li>
						<span>Осталось на повторение</span>
						<strong>{pageState.reviewCardsLeft}</strong>
					</li>
					<li>
						<span>Всего ответов</span>
						<strong>{pageState.totalAnswersCount}</strong>
					</li>
				</ul>
			</section>
		</aside>
	</div>
</div>

<style>
	.study-page-shell {
		display: flex;
		flex-direction: column;
		gap: 1.25rem;
		width: 100%;
		max-width: 90rem;
		margin: 0 auto;
		padding: 1.5rem;
	}

	.study-layout-grid {
		display: grid;
		grid-template-columns: minmax(0, 1fr) 18rem;
		gap: 1.25rem;
		align-items: start;
	}

	.study-main-panel {
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	.study-side-panel {
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	.side-card,
	.finished-panel {
		display: flex;
		flex-direction: column;
		gap: 0.75rem;
		padding: 1.25rem;
		border: 0.0625rem solid var(--color-sage);
		border-radius: 1.75rem;
		background: var(--color-paper);
		box-shadow: var(--shadow);
	}

	.side-card h2,
	.finished-panel h2 {
		font-size: 1.25rem;
		line-height: 1.25;
	}

	.info-list {
		display: flex;
		flex-direction: column;
		gap: 0.75rem;
		list-style: none;
	}

	.info-list li {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 1rem;
		padding-bottom: 0.75rem;
		border-bottom: 0.0625rem solid var(--color-sage-soft);
	}

	.info-list li:last-child {
		padding-bottom: 0;
		border-bottom: none;
	}

	.info-list span {
		color: var(--color-text-light);
	}
</style>
