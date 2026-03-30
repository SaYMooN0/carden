module Domain.Email
open System
open System.Text.RegularExpressions

type Email = private Email of string
module Email =

    type EmailCreationErr =
        | NoValue
        | IncorrectFormat of {| Value: string |}

    let private emailRegex = Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)

    let tryCreate (value: string) : Result<Email, EmailCreationErr> =
        if String.IsNullOrWhiteSpace value then Error NoValue
        else if emailRegex.IsMatch value then Ok(Email value)
        else Error(IncorrectFormat {| Value = value |})


    let value (Email v) = v