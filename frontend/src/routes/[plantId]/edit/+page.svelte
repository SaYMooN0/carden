<script lang="ts">
	import PageLoadErr from '$lib/components/PageLoadErr.svelte';
	import EditPlantDeck from './_c_page/EditPlantDeck.svelte';
	import type { PageProps } from './$types';

	let { data }: PageProps = $props();
</script>

{#if data.errs?.some((err) => err.extraData?.id === 'INVALID_PLANT_ID')}
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
	<EditPlantDeck plant={data.data} />
{/if}
