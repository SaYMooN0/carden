<script lang="ts">
	const { onClick, text } = $props<{ onClick: () => void; text: string }>();
</script>

<p class="unselectable" onclick={() => onClick()}>{text}</p>

<style>
	p {
		padding: 0.125rem 1rem;
		margin: 0.5rem auto 0;
		border-radius: 0.375rem;
		background-color: transparent;
		font-size: 1rem;
		text-decoration: underline;
		transition: all 0.12s ease;
		cursor: pointer;
	}
</style>
