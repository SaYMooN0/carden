module WebApi.EmailService

open System.Threading.Tasks
open Domain.Models
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

    member private _.ConfigureSmtpClient() : Task<SmtpClient> =
        task {
            let client = new SmtpClient()
            do! client.ConnectAsync(config.Host, config.Port, true)
            do! client.AuthenticateAsync(config.Username, config.Password)
            return client
        }

    member private this.SendEmailWithHtmlBody (toEmail: Email.Email) (subject: string) (body: string) : Task<Result<unit, string>> =
        task {
            try
                let message = new MimeMessage()
                message.From.Add(MailboxAddress("Carden", config.Username))
                message.To.Add(MailboxAddress("", Email.value toEmail))
                message.Subject <- subject

                let bodyPart = new TextPart("html")
                bodyPart.Text <- body
                message.Body <- bodyPart

                let! smtpClient = this.ConfigureSmtpClient()
                use client = smtpClient

                let! _ = client.SendAsync(message)
                do! client.DisconnectAsync(true)

                return Ok()
            with _ ->
                return Error "Could not establish connection to send an email"
        }



    member this.SendRegistrationConfirmationLink
        (email: Email.Email)
        (userId: UnconfirmedUserId)
        (confirmationCode: ConfirmationCode)
        : Task<Result<unit, string>> =
        task {
            let link = buildConfirmationLink userId confirmationCode

            let subject = "Confirm your email address"

            let body =
                $"""
                    <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #1e293b;">
                        <h2 style="color: #4f46e5;">Confirm your email address</h2>
                        <p>
                            Thank you for creating an account. Please confirm your email address to activate it.
                        </p>
                        <p style="margin: 1.5rem 0;">
                            <a href="{link}"
                               style="background-color: #4f46e5; color: #ffffff; text-decoration: none;
                                      padding: 0.6rem 1.2rem; border-radius: 6px; font-weight: bold;">
                                Confirm Email
                            </a>
                        </p>
                        <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 1.5rem 0;" />
                        <p style="font-size: 0.85rem; color: #64748b;">
                            If the button above does not work, copy and paste this link into your browser:<br />
                            <a href="{link}" style="color: #4f46e5;">{link}</a>
                        </p>
                    </div>
                    """

            return! this.SendEmailWithHtmlBody email subject body
        }
