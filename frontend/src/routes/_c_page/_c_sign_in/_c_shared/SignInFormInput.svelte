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
