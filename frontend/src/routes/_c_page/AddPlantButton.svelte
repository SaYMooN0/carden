<script lang="ts">
	interface Props {
		onAdd?: () => void;
		label?: string;
		disabled?: boolean;
	}

	let { onAdd, label = 'Add Plant', disabled = false }: Props = $props();
</script>

<button class="add-plant-button" type="button" onclick={onAdd} {disabled}>
	<span class="icon-wrap" aria-hidden="true"> +</span>

	<span class="label">{label}</span>
</button>

<style>
	.add-plant-button {
		--button-surface: var(--primary);
		--button-surface-hover: var(--primary-hov);

		display: inline-flex;
		align-items: center;
		gap: 0.75rem;
		padding: 0.875rem 1.25rem;
		border: none;
		border-radius: 999rem;
		background: var(--button-surface);
		color: var(--primary-foreground);
		font-size: 1rem;
		font-weight: 700;
		line-height: 1;
		cursor: pointer;
		box-shadow: var(--shadow);
		transition:
			transform 140ms ease,
			background 140ms ease,
			box-shadow 140ms ease;
	}

	.add-plant-button:hover {
		background: var(--button-surface-hover);
		transform: translateY(-0.0625rem);
	}

	.add-plant-button:active {
		transform: translateY(0);
	}

	.add-plant-button:disabled {
		cursor: not-allowed;
		opacity: 0.7;
		transform: none;
	}

	.icon-wrap {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		inline-size: 2rem;
		block-size: 2rem;
		border-radius: 999rem;
		background: color-mix(in srgb, var(--primary-foreground) 18%, transparent);
		flex-shrink: 0;
	}

	.icon-wrap svg {
		inline-size: 1.125rem;
		block-size: 1.125rem;
	}

	.label {
		white-space: nowrap;
	}
</style>
