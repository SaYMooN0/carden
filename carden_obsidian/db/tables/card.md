```
CREATE TYPE content_item AS (
	Value VARCHAR(4096)
	Type VARCHAR(30)
	OrderInCard INT
)

CREATE TABLE card (
    Id UUID PRIMARY KEY,
    DeckId UUID NOT NULL,
	
    ContentFront content_item[] NOT NULL,
	ContentBack content_item[] NOT NULL,

    LastTimeEdited TIMESTAMPTZ NOT NULL,
    CreationTime TIMESTAMPTZ NOT NULL,
    FOREIGN KEY (DeckId) REFERENCES deck(Id) ON DELETE CASCADE
);
```