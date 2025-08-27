namespace ReshamBazaar.Api.DTOs;

public class EmailSettings
{
    public string? SmtpServer { get; set; }
    public int Port { get; set; }
    public string? SenderEmail { get; set; }
    public string? SenderPassword { get; set; }
}

public record EmailRequest(
    string ToEmail,
    string Subject,
    string Body,
    bool IsBodyHtml = false
);

public record EmailResponse(
    bool Success,
    string? Message = null
);
