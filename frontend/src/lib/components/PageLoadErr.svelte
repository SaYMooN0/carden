<script lang="ts">
	import type { Err } from '../ts/err';
	import DefaultErrBlock from './errs/DefaultErrBlock.svelte';

	let { errs }: { errs: Err[] } = $props();
</script>

<DefaultErrBlock {errs} />
