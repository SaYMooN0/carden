<script lang="ts">
	import { toast } from 'svelte-sonner';
	import type { StudyDeckLoadResponse } from '../shared_types';
	import { StudyDeckPageState } from './study-deck-page-state.svelte';
	import StudyAnswerButtons from './_c_study_deck/StudyAnswerButtons.svelte';
	import StudyDeckCard from './_c_study_deck/StudyDeckCard.svelte';
	import StudySessionStats from './_c_study_deck/StudySessionStats.svelte';

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

<h1>dsads1111111111111111111111111</h1>
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
					<p class="finished-eyebrow">Done for today</p>
					<h2>Study session finished</h2>
					<p>
						You reviewed <strong>{pageState.deckStudyState.totalAnswersCount}</strong> answers
						across
						<strong>{pageState.deckStudyState.uniqueCardsSeenCount}</strong> unique cards.
					</p>
				</section>
			{/if}
		</section>

		<aside class="study-side-panel">
			<section class="side-card">
				<h2>How it works</h2>
				<p>Flip the card, then rate how well you remembered it. Cards you miss come back sooner.</p>
			</section>

			<section class="side-card">
				<h2>Current progress</h2>
				<ul class="info-list">
					<li>
						<span>New cards left</span>
						<strong>{pageState.newCardsLeft}</strong>
					</li>
					<li>
						<span>Review cards left</span>
						<strong>{pageState.reviewCardsLeft}</strong>
					</li>
					<li>
						<span>Total answers</span>
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

	.finished-eyebrow {
		font-size: 0.875rem;
		font-weight: 700;
		letter-spacing: 0.04em;
		text-transform: uppercase;
		color: var(--color-text-light);
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
