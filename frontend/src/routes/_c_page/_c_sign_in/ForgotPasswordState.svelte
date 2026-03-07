<script lang="ts">
	import DefaultErrBlock from '$lib/components/errs/DefaultErrBlock.svelte';
	import PlantIcon from '$lib/icons/PlantIcon.svelte';
	import { Backend, RJO } from '$lib/ts/backend';
	import type { Err } from '$lib/ts/err';
	import { StringUtils } from '$lib/ts/utils/string-utils';
	import SignInFormConfirmButton from './_c_shared/SignInFormConfirmButton.svelte';
	import SignInFormInput from './_c_shared/SignInFormInput.svelte';
	import SignInFormLink from './_c_shared/SignInFormLink.svelte';
	import type { SignInFormState } from './sign-in-form-page';

	interface Props {
		email: string;
		changeState: (val: SignInFormState) => void;
	}

	let { email = $bindable(), changeState }: Props = $props();

	let isLoading = $state(false);
	let errs: Err[] = $state([]);

	async function confirmForgotPassword() {
		if (isLoading) {
			return;
		}

		validateForm();

		if (errs.length > 0) {
			return;
		}

		isLoading = true;
		const response = await Backend.fetchJsonResponse<void>(
			'/auth/forgot-password',
			RJO.POST({ email })
		);
		isLoading = false;

		if (response.isSuccess) {
			changeState('password-reset-link-sent');
		} else {
			errs = response.errs;
		}
	}

	function validateForm(): Err[] {
		errs = [];

		if (StringUtils.isNullOrWhiteSpace(email)) {
			errs.push({ msg: 'Email is required' });
		} else if (!StringUtils.isValidEmail(email)) {
			errs.push({ msg: 'Email is invalid' });
		}

		return errs;
	}
</script>

<div class="form-hero">
	<PlantIcon size="54px" color="var(--color-sage)" />
	<h2>Forgot password?</h2>
	<p>Enter your email and we will send you a reset link</p>
</div>

<SignInFormInput type="email" fieldName="Email" bind:value={email} />
<div class="gap" />
<SignInFormLink text="Back to log in" onClick={() => changeState('login')} />
<DefaultErrBlock errList={errs} class="forgot-password-err-block" />
<SignInFormConfirmButton
	text="Send reset link"
	onclick={() => confirmForgotPassword()}
	{isLoading}
/>

<style>
	.gap {
		margin-top: auto;
	}

	:global(.err-block.forgot-password-err-block) {
		margin-top: 0.375rem;
	}

	.form-hero {
		display: flex;
		flex-direction: column;
		align-items: center;
		margin-bottom: 2rem;
	}

	.form-hero h2 {
		margin-top: 1rem;
		font-size: 1.5rem;
		font-weight: 600;
		color: var(--text);
	}

	.form-hero p {
		margin-top: 0.25rem;
		font-size: 0.95rem;
		color: var(--color-text-light);
		text-align: center;
	}
</style>
