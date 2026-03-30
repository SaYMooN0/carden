module Domain.PlantName

open System
type PlantName = private PlantName of string

type PlantNameCreationErr =
    | NoValue
    | TooLong


module PlantName =
    let MaxLength = 100

    let tryCreate (valueToValidate: string) : Result<PlantName, PlantNameCreationErr> =
        let value =
            if isNull valueToValidate then
                ""
            else
                valueToValidate.Trim()

        if String.IsNullOrWhiteSpace value then Error NoValue
        elif value.Length > MaxLength then Error TooLong
        else Ok(PlantName value)

    let value (PlantName value) = value
