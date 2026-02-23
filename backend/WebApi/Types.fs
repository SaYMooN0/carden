module WebApi.Types

open Domain.Errs

type private ListOfInvalidFields = {| Name: string; Message: string |} list

type ApiLvlErr =
    | NoMatchingEndpoint of {| Method: string; Route: string |}
    | RequestBodyParseErr of ListOfInvalidFields

type BackendErr =
    | DomainErr of Err
    | ApiErr of ApiLvlErr
type ErrResponse<'a> =
    { Msg: string
      Code: string
      Details: string option
      FixSuggestion: string option
      AdditionalData: 'a option }

module ErrResponse =
    let private getStringForDomainErr (err: Err) =
        match (ErrExtract.code err) with
        | Unspecified -> "unspecified"
        | NotImplemented -> "not_implemented"
        | ProgramBug -> "program_bug"
        | IncorrectFormat -> "incorrect_format"
        | NoValue -> "no_value"

    let private getStringForApiErr (err: ApiLvlErr) =
        match err with
        | NoMatchingEndpoint _ -> "no_matching_endpoint"
        | RequestBodyParseErr _ -> "request_parse_err"

    let fromDomainErr (err: Err) : ErrResponse<'a> =
        { Msg = ErrExtract.msg err
          Code = getStringForDomainErr err
          Details = ErrExtract.details err
          FixSuggestion = ErrExtract.suggestion err
          AdditionalData = None }

    let createFromApiErr msg (err: ApiLvlErr) : ErrResponse<'a> =
        { Msg = msg
          Code = getStringForApiErr err
          Details = None
          FixSuggestion = None
          AdditionalData = None }

    let setAdditionalData data err = { err with AdditionalData = Some data }

    let fromApiLvlErr (err: ApiLvlErr) : ErrResponse<ApiLvlErr> =
        let msg =
            match err with
            | NoMatchingEndpoint _ ->"There is no matching endpoint"
            | RequestBodyParseErr  _ -> "Some errors in the input data"
        createFromApiErr msg err
