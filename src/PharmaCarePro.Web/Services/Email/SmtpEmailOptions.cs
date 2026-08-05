using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Web.Services.Email;

public sealed class SmtpEmailOptions
{
    public const string SectionName = "Email:Smtp";

    [Required]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    [Required]
    [EmailAddress]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string FromAddress { get; set; } = string.Empty;

    [Required]
    public string FromName { get; set; } = "PharmaCare Pro";

    public bool EnableSsl { get; set; } = true;
}
