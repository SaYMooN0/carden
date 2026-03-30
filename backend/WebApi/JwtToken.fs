module WebApi.JwtToken

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Security.Cryptography
open Domain.Plants
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens


type JwtToken = string

type TokenParsingErr =
    | InvalidToken
    | Unexpected

type JwtTokenConfig =
    { Issuer: string
      Audience: string
      PublicKey: string
      PrivateKey: string
      UserIdClaimKey: string }

type JwtTokenService(config: JwtTokenConfig, logger: ILogger<JwtTokenService>) =
    let privateKey =
        let rsa = RSA.Create()
        rsa.ImportFromPem(config.PrivateKey)
        RsaSecurityKey(rsa) :> SecurityKey

    let publicKey =
        let rsa = RSA.Create()
        rsa.ImportFromPem(config.PublicKey)
        RsaSecurityKey(rsa)

    let issuer = config.Issuer
    let audience = config.Audience
    let userIdClaimKey = config.UserIdClaimKey

    member _.CreateToken(user: AppUser) : JwtToken =
        try
            let userIdStr = (AppUserId.value user.Id).ToString()
            let claims = [| Claim(userIdClaimKey, userIdStr) |]
            let creds = SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256)

            let token =
                JwtSecurityToken(
                    issuer = issuer,
                    audience = audience,
                    claims = claims,
                    expires = DateTime.UtcNow.AddDays(30.0),
                    signingCredentials = creds
                )

            let tokenString = JwtSecurityTokenHandler().WriteToken(token)
            JwtToken tokenString

        with ex ->
            logger.LogError(
                ex,
                "Failed to generate JWT token for userId '{userId}'. Error: {errorMessage}",
                user.Id.ToString(),
                ex.Message
            )

            reraise ()




    member _.UserIdFromJwtToken(token: JwtToken) : Result<AppUserId, TokenParsingErr> =

        let parameters =
            TokenValidationParameters(
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = publicKey
            )

        let jwtHandler = JwtSecurityTokenHandler()
        let principal, _ as _ = jwtHandler.ValidateToken(string token, parameters)

        try
            principal.Claims
            |> Seq.tryFind (fun c -> c.Type = userIdClaimKey)
            |> function
                | None ->
                    logger.LogWarning("JWT token does not contain '{claimName}' claim.", userIdClaimKey)
                    Error TokenParsingErr.InvalidToken
                | Some claim ->
                    match Guid.TryParse claim.Value with
                    | true, guid -> Ok(AppUserId guid)
                    | false, _ ->
                        logger.LogWarning(
                            "JWT token contains malformed userId claim. Value: '{claimValue}'.",
                            claim.Value
                        )

                        Error TokenParsingErr.InvalidToken

        with ex ->
            logger.LogError(ex, "Unexpected error while parsing JWT token.")
            Error TokenParsingErr.Unexpected
