module WebApi.Validation

open System.Net
open Giraffe

open WebApi.BackendResponse

let map2 (f: 'a -> 'b -> 'c) (ra: Result<'a, 'e list>) (rb: Result<'b, 'e list>) : Result<'c, 'e list> =
    match ra, rb with
    | Ok a, Ok b -> Result.Ok(f a b)
    | Error ea, Ok _ -> Error ea
    | Ok _, Error eb -> Error eb
    | Error ea, Error eb -> Error(ea @ eb)

let withValidatedBody<'raw, 'parsed>
    (parse: 'raw -> Result<'parsed, BackendResponseErr list>)
    (onValid: 'parsed -> HttpHandler)
    : HttpHandler =
    fun next ctx ->
        task {
            let! raw = ctx.BindJsonAsync<'raw>()

            match parse raw with
            | Ok parsed -> return! onValid parsed next ctx
            | Error errs -> return! constructFailure HttpStatusCode.BadRequest errs next ctx
        }
