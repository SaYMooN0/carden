<script lang="ts">
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<div class="main-layout-container">
	<div class="content">
		{@render children()}
	</div>
</div>

<style>
	.main-layout-container {
		width: 100%;
		height: 100%;
		display: grid;
	}

	.content {
		width: 100%;
	}
</style>
