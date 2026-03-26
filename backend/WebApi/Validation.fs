module WebApi.Validation

open System.Net
open Giraffe

open Microsoft.AspNetCore.Http
open WebApi.BackendResponse

let map2 (f: 'a -> 'b -> 'c) (ra: Result<'a, 'e list>) (rb: Result<'b, 'e list>) : Result<'c, 'e list> =
    match ra, rb with
    | Ok a, Ok b -> Result.Ok(f a b)
    | Error ea, Ok _ -> Error ea
    | Ok _, Error eb -> Error eb
    | Error ea, Error eb -> Error(ea @ eb)

let map3 f r1 r2 r3 =
    match r1, r2, r3 with
    | Ok v1, Ok v2, Ok v3 -> Ok(f v1 v2 v3)

    | _ ->
        let errors =
            [ match r1 with
              | Error e -> yield! e
              | _ -> ()

              match r2 with
              | Error e -> yield! e
              | _ -> ()

              match r3 with
              | Error e -> yield! e
              | _ -> () ]

        Error errors

let withValidatedBody<'raw, 'parsed>
    (parse: 'raw -> Result<'parsed, BackendResponseErr list>)
    (onValid: 'parsed -> HttpFunc -> HttpContext -> HttpFuncResult)
    : HttpHandler =
    fun next ctx ->
        task {
            let! raw = ctx.BindJsonAsync<'raw>()

            match parse raw with
            | Ok parsed -> return! onValid parsed next ctx
            | Error errs -> return! constructFailure HttpStatusCode.BadRequest errs next ctx
        }
