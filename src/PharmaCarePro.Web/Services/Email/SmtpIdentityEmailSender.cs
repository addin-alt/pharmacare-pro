using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using PharmaCarePro.Web.Data;

namespace PharmaCarePro.Web.Services.Email;

public sealed class SmtpIdentityEmailSender :
    IEmailSender<ApplicationUser>
{
    private readonly SmtpEmailOptions options;
    private readonly ILogger<SmtpIdentityEmailSender> logger;

    public SmtpIdentityEmailSender(
        IOptions<SmtpEmailOptions> options,
        ILogger<SmtpIdentityEmailSender> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    public Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink)
    {
        return SendLinkEmailAsync(
            email,
            "Confirm your PharmaCare Pro email",
            "Confirm your email address",
            """
            Confirm this email address to protect your pharmacy
            owner account and enable secure account recovery.
            """,
            "Confirm email address",
            confirmationLink,
            """
            You received this message because an email confirmation
            was requested for a PharmaCare Pro account.
            """);
    }

    public Task SendPasswordResetLinkAsync(
        ApplicationUser user,
        string email,
        string resetLink)
    {
        return SendLinkEmailAsync(
            email,
            "Reset your PharmaCare Pro password",
            "Reset your password",
            """
            A password reset was requested for your PharmaCare Pro
            owner account. Use the secure button below to choose a
            new password.
            """,
            "Reset password",
            resetLink,
            """
            If you did not request this password reset, you can
            safely ignore this message.
            """);
    }

    public Task SendPasswordResetCodeAsync(
        ApplicationUser user,
        string email,
        string resetCode)
    {
        var encodedCode =
            WebUtility.HtmlEncode(resetCode);

        var htmlBody = BuildEmailTemplate(
            "Password reset code",
            """
            Use the following security code to continue resetting
            your PharmaCare Pro password.
            """,
            $$"""
            <div style="
                margin:24px 0;
                padding:18px;
                border:1px solid #bfe8dc;
                border-radius:14px;
                background:#effaf6;
                color:#08745f;
                font-size:24px;
                font-weight:800;
                letter-spacing:4px;
                text-align:center;">
                {{encodedCode}}
            </div>
            """,
            """
            If you did not request this code, you can safely
            ignore this message.
            """);

        var textBody =
            $"""
            PharmaCare Pro

            Password reset code

            Use this security code to reset your password:

            {resetCode}

            If you did not request this code, ignore this message.
            """;

        return SendEmailAsync(
            email,
            "Your PharmaCare Pro password reset code",
            htmlBody,
            textBody);
    }

    private Task SendLinkEmailAsync(
        string email,
        string subject,
        string heading,
        string introduction,
        string buttonText,
        string actionLink,
        string footerText)
    {
        /*
         * ASP.NET Identity supplies an HTML-safe link.
         * Do not encode the link a second time.
         */
        var htmlBody = BuildEmailTemplate(
            heading,
            introduction,
            $$"""
            <div style="margin:26px 0;">
                <a href="{{actionLink}}"
                   style="
                       display:inline-block;
                       padding:14px 22px;
                       border-radius:12px;
                       background:#0d9f85;
                       color:#ffffff;
                       font-size:15px;
                       font-weight:800;
                       text-decoration:none;">
                    {{buttonText}}
                </a>
            </div>

            <p style="
                margin:0;
                color:#7a8797;
                font-size:12px;
                line-height:1.6;">
                Button not working? Copy and paste this address:
            </p>

            <p style="
                margin:6px 0 0;
                color:#08745f;
                font-size:11px;
                line-height:1.6;
                overflow-wrap:anywhere;">
                {{actionLink}}
            </p>
            """,
            footerText);

        var plainLink =
            WebUtility.HtmlDecode(actionLink);

        var textBody =
            $"""
            PharmaCare Pro

            {heading}

            {introduction}

            {buttonText}:
            {plainLink}

            {footerText}
            """;

        return SendEmailAsync(
            email,
            subject,
            htmlBody,
            textBody);
    }

    private async Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string textBody)
    {
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                options.FromName,
                options.FromAddress));

        message.To.Add(
            MailboxAddress.Parse(recipientEmail));

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        client.Timeout = 30_000;

        var socketOptions =
            options.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

        try
        {
            await client.ConnectAsync(
                options.Host,
                options.Port,
                socketOptions);

            await client.AuthenticateAsync(
                options.Username,
                options.Password);

            await client.SendAsync(message);

            await client.DisconnectAsync(
                quit: true);

            logger.LogInformation(
                "Identity email sent successfully. Subject: {Subject}",
                subject);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Identity email delivery failed. Subject: {Subject}",
                subject);

            throw;
        }
    }

    private static string BuildEmailTemplate(
        string heading,
        string introduction,
        string mainContent,
        string footerText)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <meta name="viewport"
                  content="width=device-width, initial-scale=1">
        </head>

        <body style="
            margin:0;
            padding:30px 14px;
            background:#f2f7f6;
            font-family:Arial, Helvetica, sans-serif;
            color:#142238;">

            <div style="
                width:100%;
                max-width:600px;
                margin:0 auto;
                overflow:hidden;
                border:1px solid #dce8e5;
                border-radius:20px;
                background:#ffffff;
                box-shadow:0 18px 45px rgba(10, 51, 62, 0.10);">

                <div style="
                    padding:24px 28px;
                    background:linear-gradient(
                        135deg,
                        #061f30,
                        #075044);
                    color:#ffffff;">

                    <div style="
                        font-size:19px;
                        font-weight:900;">
                        Rx&nbsp; PharmaCare
                    </div>

                    <div style="
                        margin-top:4px;
                        color:#56e5c8;
                        font-size:10px;
                        font-weight:900;
                        letter-spacing:3px;">
                        PRO
                    </div>
                </div>

                <div style="padding:30px 28px;">
                    <div style="
                        margin-bottom:8px;
                        color:#0d9f85;
                        font-size:10px;
                        font-weight:900;
                        letter-spacing:2px;">
                        SECURE ACCOUNT MESSAGE
                    </div>

                    <h1 style="
                        margin:0;
                        color:#142238;
                        font-size:27px;
                        line-height:1.25;">
                        {{heading}}
                    </h1>

                    <p style="
                        margin:14px 0 0;
                        color:#617083;
                        font-size:14px;
                        line-height:1.75;">
                        {{introduction}}
                    </p>

                    {{mainContent}}

                    <div style="
                        margin-top:28px;
                        padding-top:20px;
                        border-top:1px solid #e6edeb;
                        color:#7a8797;
                        font-size:12px;
                        line-height:1.7;">
                        {{footerText}}
                    </div>
                </div>

                <div style="
                    padding:16px 28px;
                    background:#f5f9f8;
                    color:#8895a3;
                    font-size:11px;
                    text-align:center;">
                    PharmaCare Pro · Secure pharmacy workspace
                </div>
            </div>
        </body>
        </html>
        """;
    }
}
