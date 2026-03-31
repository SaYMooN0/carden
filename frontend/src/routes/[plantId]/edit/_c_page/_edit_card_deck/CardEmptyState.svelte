<script lang="ts">
	interface Props {
		icon: string;
		header: string;
		text: string;
		button: { text: string; onClick: () => void };
	}
	let { icon, header, text, button }: Props = $props();
</script>

<div class="empty-state">
	<div class="icon">{icon}</div>
	<h2>{header}</h2>
	<p>{text}</p>
	<button class="primary-button" type="button" onclick={() => button.onClick()}>
		{button.text}
	</button>
</div>

<style>
	.empty-state {
		display: grid;
		justify-items: center;
		align-content: center;
		gap: 0.75rem;
		min-height: 100%;
		padding: 2rem;
		text-align: center;
		background: var(--editor-surface-soft);
		border: 0.0625rem dashed var(--editor-border);
		border-radius: 1.5rem;
	}

	.icon {
		display: grid;
		place-items: center;
		inline-size: 4rem;
		block-size: 4rem;
		border-radius: 999rem;
		background: var(--color-sage-hover);
		font-size: 1.625rem;
		font-weight: 700;
		color: var(--color-terracotta);
	}

	.empty-state h2 {
		font-size: 1.5rem;
		line-height: 1.2;
	}

	.empty-state p {
		max-inline-size: 28rem;
		font-size: 1rem;
		line-height: 1.5;
		color: var(--editor-text-soft);
	}
</style>
