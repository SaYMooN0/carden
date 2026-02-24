module WebApi.BackendResponse

open System
open System.Net
open Domain.Errs
open Giraffe


type BackendResponseErr =
    { Msg: string
      Details: string option
      FixSuggestion: string option
      AdditionalData: {| id: string; data: obj |} option }

module BackendResponseErr =
    let create msg : BackendResponseErr =
        { Msg = msg
          Details = None
          FixSuggestion = None
          AdditionalData = None }

    let setDetails details err : BackendResponseErr = { err with Details = Some details }

    let setFixSuggestion suggestion err : BackendResponseErr =
        { err with
            FixSuggestion = Some suggestion }

    let private setAdditionalData (id: string) data err =
        { err with
            AdditionalData = Some {| id = id; data = data |} }

    module SetAdditionalData =
        let noMatchedEndpoint (data: {| Method: string; Route: string |}) err : BackendResponseErr =
            setAdditionalData "NO_MATCHED_ENDPOINT" data err

        let serverException (ex: Exception) err : BackendResponseErr =
            setAdditionalData "SERVER_EXCEPTION" {| Exception = ex.Message |} err

type BackendFailure = BackendFailure of BackendResponseErr list

let constructSuccess<'T> (data: 'T) (status: HttpStatusCode) : HttpHandler =
    fun next ctx ->
        ctx.SetStatusCode(int status)
        json {| IsSuccess = true; data = data |} next ctx

let constructFailure (status: HttpStatusCode) errs : HttpHandler =
    fun next ctx ->
        ctx.SetStatusCode(int status)
        json {| isSuccess = false; errs = errs |} next ctx
