<script lang="ts">
	const { text } = $props<{ text: string }>();
</script>

<h1>{text}</h1>

<style>
	h1 {
		width: 100%;
		margin-bottom: 1.5rem;
		font-size: 1.5rem;
		font-weight: 600;
		text-align: center;
		color: var(--color-text);
	}
</style>
