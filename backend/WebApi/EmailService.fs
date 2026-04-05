module WebApi.EmailService

open System.Threading.Tasks
open Domain.Email
open Domain.Plants
open Domain.Users
open MailKit.Net.Smtp
open MimeKit

type EmailServiceConfig =
    { Host: string
      Port: int
      Username: string
      Password: string }

type FrontendConfig =
    { BaseUrl: string
      ConfirmRegistrationPath: string }

type EmailService(config: EmailServiceConfig, frontendConfig: FrontendConfig) =

    let buildConfirmationLink (userId: UnconfirmedUserId) (confirmationCode: ConfirmationCode) =
        let baseUrl = frontendConfig.BaseUrl.TrimEnd('/')
        let confirmPath = frontendConfig.ConfirmRegistrationPath.Trim('/')

        $"{baseUrl}/{confirmPath}/{UnconfirmedUserId.value userId}/{ConfirmationCode.value confirmationCode}"

    member private _.ConfigureSmtpClient: Task<SmtpClient> =
        task {
            let client = new SmtpClient()
            do! client.ConnectAsync(config.Host, config.Port, true)
            do! client.AuthenticateAsync(config.Username, config.Password)
            return client
        }

    member private this.SendEmailWithHtmlBody
        (toEmail: Email)
        (subject: string)
        (body: string)
        (ct)
        : Task<Result<unit, string>> =
        task {
            try
                let message = new MimeMessage()
                message.From.Add(MailboxAddress("Carden", config.Username))
                message.To.Add(MailboxAddress("", Email.value toEmail))
                message.Subject <- subject

                let bodyPart = new TextPart("html")
                bodyPart.Text <- body
                message.Body <- bodyPart

                let! smtpClient = this.ConfigureSmtpClient
                use client = smtpClient

                let! _ = client.SendAsync(message)
                do! client.DisconnectAsync(true, ct)

                return Ok()
            with ex ->
                return Error "Could not establish connection to send an email"
        }



    member this.SendRegistrationConfirmationLink
        (email: Email)
        (userId: UnconfirmedUserId)
        (confirmationCode: ConfirmationCode)
        (ct)
        : Task<Result<unit, string>> =
        task {
            let link = buildConfirmationLink userId confirmationCode

            let subject = "Confirm your email address"

            let body =
                $"""
<div style="margin:0; padding:2rem 1rem; background-color:#f5f2e7;">
    <div style="
        max-width:36rem;
        margin:0 auto;
        background-color:#ffffff;
        border:1px solid #c5d3c1;
        border-radius:1.25rem;
        overflow:hidden;
        box-shadow:0 4px 6px rgba(0, 0, 0, 0.1);
        font-family:'Chiron GoRound TC', Arial, sans-serif;
        color:#1e293b;
    ">
        <div style="
            background-color:#c5d3c1;
            padding:1.25rem 1.5rem;
            text-align:center;
        ">
            <div style="
                display:inline-block;
                padding:0.375rem 0.75rem;
                background-color:#f5f2e7;
                color:#c57b58;
                border-radius:999px;
                font-size:0.875rem;
                font-weight:700;
                letter-spacing:0.02em;
            ">
                Carden
            </div>
        </div>

        <div style="padding:2rem 1.5rem 1.5rem 1.5rem;">
            <h2 style="
                margin:0 0 1rem 0;
                font-size:1.75rem;
                line-height:1.2;
                font-weight:700;
                color:#1e293b;
            ">
                Confirm your email address
            </h2>

            <p style="
                margin:0 0 1rem 0;
                font-size:1rem;
                line-height:1.7;
                color:#1e293b;
            ">
                Thank you for creating your account. Please confirm your email address to activate it and start growing your garden.
            </p>

            <div style="margin:2rem 0; text-align:center;">
                <a
                    href="{link}"
                    style="
                        display:inline-block;
                        background-color:#c57b58;
                        color:#ffffff;
                        text-decoration:none;
                        padding:0.875rem 1.5rem;
                        border-radius:0.875rem;
                        font-size:1rem;
                        font-weight:700;
                    "
                >
                    Confirm Email
                </a>
            </div>

            <div style="
                margin:1.5rem 0;
                padding:1rem;
                background-color:#f5f2e7;
                border:1px solid #c5d3c1;
                border-radius:0.875rem;
            ">
                <p style="
                    margin:0;
                    font-size:0.9375rem;
                    line-height:1.6;
                    color:#64748b;
                ">
                    If the button does not work, copy and paste this link into your browser:
                </p>
                <p style="margin:0.75rem 0 0 0; word-break:break-all;">
                    <a
                        href="{link}"
                        style="
                            color:#c57b58;
                            text-decoration:none;
                            font-weight:600;
                        "
                    >
                        {link}
                    </a>
                </p>
            </div>
        </div>

        <div style="
            padding:1rem 1.5rem 1.5rem 1.5rem;
            text-align:center;
        ">
            <p style="
                margin:0;
                font-size:0.8125rem;
                line-height:1.6;
                color:#64748b;
            ">
                If you did not create an account, you can safely ignore this email.
            </p>
        </div>
    </div>
</div>
"""

            return! this.SendEmailWithHtmlBody email subject body ct
        }
