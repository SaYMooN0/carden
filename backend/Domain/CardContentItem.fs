module Domain.CardContentItem

open System


[<RequireQualifiedAccess>]
type CardContentItem =
    private
    | Text of text: string
    | Image of image: string
    | MathAjax of expression: string

[<RequireQualifiedAccess>]
type TextContentItemCreationErr =
    | Empty
    | TooLong

[<RequireQualifiedAccess>]
type ImageContentItemCreationErr =
    | Empty
    | TooLong

[<RequireQualifiedAccess>]
type MathAjaxContentItemCreationErr =
    | Empty
    | TooLong

[<RequireQualifiedAccess>]
type CardContentItemCreationErr =
    | Text of TextContentItemCreationErr
    | Image of ImageContentItemCreationErr
    | MathAjax of MathAjaxContentItemCreationErr

module CardContentItem =
    [<Literal>]
    let TextMaxLength = 4096

    [<Literal>]
    let ImageMaxLength = 512

    [<Literal>]
    let MathAjaxMaxLength = 4096

    let private validateString
        (maxLength: int)
        (emptyErr: 'err)
        (tooLongErr: 'err)
        (value: string)
        : Result<string, 'err> =
        if String.IsNullOrWhiteSpace value then Error emptyErr
        elif value.Length > maxLength then Error tooLongErr
        else Ok value

    let createText (text: string) : Result<CardContentItem, TextContentItemCreationErr> =
        validateString TextMaxLength TextContentItemCreationErr.Empty TextContentItemCreationErr.TooLong text
        |> Result.map CardContentItem.Text

    let createImage (image: string) : Result<CardContentItem, ImageContentItemCreationErr> =
        validateString ImageMaxLength ImageContentItemCreationErr.Empty ImageContentItemCreationErr.TooLong image
        |> Result.map CardContentItem.Image

    let createMathAjax (expression: string) : Result<CardContentItem, MathAjaxContentItemCreationErr> =
        validateString
            MathAjaxMaxLength
            MathAjaxContentItemCreationErr.Empty
            MathAjaxContentItemCreationErr.TooLong
            expression
        |> Result.map CardContentItem.MathAjax

    let value (item: CardContentItem) : string =
        match item with
        | CardContentItem.Text text -> text
        | CardContentItem.Image image -> image
        | CardContentItem.MathAjax expression -> expression
