<script lang="ts">
	import { toast } from 'svelte-sonner';
	import type { StudyDeckLoadResponse } from '../shared_types';
	import { StudyDeckPageState } from './study-deck-page-state.svelte';
	import StudyDeckPage from './_c_study_deck/StudyDeckPage.svelte';

	interface Props {
		studyDeckLoadResponse: StudyDeckLoadResponse;
	}

	let { studyDeckLoadResponse }: Props = $props();
	let pageState = new StudyDeckPageState(studyDeckLoadResponse, (msg) => toast.error(msg));
</script>

<StudyDeckPage {pageState} />
