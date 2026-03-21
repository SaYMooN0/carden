<script>
	import { Toaster } from 'svelte-sonner';
</script>

<Toaster
	richColors
	class="my-toaster"
	toastOptions={{
		classes: {
			toast: 'base-toast',
			error: 'error-toast',
			warning: 'warning-toast',
			info: 'info-toast',
			success: 'success-toast',
			icon: 'toast-icon'
		}
	}}
>
	{#snippet errorIcon()}
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none">
			<path
				d="M13.9248 21H10.0752C5.44476 21 3.12955 21 2.27636 19.4939C1.42317 17.9879 2.60736 15.9914 4.97574 11.9985L6.90057 8.75333C9.17559 4.91778 10.3131 3 12 3C13.6869 3 14.8244 4.91777 17.0994 8.75332L19.0243 11.9985C21.3926 15.9914 22.5768 17.9879 21.7236 19.4939C20.8704 21 18.5552 21 13.9248 21Z"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
				stroke-linejoin="round"
			></path>
			<path
				d="M12 9V13.5"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
				stroke-linejoin="round"
			></path>
			<path
				d="M12 16.9922V17.0022"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
				stroke-linejoin="round"
			></path>
		</svg>
	{/snippet}
	{#snippet warningIcon()}
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none">
			<path
				d="M12.3107 3H11.6893C9.25367 3 8.03584 3 7.03946 3.55252C6.04307 4.10503 5.45164 5.10831 4.26878 7.11486L3.67928 8.11486C2.55976 10.0139 2 10.9635 2 12C2 13.0365 2.55976 13.9861 3.67928 15.8851L4.26878 16.8851C5.45164 18.8917 6.04307 19.895 7.03946 20.4475C8.03584 21 9.25367 21 11.6893 21H12.3107C14.7463 21 15.9642 21 16.9605 20.4475C17.9569 19.895 18.5484 18.8917 19.7312 16.8851L20.3207 15.8851C21.4402 13.9861 22 13.0365 22 12C22 10.9635 21.4402 10.0139 20.3207 8.11485L19.7312 7.11486C18.5484 5.10831 17.9569 4.10503 16.9605 3.55252C15.9642 3 14.7463 3 12.3107 3Z"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
			></path>
			<path
				d="M11.992 16H12.001"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
				stroke-linejoin="round"
			></path>
			<path
				d="M11.9922 13L11.9922 8"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
				stroke-linejoin="round"
			></path>
		</svg>
	{/snippet}
	{#snippet infoIcon()}
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none">
			<circle
				cx="12"
				cy="12"
				r="10"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
				stroke-linejoin="round"
			></circle>
			<path
				d="M12 16V11.8"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
				stroke-linejoin="round"
			></path>
			<path
				d="M12 8.01172V8.00172"
				stroke="currentColor"
				stroke-width="1.8"
				stroke-linecap="round"
				stroke-linejoin="round"
			></path>
		</svg>
	{/snippet}
</Toaster>

<style>
	:global(.my-toaster) {
		--error-bg: var(--red-1) !important;
		--error-text: var(--red-4) !important;
	}

	:global .my-toaster {
		.base-toast {
			height: var(--initial-height) !important;
			padding: 1.5rem 1rem !important;
		}

		.toast-icon {
			width: 1.5rem !important;
			height: 1.5rem !important;
			color: inherit;
		}
	}
</style>
