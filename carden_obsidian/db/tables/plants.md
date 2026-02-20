```
CREATE TABLE plants (
    Id UUID PRIMARY KEY,
    OwnerId UUID NOT NULL,
    Description TEXT,
    DeckId UUID NOT NULL UNIQUE,
    PotTypeId VARCHAR(50) NOT NULL,
    PlantSpecieId VARCHAR(50) NOT NULL,
    CreationDate TIMESTAMP NOT NULL,
    FOREIGN KEY (OwnerId) REFERENCES users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DeckId) REFERENCES decks(Id) ON DELETE CASCADE,
    FOREIGN KEY (PotTypeId) REFERENCES pot_types(Id),
    FOREIGN KEY (PlantSpecieId) REFERENCES plant_species(Id)
);
```