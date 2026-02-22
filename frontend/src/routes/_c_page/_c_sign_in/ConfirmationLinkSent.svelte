<script lang="ts">
	interface Props {
		email: string;
	}

	let { email }: Props = $props();
</script>

<div class="confirmation-link-sent">
	<p>Confirmation link has been sent to {email}</p>
</div>
