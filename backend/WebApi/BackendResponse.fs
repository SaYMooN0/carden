module WebApi.BackendResponse

open System
open System.Net
open Giraffe


type BackendResponseErr =
    { Msg: string
      Details: string option
      FixSuggestion: string option
      ExtraData: {| id: string; data: obj |} option }

module BackendResponseErr =
    let create msg : BackendResponseErr =
        { Msg = msg
          Details = None
          FixSuggestion = None
          ExtraData = None }

    let setDetails details err : BackendResponseErr = { err with Details = Some details }

    let setFixSuggestion suggestion err : BackendResponseErr =
        { err with
            FixSuggestion = Some suggestion }

    let private setExtraData (id: string) data err =
        { err with
            ExtraData = Some {| id = id; data = data |} }

    module SetExtraData =
        let noMatchedEndpoint (data: {| Method: string; Route: string |}) err : BackendResponseErr =
            setExtraData "NO_MATCHED_ENDPOINT" data err

        let serverException (ex: Exception) err : BackendResponseErr =
            setExtraData "SERVER_EXCEPTION" {| Exception = ex.Message |} err

type BackendFailure = BackendFailure of BackendResponseErr list

let constructSuccess<'T> (data: 'T) (status: HttpStatusCode) : HttpHandler =
    fun next ctx ->
        ctx.SetStatusCode(int status)
        json {| IsSuccess = true; data = data |} next ctx

let constructFailure (status: HttpStatusCode) errs : HttpHandler =
    fun next ctx ->
        ctx.SetStatusCode(int status)
        json {| isSuccess = false; errs = errs |} next ctx
