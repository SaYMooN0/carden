module WebApi.Types

open System.Net.Http
open Domain.Errs

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


type ErrResponse<'a> =
    { Msg: string
      Code: string
      Details: string option
      FixSuggestion: string option
      AdditionalData: 'a option }

module ErrResponse =
    let createWithDetails code msg details : ErrResponse<'a> =
        { Msg = msg
          Code = parseErrToStringCode code
          Details = details
          FixSuggestion = None
          AdditionalData = None }

    let create code msg : ErrResponse<'a> = createWithDetails code msg None

    let fromErr (err: Err) =
        createWithDetails (err |> ErrExtract.code |> DomainErr) (ErrExtract.msg err) (ErrExtract.details err)

    let setAdditionalData data err = { err with AdditionalData = Some data }

    let NoMatchingEndpoint (method: string) route : ErrResponse<{| Method: string; Route: string |}> =
        create NoMatchingEndpoint "There is no matching endpoint"
        |> setAdditionalData {| Method = method; Route = route |}
