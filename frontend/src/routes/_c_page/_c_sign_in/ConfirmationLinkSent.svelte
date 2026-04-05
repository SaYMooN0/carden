<script lang="ts">
	interface Props {
		email: string;
	}

	let { email }: Props = $props();
</script>

<div class="confirmation-link-sent">
	<p>Ссылка для подтверждения отправлена на <strong>{email}</strong>.</p>
	<p class="subtext">
		Пожалуйста, проверьте почту и перейдите по ссылке для подтверждения аккаунта.
	</p>
</div>

<style>
	.confirmation-link-sent {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		text-align: center;
		padding: 2rem 0;
	}

	p {
		font-size: 1.125rem;
		color: var(--text);
		margin-bottom: 0.5rem;
	}

	.subtext {
		font-size: 0.95rem;
		color: var(--color-text-light);
	}

	strong {
		color: var(--color-terracotta);
	}
</style>
