<script lang="ts">
	import ConfirmationLinkSent from './_c_sign_in/ConfirmationLinkSent.svelte';
	import LoginState from './_c_sign_in/LoginState.svelte';
	import type { SignInFormState } from './_c_sign_in/sign-in-form-page';
	import SignUpState from './_c_sign_in/SignUpState.svelte';

	let email = $state('');
	let password = $state('');

	let currentState: SignInFormState = $state('login');

	function changeState(val: SignInFormState) {
		currentState = val;
	}
</script>

<div class="sign-in-page-wrapper">
	<div class="sign-in-form-container">
		{#if currentState === 'login'}
			<LoginState bind:email bind:password {changeState} />
		{:else if currentState === 'signup'}
			<SignUpState bind:email bind:password {changeState} />
		{:else if currentState === 'confirmation-sent'}
			<ConfirmationLinkSent {email} />
		{:else}
			<p>неизвестное состояние</p>
			<button onclick={() => changeState('login')}>вернуться</button>
		{/if}
	</div>
</div>

<style>
	.sign-in-page-wrapper {
		display: flex;
		justify-content: center;
		align-items: center;
		min-height: 100vh;
		width: 100%;
	}

	.sign-in-form-container {
		display: flex;
		flex-direction: column;
		background: #ffffff;
		padding: 2.5rem 2rem;
		border-radius: 1rem;
		box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05);
		width: 100%;
		max-width: 500px;
		margin: 2rem;
	}
</style>
