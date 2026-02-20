``` 
INSERT INTO cards (  
Id,  
DeckId,  
ContentFront,  
ContentBack,  
LastTimeEdited,  
CreationTime  
)  
VALUES (  
$1, -- UUID (CardId)  
$2, -- UUID (DeckId)  
  
$3, -- content_item[]  
$4, -- content_item[]  
  
$5, -- LastTimeEdited  
$6 -- CreationTime  
);
``` 