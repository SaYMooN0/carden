<script lang="ts">
	import ConfirmationLinkSent from './_c_sign_in/ConfirmationLinkSent.svelte';
	import ForgotPasswordState from './_c_sign_in/ForgotPasswordState.svelte';
	import LoginState from './_c_sign_in/LoginState.svelte';
	import PasswordResetLinkSent from './_c_sign_in/PasswordResetLinkSent.svelte';
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
		{:else if currentState === 'password-forgotten'}
			<ForgotPasswordState bind:email {changeState} />
		{:else if currentState === 'password-reset-link-sent'}
			<PasswordResetLinkSent {email} />
		{:else}
			<p>unknown state</p>
			<button onclick={() => changeState('login')}>go back</button>
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
		max-width: 400px;
		margin: 2rem;
	}
</style>
