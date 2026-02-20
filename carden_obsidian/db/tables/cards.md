```
CREATE TYPE content_item AS (
	Value VARCHAR(4096)
	Type VARCHAR(30)
)

CREATE TABLE cards (
    Id UUID PRIMARY KEY,
    DeckId UUID NOT NULL,
	
    ContentFront content_item[] NOT NULL,
	ContentBack content_item[] NOT NULL,

    LastTimeEdited TIMESTAMP NOT NULL,
    CreationTime TIMESTAMP NOT NULL,
    FOREIGN KEY (DeckId) REFERENCES decks(Id) ON DELETE CASCADE
);
```