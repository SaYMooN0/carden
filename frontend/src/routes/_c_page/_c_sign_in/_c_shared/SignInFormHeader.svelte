<script lang="ts">
	const { text } = $props<{ text: string }>();
</script>

<h1>{text}</h1>

<style>
	h1 {
		width: 100%;
		margin-bottom: 1rem;
		font-size: 1.75rem;
		font-weight: 500;
		text-align: center;
	}
</style>
