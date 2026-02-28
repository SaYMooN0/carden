<script lang="ts">
	import DefaultErrBlock from '$lib/components/errs/DefaultErrBlock.svelte';
	import { Backend, RJO } from '$lib/ts/backend';
	import type { Err } from '$lib/ts/err';
	import { StringUtils } from '$lib/ts/utils/string-utils';
	import SignInFormConfirmButton from './_c_shared/SignInFormConfirmButton.svelte';
	import PlantIcon from '$lib/icons/PlantIcon.svelte';
	import SignInFormInput from './_c_shared/SignInFormInput.svelte';
	import SignInFormLink from './_c_shared/SignInFormLink.svelte';
	import type { SignInFormState } from './sign-in-form-page';

	interface Props {
		email: string;
		password: string;
		changeState: (val: SignInFormState) => void;
	}
	let { email = $bindable(), password = $bindable(), changeState }: Props = $props();
	let confirmPassword = $state('');
	let errs: Err[] = $state([]);

	let isLoading = $state(false);

	export function clearErrs() {
		errs = [];
	}

	async function confirmSignUp() {
		if (isLoading) {
			return;
		}
		validateForm();
		if (errs.length > 0) {
			return;
		}
		isLoading = true;
		const response = await Backend.fetchJsonResponse<void>(
			'/auth/sign-up',
			RJO.POST({ email, password, confirmPassword })
		);
		isLoading = false;
		if (response.isSuccess) {
			changeState('confirmation-sent');
		} else {
			errs = response.errs;
		}
	}
	function validateForm(): Err[] {
		errs = [];
		if (StringUtils.isNullOrWhiteSpace(email)) {
			errs.push({ msg: 'Email is required' });
		} else if (email.indexOf('@') === -1) {
			errs.push({ msg: 'Email is invalid' });
		}
		if (StringUtils.isNullOrWhiteSpace(password)) {
			errs.push({ msg: 'Password is required' });
		}
		if (StringUtils.isNullOrWhiteSpace(confirmPassword)) {
			errs.push({ msg: 'Confirm password is required' });
		} else if (password !== confirmPassword) {
			errs.push({ msg: 'Passwords do not match' });
		}
		return errs;
	}
</script>

<div class="form-hero">
	<PlantIcon size="54px" color="var(--color-sage)" />
	<h2>Create an account</h2>
	<p>Join Carden to get started</p>
</div>

<SignInFormInput type="email" fieldName="Email" bind:value={email} />
<SignInFormInput type="password" fieldName="Password" bind:value={password} />
<SignInFormInput type="password" fieldName="Confirm password" bind:value={confirmPassword} />
<div class="gap" />
<SignInFormLink text="I already have an account" onClick={() => changeState('login')} />
<DefaultErrBlock errList={errs} class="sign-up-err-block" />
<SignInFormConfirmButton text="Sign Up" onclick={() => confirmSignUp()} {isLoading} />

<style>
	.gap {
		margin-top: auto;
	}

	:global(.err-block.sign-up-err-block) {
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
		color: var(--color-text-light, #64748b);
	}
</style>
