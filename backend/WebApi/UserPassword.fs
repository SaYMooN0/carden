module WebApi.UserPassword

open System
open BCrypt.Net
open Domain.Models

type UserPassword = private UserPassword of string
let MinLength = 6
let MaxLength = 30

type PasswordCreationErr =
    | NoValue
    | TooShort
    | TooLong

let tryCreate (value: string) : Result<UserPassword, PasswordCreationErr> =
    if String.IsNullOrWhiteSpace value then Error NoValue
    else if value.Length < MinLength then Error TooShort
    else if value.Length > MaxLength then Error TooLong
    else Ok(UserPassword value)

let value (UserPassword v) = v

type PasswordHasher() =
    member _.HashPassword(password: UserPassword) : PasswordHash =
        PasswordHash(BCrypt.HashPassword(value password))

    member _.VerifyPassword (hash: PasswordHash) (password: UserPassword) =
        BCrypt.Verify(value password, string hash)
