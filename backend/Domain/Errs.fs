module Domain

type ErrCode =
    | Unspecified
    | NotImplemented
    | ProgramBug
    | IncorrectFormat

type ErrExtract =
    private
        { Code: ErrCode
          Msg: string
          Details: string option
          FixSuggestion: string option }


module ErrExtract =
    let code (err: ErrExtract) = err.Code
    let msg (err: ErrExtract) = err.Msg
    let details (err: ErrExtract) = err.Details
    let suggestion (err: ErrExtract) = err.FixSuggestion

module Err =

    let private createWithoutSuggestion code msg details =
        { Code = code
          Msg = msg
          Details = details
          FixSuggestion = None }

    let WithDetails details err = { err with Details = details }

    let withSuggestion suggestion err =
        { err with
            FixSuggestion = Some suggestion }

    let Unspecified msg =
        createWithoutSuggestion ErrCode.Unspecified msg (option.Some "Error details are not specified")

    let NotImplemented msg =
        createWithoutSuggestion ErrCode.NotImplemented msg (option.Some "Requested functionality is not implemented")

    let ProgramBug msg =
        createWithoutSuggestion ErrCode.ProgramBug msg (option.Some "Something unexpected happened")

    module IncorrectFormat =

        let WrongValue msg valueName fieldName =
            createWithoutSuggestion ErrCode.IncorrectFormat msg (option.Some $"Value '{valueName}' is not suitable for {fieldName}")
