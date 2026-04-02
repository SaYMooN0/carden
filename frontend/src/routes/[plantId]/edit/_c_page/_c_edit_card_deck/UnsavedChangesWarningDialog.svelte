<script lang="ts">
	import DialogWithCloseButton from '$lib/components/dialogs/DialogWithCloseButton.svelte';

	interface Props {
		onStay: () => void;
		onDiscardAndContinue: (newCardId: string) => void;
	}

	let { onStay, onDiscardAndContinue }: Props = $props();

	let dialog: DialogWithCloseButton = $state()!;
	let newCardId = $state<string | null>(null);
	export function open(cardId: string) {
		newCardId = cardId;
		dialog.open();
	}
	export function close() {
		newCardId = null;
		dialog.close();
	}
</script>

<DialogWithCloseButton bind:this={dialog} dialogId="unsaved-changes-warning-dialog">
	<div class="dialog-copy">
		<div class="dialog-eyebrow">unsaved changes</div>
		<h3 class="dialog-title">You have unsaved changes in this card</h3>
		<p class="dialog-description">
			Save the card before leaving, or discard the current changes and continue.
		</p>
	</div>

	<div class="dialog-actions">
		<button class="primary-button" type="button" onclick={onStay}>stay here</button>
		{#if newCardId}
			<button class="danger-button" type="button" onclick={() => onDiscardAndContinue(newCardId!)}>
				discard and continue
			</button>
		{/if}
	</div>
</DialogWithCloseButton>

<style>
	:global(#unsaved-changes-warning-dialog > .dialog-content) {
		display: grid;
		gap: 1.5rem;
		min-inline-size: min(28rem, calc(100vw - 2rem));
	}

	.dialog-copy {
		display: grid;
		gap: 0.5rem;
	}

	.dialog-eyebrow {
		font-size: 0.75rem;
		font-weight: 700;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--color-text-light);
	}

	.dialog-title {
		font-size: 1.5rem;
		line-height: 1.2;
		color: var(--text);
	}

	.dialog-description {
		font-size: 1rem;
		line-height: 1.5;
		color: var(--color-text-light);
	}

	.dialog-actions {
		display: grid;
		gap: 0.75rem;
	}

	.primary-button,
	.danger-button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-block-size: 3rem;
		padding-inline: 1rem;
		border-radius: 1rem;
		font-size: 1rem;
		font-weight: 700;
		transition:
			transform 0.18s ease,
			background 0.18s ease,
			border-color 0.18s ease,
			opacity 0.18s ease;
	}

	.primary-button {
		background: var(--primary);
		color: var(--primary-foreground);
		border: 0.125rem solid var(--primary);
	}

	.primary-button:hover {
		background: var(--primary-hov);
		border-color: var(--primary-hov);
	}

	.danger-button {
		background: var(--red-1);
		color: var(--red-3);
		border: 0.125rem solid var(--red-3);
	}

	.danger-button:hover {
		background: var(--red-2);
		color: var(--red-4);
		border-color: var(--red-4);
	}
</style>
