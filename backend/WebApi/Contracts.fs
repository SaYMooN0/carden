module WebApi.Contracts

open Domain.Models
open Domain.Models.Email
open WebApi.BackendResponse
open WebApi.UserPassword
open WebApi.Validation

type RawSignUpRequest =
    { Email: string
      Password: string
      ConfirmPassword: string }

type ParsedSignUpRequest =
    { Email: Email; Password: UserPassword }

module RawSignUpRequest =
    let private validateEmail (rawEmail: string) : Result<Email, BackendResponseErr list> =
        Email.tryCreate rawEmail
        |> Result.mapError (fun e ->
            match e with
            | Email.NoValue -> [ BackendResponseErr.create "No email provided" ]
            | IncorrectFormat _ -> [ BackendResponseErr.create $"{rawEmail} is not a valid email" ])

    let private validatePassword password confirm =
        map2
            (fun pwd _ -> pwd)
            (tryCreate password
             |> Result.mapError (fun e ->
                 match e with
                 | NoValue -> [ BackendResponseErr.create "Password is required" ]
                 | TooShort -> [ BackendResponseErr.create $"Password must be at least {MinLength} characters" ]
                 | TooLong -> [ BackendResponseErr.create $"Password must be at most {MaxLength} characters" ]))
            (if password = confirm then
                 Ok()
             else
                 Error [ BackendResponseErr.create "Passwords do not match" ])

    let parse (req: RawSignUpRequest) : Result<ParsedSignUpRequest, BackendResponseErr list> =
        map2
            (fun email password -> { Email = email; Password = password })
            (validateEmail req.Email)
            (validatePassword req.Password req.ConfirmPassword)


type RawLoginRequest = { Email: string; Password: string }

type ParsedLoginRequest =
    { Email: Email; Password: UserPassword }

module RawLoginRequest =
    let private validateEmail raw =
        Email.tryCreate raw
        |> Result.mapError (fun e ->
            match e with
            | Email.NoValue -> [ BackendResponseErr.create "No email provided" ]
            | IncorrectFormat _ -> [ BackendResponseErr.create "Invalid email format" ])

    let private validatePassword raw =
        tryCreate raw
        |> Result.mapError (fun e ->
            match e with
            | NoValue -> [ BackendResponseErr.create "Password is required" ]
            | TooShort -> [ BackendResponseErr.create "Password too short" ]
            | TooLong -> [ BackendResponseErr.create "Password too long" ])

    let parse (req: RawLoginRequest) =
        map2 (fun email password -> { Email = email; Password = password }) (validateEmail req.Email) (validatePassword req.Password)
