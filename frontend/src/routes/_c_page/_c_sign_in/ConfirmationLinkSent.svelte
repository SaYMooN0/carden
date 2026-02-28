<script lang="ts">
	interface Props {
		email: string;
	}

	let { email }: Props = $props();
</script>

<div class="confirmation-link-sent">
	<p>Confirmation link has been sent to <strong>{email}</strong>.</p>
	<p class="subtext">Please check your inbox and follow the link to verify your account.</p>
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
		color: var(--color-text-light, #64748b);
	}

	strong {
		color: var(--color-terracotta);
	}
</style>
