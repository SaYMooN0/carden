<script lang="ts">
	import { Backend, RJO } from '$lib/ts/backend';
	import { toast } from 'svelte-sonner';

	let isMenuOpen = $state(false);

	function toggleMenu() {
		isMenuOpen = !isMenuOpen;
	}

	export function closeMenu() {
		isMenuOpen = false;
	}
	async function logout() {
		const response = await Backend.fetchJsonResponse('/auth/logout', RJO.POST({}));
		if (response.isSuccess) {
			window.location.href = '/';
		} else {
			toast.error('Could not log out');
		}
	}
</script>

<div class="account-container" onclick={(e) => e.stopPropagation()}>
	<button
		class="account-btn"
		onclick={toggleMenu}
		aria-label="Account Menu"
		class:active={isMenuOpen}
	>
		<svg
			xmlns="http://www.w3.org/2000/svg"
			viewBox="0 0 24 24"
			color="currentColor"
			fill="none"
			stroke="currentColor"
			stroke-linecap="round"
			stroke-linejoin="round"
		>
			<path
				d="M15.5 10.5C15.5 8.567 13.933 7 12 7C10.067 7 8.5 8.567 8.5 10.5C8.5 12.433 10.067 14 12 14C13.933 14 15.5 12.433 15.5 10.5Z"
			/>
			<path
				d="M22 12C22 6.47715 17.5228 2 12 2C6.47715 2 2 6.47715 2 12C2 17.5228 6.47715 22 12 22C17.5228 22 22 17.5228 22 12Z"
			/>
			<path d="M18 20C18 16.6863 15.3137 14 12 14C8.68629 14 6 16.6863 6 20" />
		</svg>
	</button>

	{#if isMenuOpen}
		<div class="context-menu">
			<button class="menu-item logout-btn" onclick={() => logout()}>
				<svg
					xmlns="http://www.w3.org/2000/svg"
					viewBox="0 0 24 24"
					fill="none"
					stroke="currentColor"
					stroke-linecap="round"
					stroke-linejoin="round"
				>
					<path
						d="M4.39267 4.00087C4 4.61597 4 5.41166 4 7.00304V16.997C4 18.5883 4 19.384 4.39267 19.9991C4.46279 20.109 4.5414 20.2132 4.62777 20.3108C5.11144 20.8572 5.87666 21.0758 7.4071 21.513C8.9414 21.9513 9.70856 22.1704 10.264 21.8417C10.3604 21.7847 10.45 21.7171 10.5313 21.6402C11 21.1965 11 20.3988 11 18.8034V5.19662C11 3.60122 11 2.80351 10.5313 2.35982C10.45 2.28288 10.3604 2.21527 10.264 2.15827C9.70856 1.82956 8.9414 2.0487 7.4071 2.48699C5.87666 2.92418 5.11144 3.14278 4.62777 3.68925C4.5414 3.78684 4.46279 3.89103 4.39267 4.00087Z"
					/>
					<path
						d="M11 4H13.0171C14.9188 4 15.8696 4 16.4604 4.58579C16.7898 4.91238 16.9355 5.34994 17 6M11 20H13.0171C14.9188 20 15.8696 20 16.4604 19.4142C16.7898 19.0876 16.9355 18.6501 17 18"
					/>
					<path
						d="M21 12H14M19.5 9.49994C19.5 9.49994 22 11.3412 22 12C22 12.6588 19.5 14.4999 19.5 14.4999"
					/>
				</svg>
				Logout
			</button>
		</div>
	{/if}
</div>

<style>
	.account-container {
		position: fixed;
		top: 1rem;
		right: 1rem;
		z-index: 50;
	}

	.account-btn {
		width: 2.75rem;
		height: 2.75rem;
		border: none;
		background-color: #fff;
		cursor: pointer;
		display: flex;
		justify-content: center;
		align-items: center;
		color: var(--color-terracotta);
		stroke-width: 2;
		padding: 0.5rem;
		border-radius: 50%;
		box-shadow: var(--shadow);
		transition: all 0.2s cubic-bezier(0.175, 0.885, 0.32, 1.275);
	}

	.account-btn:hover,
	.account-btn.active {
		background-color: var(--color-terracotta-light);
		color: var(--color-cream);
		border-radius: 35%;
		transform: scale(1.05);
	}

	.context-menu {
		position: absolute;
		top: calc(100% + 0.75rem);
		right: 0;
		background-color: #ffffff;
		border-radius: 0.5rem;
		padding: 0.375rem;
		box-shadow:
			0 10px 25px -5px rgba(0, 0, 0, 0.1),
			0 8px 10px -6px rgba(0, 0, 0, 0.1);
		animation: slideDown 0.2s cubic-bezier(0.16, 1, 0.3, 1);
		transform-origin: top right;
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}

	.menu-item {
		width: 100%;
		display: flex;
		align-items: center;
		gap: 0.5rem;
		padding: 0.25rem 1rem;
		border: none;
		background-color: transparent;
		font-weight: 500;
		font-size: 1.125rem;
		cursor: pointer;
		border-radius: 0.5rem;
		transition: all 0.15s ease;
	}

	.logout-btn {
		color: var(--red-3);
	}
	.logout-btn > svg {
		height: 1.25rem;
		width: 1.25rem;
		stroke-width: 2;
	}

	.logout-btn:hover {
		background-color: var(--red-2);
		color: var(--red-4);
	}

	@keyframes slideDown {
		from {
			opacity: 0;
			transform: scale(0.92) translateY(-10px);
		}
		to {
			opacity: 1;
			transform: scale(1) translateY(0);
		}
	}
</style>
