module WebApi.BackendResponse

open WebApi.Types

type BackendResponse<'T> =
    | Success of 'T
    | Failure of BackendErr list
