<script lang="ts">
	function formatDateTime(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) {
			return value;
		}
		return date.toLocaleString();
	}

	function removeTextContentItem(side: EditableSide, itemId: string) {
		if (!pageState.editingCard) {
			return;
		}

		pageState.editingCard[side] = pageState.editingCard[side].filter(
			(item) => item.stringId !== itemId
		);
	}

	function moveTextContentItem(
		fromSide: EditableSide,
		toSide: EditableSide,
		fromIndex: number,
		toIndex: number
	) {
		if (!pageState.editingCard) {
			return;
		}

		if (fromSide === toSide) {
			const nextItems = [...pageState.editingCard[fromSide]];
			const [movedItem] = nextItems.splice(fromIndex, 1);

			if (!movedItem) {
				return;
			}

			nextItems.splice(toIndex, 0, movedItem);
			pageState.editingCard[fromSide] = nextItems;
			return;
		}

		const nextSourceItems = [...pageState.editingCard[fromSide]];
		const nextTargetItems = [...pageState.editingCard[toSide]];
		const [movedItem] = nextSourceItems.splice(fromIndex, 1);

		if (!movedItem) {
			return;
		}

		nextTargetItems.splice(toIndex, 0, movedItem);
		pageState.editingCard[fromSide] = nextSourceItems;
		pageState.editingCard[toSide] = nextTargetItems;
	}
</script>

{@const editingCard = pageState.cardEditingState.card}
<div class="editor-header">
	<div>
		<div class="editor-header__eyebrow">editing card</div>
		<h2 class="editor-header__title">Deck card editor</h2>
		<p class="editor-header__meta">
			last edited {formatDateTime(editingCard.lastTimeEdited)}
		</p>
	</div>

	<div class="editor-header__actions">
		<span class:dirty={hasUnsavedChanges} class="status-pill">
			{hasUnsavedChanges ? 'unsaved changes' : 'saved'}
		</span>

		<button
			class="secondary-button"
			disabled={!hasUnsavedChanges}
			type="button"
			onclick={discardCurrentChanges}
		>
			reset
		</button>

		<button
			class="primary-button"
			disabled={!hasUnsavedChanges}
			type="button"
			onclick={saveCurrentCard}
		>
			save card
		</button>
	</div>
</div>

<div class="editor-grid">
	<SortableTextContentList
		group={`card-${editingCard.id}`}
		listId="contentFront"
		items={editingCard.contentFront}
		title="front side"
		subtitle="Question, prompt or title"
		onAddItem={() => addTextContentItem('contentFront')}
		onRemoveItem={(itemId) => removeTextContentItem('contentFront', itemId)}
		onMoveItem={moveTextContentItem}
		onUpdateText={(itemId, nextText) => updateTextContentItem('contentFront', itemId, nextText)}
	/>

	<SortableTextContentList
		group={`card-${editingCard.id}`}
		listId="contentBack"
		items={editingCard.contentBack}
		title="back side"
		subtitle="Answer, explanation or extra note"
		onAddItem={() => addTextContentItem('contentBack')}
		onRemoveItem={(itemId) => removeTextContentItem('contentBack', itemId)}
		onMoveItem={moveTextContentItem}
		onUpdateText={(itemId, nextText) => updateTextContentItem('contentBack', itemId, nextText)}
	/>
</div>
