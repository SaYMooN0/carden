module Domain.CardContentItem

open System


type CardContentItem = private CardContentItem of string

[<RequireQualifiedAccess>]
type CardContentItemCreationErr =
    | Empty
    | TooLong

module CardContentItem =
    [<Literal>]
    let TextMaxLength = 4096


    let tryCreate (value: string) : Result<CardContentItem, CardContentItemCreationErr> =
        if String.IsNullOrWhiteSpace value then
            Error CardContentItemCreationErr.Empty
        elif value.Length > TextMaxLength then
            Error CardContentItemCreationErr.TooLong
        else
            Ok(CardContentItem value)

    let value (CardContentItem item: CardContentItem) = item
