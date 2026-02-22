<script lang="ts">
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<div class="main-layout-container">
	<div class="header">
		<h1>Some crazy Carden header</h1>
	</div>
	<div class="content">
		{@render children()}
	</div>
</div>

<style>
	.main-layout-container {
		width: 100%;
		height: 100%;
		display: grid;
		grid-template-rows: auto 1fr;
	}

	.header {
		width: 100%;
		background-color: #f5f5f5;
	}

	.content {
		width: 100%;
	}
</style>
