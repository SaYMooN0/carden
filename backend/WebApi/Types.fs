module WebApi.Types

open System.Net.Http
open Domain

type ApiLevelErr =
    | DomainErr of ErrCode
    | NoMatchingEndpoint

let parseErrToStringCode =
    function
    | DomainErr Unspecified -> "unspecified"
    | DomainErr NotImplemented -> "not_implemented"
    | DomainErr ProgramBug -> "program_bug"
    | DomainErr IncorrectFormat -> "incorrect_format"
    | NoMatchingEndpoint -> "no_matching_endpoint"


type  FlatErrResponse<'a>  =
    private
        { Msg: string
          Code: string
          Details: string option
          FixSuggestion: string option
          AdditionalData: 'a option }
        
module FlatErrResponse =
    let private create code msg details : FlatErrResponse<'a> =
        { Msg = msg
          Code = parseErrToStringCode code
          Details = details
          FixSuggestion = None
          AdditionalData = None }

    let fromErr (err: ErrExtract) =
        create
            (err |> ErrExtract.code |> DomainErr)
            (ErrExtract.msg err)
            (ErrExtract.details err)

    let NoMatchingEndpoint (method: HttpMethod) route =
        create
            NoMatchingEndpoint
            "There is no matching endpoint"
            (Some $"No endpoint mapped for {method.Method} '{route}'")