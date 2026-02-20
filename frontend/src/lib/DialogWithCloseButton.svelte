<script lang="ts">
	import type { Snippet } from 'svelte';
	import BaseDialog from './BaseDialog.svelte';
	import { StringUtils } from '$lib/ts/utils/string-utils';

	interface Props {
		children: Snippet;
		subheading?: string;
		dialogId?: string;
		onBeforeClose?: () => void;
		closedby?: 'none' | 'closerequest' | 'any';
	}

	let { children, subheading, dialogId, onBeforeClose, closedby }: Props = $props();

	let dialog = $state<BaseDialog>()!;
	export function open() {
		dialog.open();
	}
	export function close() {
		if (onBeforeClose) {
			onBeforeClose();
		}
		dialog.close();
	}
</script>

<BaseDialog bind:this={dialog} {dialogId} {closedby}>
	{#if !StringUtils.isNullOrWhiteSpace(subheading)}
		<h1 class="subheading">{subheading}</h1>
	{/if}
	<button class="dialog-close-btn" onclick={() => close()}>
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none">
			<path
				d="M18 6L6.00081 17.9992M17.9992 18L6 6.00085"
				stroke="currentColor"
				stroke-width="2.2"
				stroke-linecap="round"
				stroke-linejoin="round"
			/>
		</svg>
	</button>
	{@render children()}
</BaseDialog>

<style>
	:global(dialog > .dialog-content) {
		display: flex;
		flex-direction: column;
		align-items: center;
		padding: 2rem;
	}

	.subheading {
		padding: 0 0 3rem;
		color: var(--text);
		font-size: 1.75rem;
		font-weight: 550;
		text-align: center;
		letter-spacing: 0.125px;
	}

	.dialog-close-btn {
		position: absolute;
		top: 0.75rem;
		right: 0.75rem;
		width: 1.5rem;
		height: 1.5rem;
		padding: 0.125rem;
		border: none;
		border-radius: 0.25rem;
		background-color: var(--secondary);
		color: var(--secondary-foreground);
		box-shadow: var(--shadow-md);
		cursor: pointer;
	}

	.dialog-close-btn > :global(svg path) {
		stroke-width: 2.2;
		transition: stroke-width 0.06s ease-in;
	}

	.dialog-close-btn:hover {
		background-color: var(--muted);
		color: var(--muted-foreground);
	}

	.dialog-close-btn:hover > :global(svg path) {
		stroke-width: 2.5;
	}

	.dialog-close-btn:active > :global(svg path) {
		stroke-width: 2.2;
	}
</style>
