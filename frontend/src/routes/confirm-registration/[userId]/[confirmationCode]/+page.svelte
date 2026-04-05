<script lang="ts">
	import type { PageProps } from './$types';
	import DefaultErrBlock from '$lib/components/errs/DefaultErrBlock.svelte';

	let { data }: PageProps = $props();
</script>

<div class="view-shell">
	<div class="confirmation-card">
		{#if data.isSuccess}
			<div class="status-badge status-badge-success" aria-hidden="true">✓</div>

			<p class="eyebrow">Почта подтверждена</p>
			<h1 class="title">Всё готово</h1>

			<p class="description">
				Вы успешно подтвердили свою почту. Теперь вы можете войти в аккаунт, используя
				<span class="email">{data.data.confirmedEmail}</span>.
			</p>

			<a class="primary-link" href="/">Перейти на страницу входа</a>
		{:else}
			<div class="status-badge status-badge-error" aria-hidden="true">!</div>

			<p class="eyebrow eyebrow-error">Ошибка подтверждения</p>
			<h1 class="title">Что-то пошло не так</h1>

			<p class="description muted">
				Не удалось подтвердить вашу почту. Подробнее об ошибке — ниже.
			</p>

			<div class="error-block-wrap">
				<DefaultErrBlock errs={data.errs} />
			</div>

			<a class="secondary-link" href="/">Вернуться на страницу входа</a>
		{/if}
	</div>
</div>

<style>
	.view-shell {
		min-height: 100vh;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 2rem 1rem;
	}

	.confirmation-card {
		width: 100%;
		max-width: 40rem;
		display: flex;
		flex-direction: column;
		align-items: center;
		text-align: center;
		gap: 1rem;
		padding: 2rem;
		background: var(--primary-foreground);
		border: 0.125rem solid var(--color-sage);
		border-radius: 1.5rem;
		box-shadow: var(--shadow);
	}

	.status-badge {
		width: 4rem;
		height: 4rem;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: 999rem;
		font-size: 1.75rem;
		font-weight: 700;
		line-height: 1;
	}

	.status-badge-success {
		background: var(--color-sage);
		color: var(--text);
	}

	.status-badge-error {
		background: var(--red-2);
		color: var(--red-5);
	}

	.eyebrow {
		font-size: 0.875rem;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--color-text-light);
	}

	.eyebrow-error {
		color: var(--red-5);
	}

	.title {
		font-size: 2.25rem;
		font-weight: 600;
		line-height: 1.1;
		color: var(--text);
	}

	.description {
		max-width: 30rem;
		font-size: 1.125rem;
		line-height: 1.6;
		color: var(--text);
	}

	.muted {
		color: var(--color-text-light);
	}

	.email {
		display: inline-block;
		margin-left: 0.25rem;
		padding: 0.25rem 0.625rem;
		border-radius: 999rem;
		background: var(--color-sage-hover);
		color: var(--text);
		font-weight: 600;
		word-break: break-word;
	}

	.primary-link,
	.secondary-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3rem;
		padding: 0.75rem 1.25rem;
		border-radius: 999rem;
		font-size: 1rem;
		font-weight: 600;
		transition:
			background-color 0.2s ease,
			transform 0.2s ease,
			color 0.2s ease;
	}

	.primary-link {
		background: var(--primary);
		color: var(--primary-foreground);
	}

	.primary-link:hover {
		background: var(--primary-hov);
		transform: translateY(-0.0625rem);
	}

	.secondary-link {
		background: var(--color-sage-hover);
		color: var(--text);
	}

	.secondary-link:hover {
		background: var(--color-sage);
		transform: translateY(-0.0625rem);
	}

	.error-block-wrap {
		width: 100%;
		margin-top: 0.25rem;
		padding: 1rem;
		background: var(--red-1);
		border: 0.125rem solid var(--red-2);
		border-radius: 1rem;
		text-align: left;
	}
</style>
