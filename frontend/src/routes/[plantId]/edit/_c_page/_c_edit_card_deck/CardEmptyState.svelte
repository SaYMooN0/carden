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
		background: var(--color-cream);
		border: 0.125rem dashed var(--color-sage);
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
		color: var(--color-text-light);
	}

	.primary-button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 3rem;
		padding-inline: 1.25rem;
		border-radius: 1rem;
		font-size: 1rem;
		font-weight: 700;
		transition:
			transform 0.18s ease,
			background 0.18s ease,
			border-color 0.18s ease,
			opacity 0.18s ease;
		background: var(--primary);
		color: var(--primary-foreground);
		border: 0.125rem solid var(--primary);
	}

	.primary-button:hover {
		background: var(--primary-hov);
		border-color: var(--primary-hov);
		transform: translateY(-0.125rem);
	}

	.primary-button:disabled {
		opacity: 0.5;
		transform: none;
	}
</style>
