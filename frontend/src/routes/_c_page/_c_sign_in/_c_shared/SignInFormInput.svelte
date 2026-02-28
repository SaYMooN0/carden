<script lang="ts">
	import { StringUtils } from '$lib/ts/utils/string-utils';
	interface Props {
		type?: 'text' | 'password' | 'email';
		fieldName: string;
		value: string;
	}
	let { type = 'text', fieldName, value = $bindable() }: Props = $props();
	let inputName = StringUtils.rndStr(8);
</script>

<div class="input-field unselectable">
	<input bind:value {type} name={inputName} required autocomplete="off" placeholder=" " />
	<label for={inputName}>
		{fieldName}
	</label>
</div>

<style>
	.input-field {
		position: relative;
		margin-bottom: 1rem;
		width: 100%;
	}

	input {
		width: 100%;
		padding: 0.875rem 1rem;
		font-size: 1rem;
		border: 1.5px solid var(--color-sage);
		border-radius: 0.5rem;
		background-color: var(--color-cream);
		outline: none;
		transition: all 0.2s ease;
	}

	input:hover {
		background-color: var(--color-sage-hover);
	}

	input:focus {
		background-color: #ffffff;
		border-color: var(--color-terracotta);
		box-shadow: 0 0 0 3px rgba(197, 123, 88, 0.15);
	}

	label {
		position: absolute;
		left: 1rem;
		top: 50%;
		transform: translateY(-50%);
		font-size: 1rem;
		color: var(--color-text-light, #64748b);
		pointer-events: none;
		transition: all 0.2s ease;
		background-color: transparent;
		padding: 0;
	}

	/* Float label up when focused or not empty */
	input:focus ~ label,
	input:not(:placeholder-shown) ~ label {
		top: 0;
		font-size: 0.8rem;
		color: var(--color-terracotta);
		background-color: #ffffff;
		padding: 0 0.25rem;
		left: 0.75rem;
	}
</style>
