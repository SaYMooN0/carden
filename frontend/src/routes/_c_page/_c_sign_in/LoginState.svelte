<script lang="ts">
	import DefaultErrBlock from '$lib/components/errs/DefaultErrBlock.svelte';
	import { Backend, RJO } from '$lib/ts/backend';
	import type { Err } from '$lib/ts/err';
	import { StringUtils } from '$lib/ts/utils/string-utils';
	import SignInFormConfirmButton from './_c_shared/SignInFormConfirmButton.svelte';
	import SignInFormHeader from './_c_shared/SignInFormHeader.svelte';
	import SignInFormInput from './_c_shared/SignInFormInput.svelte';
	import SignInFormLink from './_c_shared/SignInFormLink.svelte';
	import type { SignInFormState } from './sign-in-form-page';
	interface Props {
		email: string;
		password: string;
		changeState: (val: SignInFormState) => void;
	}
	let { email = $bindable(), password = $bindable(), changeState }: Props = $props();
	let isLoading = $state(false);
	let errs: Err[] = $state([]);

	async function confirmLogin() {
		if (isLoading) {
			return;
		}
		validateForm();
		if (errs.length > 0) {
			return;
		}
		isLoading = true;
		const response = await Backend.fetchJsonResponse<void>(
			'/auth/login',
			RJO.POST({ email, password })
		);
		isLoading = false;
		if (response.isSuccess) {
			window.location.reload();
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
		return errs;
	}
</script>

<SignInFormHeader text="Log into your account" />
<SignInFormInput type="email" fieldName="Email" bind:value={email} />
<SignInFormInput type="password" fieldName="Password" bind:value={password} />
<div class="gap" />
<SignInFormLink text="I don't have an account yet" onClick={() => changeState('signup')} />
<DefaultErrBlock errList={errs} class="login-err-block" />
<SignInFormConfirmButton text="Log in" onclick={() => confirmLogin()} {isLoading} />

<style>
	.gap {
		margin-top: auto;
	}

	:global(.err-block.login-err-block) {
		margin-top: 0.375rem;
	}
</style>
