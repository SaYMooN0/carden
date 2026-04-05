<script lang="ts">
	import PageLoadErr from '$lib/components/PageLoadErr.svelte';
	import { ErrUtils } from '$lib/ts/err';
	import type { PageProps } from './$types';
	import NotEnoughCardsToStudyState from './_c_page/NotEnoughCardsToStudyState.svelte';
	import StudyPlantDeck from './_c_page/StudyPlantDeck.svelte';

	let { data }: PageProps = $props();
	console.log(data);
</script>

{#if !data.isSuccess && ErrUtils.ensureExtraData(data.errs[0], 'NOT_ENOUGH_CARDS_TO_STUDY')}
	<NotEnoughCardsToStudyState
		plantId={data.errs[0].extraData.data.plantId}
		cardsCount={data.errs[0].extraData.data.cardsCount}
	/>
{:else if data.errs?.some((err) => err.extraData?.id === 'INVALID_PLANT_ID')}
	<PageLoadErr
		errs={[
			{
				msg: 'Неверный идентификатор растения',
				fixSuggestion: 'Пожалуйста, убедитесь, что вы использовали правильную ссылку'
			}
		]}
	/>
{:else if !data.isSuccess}
	<PageLoadErr errs={data.errs} />
	{#if data.statusCode === 401}
		<button onclick={() => (window.location.href = '/')}>На главную страницу</button>
	{/if}
{:else}
	<StudyPlantDeck studyDeckLoadResponse={data.data} />
{/if}
