<script lang="ts">
	import DialogWithCloseButton from '$lib/components/dialogs/DialogWithCloseButton.svelte';

	interface Props {
		deleteCard: (
			cardId: string
		) => Promise<{ isSuccess: true } | { isSuccess: false; errMsg: string }>;
	}

	let { deleteCard }: Props = $props();

	let dialog: DialogWithCloseButton = $state()!;
	let cardToDeleteId = $state<string | null>(null);
	let isDeleting = $state(false);
	let errorMsg = $state<string | null>(null);

	export function open(cardId: string) {
		cardToDeleteId = cardId;
		errorMsg = null;
		dialog.open();
	}

	export function close() {
		if (isDeleting) {
			return;
		}

		cardToDeleteId = null;
		errorMsg = null;
		dialog.close();
	}

	async function handleDelete() {
		if (!cardToDeleteId || isDeleting) {
			return;
		}

		isDeleting = true;
		errorMsg = null;

		try {
			const result = await deleteCard(cardToDeleteId!);

			if (result.isSuccess) {
				close();
			} else {
				errorMsg = result.errMsg;
			}
		} finally {
			isDeleting = false;
			close();
		}
	}
</script>

<DialogWithCloseButton bind:this={dialog} dialogId="confirm-card-delete-dialog">
	<div class="dialog-copy">
		<div class="dialog-eyebrow">удалить карточку</div>
		<h3 class="dialog-title">Удалить эту карточку?</h3>
		<p class="dialog-description">
			Это действие нельзя отменить. Карточка будет навсегда удалена из колоды.
		</p>
	</div>

	{#if errorMsg}
		<div class="error-box">{errorMsg}</div>
	{/if}

	<div class="dialog-actions">
		<button class="secondary-button" type="button" onclick={close} disabled={isDeleting}>
			отмена
		</button>

		{#if cardToDeleteId}
			<button class="danger-button" type="button" onclick={handleDelete} disabled={isDeleting}>
				{isDeleting ? 'удаление...' : 'удалить карточку'}
			</button>
		{/if}
	</div>
</DialogWithCloseButton>

<style>
	:global(#confirm-card-delete-dialog > .dialog-content) {
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
		letter-spacing: 0.125rem;
		text-transform: uppercase;
		color: var(--color-text-light);
	}

	.dialog-title {
		font-size: 1.5rem;
		line-height: 1.25;
		color: var(--text);
	}

	.dialog-description {
		font-size: 1rem;
		line-height: 1.5;
		color: var(--color-text-light);
	}

	.error-box {
		padding: 0.875rem 1rem;
		border: 0.125rem solid var(--red-3);
		border-radius: 1rem;
		background: var(--red-1);
		color: var(--red-5);
		font-size: 0.9375rem;
		line-height: 1.5;
	}

	.dialog-actions {
		display: grid;
		gap: 0.75rem;
	}

	.secondary-button,
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
			transform 0.125s ease,
			background 0.125s ease,
			border-color 0.125s ease,
			opacity 0.125s ease;
	}

	.secondary-button:disabled,
	.danger-button:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.secondary-button {
		background: var(--color-sage-hover);
		color: var(--text);
		border: 0.125rem solid var(--color-sage);
	}

	.secondary-button:hover:enabled {
		background: var(--color-sage);
	}

	.danger-button {
		background: var(--red-1);
		color: var(--red-3);
		border: 0.125rem solid var(--red-3);
	}

	.danger-button:hover:enabled {
		background: var(--red-2);
		color: var(--red-4);
		border-color: var(--red-4);
	}
</style>
