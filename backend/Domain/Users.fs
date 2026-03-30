module Domain.Users

open System
open System.Security.Cryptography
open Domain.Plants

type PasswordHash = PasswordHash of string

module PasswordHash =
    let value (PasswordHash h) = h

type AppUser =
    { Id: AppUserId
      Email: Email.Email
      PasswordHash: PasswordHash
      RegistrationDate: DateTimeOffset }

type UnconfirmedUserId = UnconfirmedUserId of Guid

module UnconfirmedUserId =
    let value (UnconfirmedUserId value) = value


type ConfirmationCode = private ConfirmationCode of string

module ConfirmationCode =
    let value (ConfirmationCode value) = value

    let tryCreate (valueToValidate: string) : Result<ConfirmationCode, string> =
        let value =
            if isNull valueToValidate then
                ""
            else
                valueToValidate.Trim()

        if String.IsNullOrWhiteSpace value then
            Error "Confirmation code is required"
        elif value.Length > 128 then
            Error "Confirmation code is too long"
        else
            Ok(ConfirmationCode value)

    let generate () : ConfirmationCode =
        RandomNumberGenerator.GetBytes(32) |> Convert.ToHexString |> ConfirmationCode


type UnconfirmedUser =
    { Id: UnconfirmedUserId
      Email: Email.Email
      PasswordHash: PasswordHash
      ConfirmationCode: ConfirmationCode }

module UnconfirmedUser =
    let toConfirmedUser (registrationDate: DateTimeOffset) (unconfirmed: UnconfirmedUser) : AppUser =
        { Id = AppUserId(UnconfirmedUserId.value unconfirmed.Id)
          Email = unconfirmed.Email
          PasswordHash = unconfirmed.PasswordHash
          RegistrationDate = registrationDate }
