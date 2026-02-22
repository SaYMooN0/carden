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

{#if currentState === 'login'}
	<LoginState bind:email bind:password {changeState} />
{:else if currentState === 'signup'}
	<SignUpState bind:email bind:password {changeState} />
{:else if currentState === 'confirmation-sent'}
	<ConfirmationLinkSent {email} />
{/if}
