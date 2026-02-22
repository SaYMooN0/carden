<script>
	import DialogWithCloseButton from './components/dialogs/DialogWithCloseButton.svelte';

	const plants = ['/plant1.png', '/plant2.png', '/plant3.png'];
	const pots = ['/pot1.png', '/pot2.png', '/pot3.png'];

	let potIndex = $state(0);
	let plantIndex = $state(0);

	let title = $state('');
	let description = $state('');

	let dialogControl = $state();

	export function open() {
		dialogControl.open();
	}

	function handleSave() {
		dialogControl.close();
	}

	function handleCancel() {
		dialogControl.close();
	}

	function prevPlant() {
		if (plantIndex != 0) {
			plantIndex--;
		}
	}

	function nextPlant() {
		if (plantIndex != plants.length - 1) {
			plantIndex++;
		}
	}

	function prevPot() {
		if (potIndex != 0) {
			potIndex--;
		}
	}

	function nextPot() {
		if (potIndex != plants.length - 1) {
			potIndex++;
		}
	}
</script>

<DialogWithCloseButton bind:this={dialogControl}>
	<div class="modal-wrapper">
		<h2>Создание растения</h2>

		<label>
			<input bind:value={title} placeholder="Название растения" />
		</label>

		<label>
			<input bind:value={description} placeholder="Описание растения" />
		</label>

		<h3>Выберите тип растения</h3>
		<button onclick={prevPlant}> &lt; </button>
		<img src={plants[plantIndex]} alt="Plant" />
		<button onclick={nextPlant}> &gt; </button>

		<h3>Выберите горшок</h3>
		<button onclick={prevPot}> &lt; </button>
		<img src={pots[potIndex]} alt="Pot" />
		<button onclick={nextPot}> &gt; </button>

		<button onclick={handleSave}> Сохранить </button>

		<button onclick={handleCancel}> Отмена </button>
	</div>
</DialogWithCloseButton>

<style>
	.modal-wrapper {
		max-height: 70vh;
		overflow-y: auto;
	}
</style>
