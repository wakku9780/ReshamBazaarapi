using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Services;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/test")]
[AllowAnonymous] // For testing only - remove in production
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TestEmailController> _logger;

    public TestEmailController(IEmailService emailService, ILogger<TestEmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("send-email")]
    public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Sending test email to {Email}", request.ToEmail);
            
            var emailRequest = new EmailRequest(
                ToEmail: request.ToEmail,
                Subject: "Test Email from ReshamBazaar",
                Body: "<h1>This is a test email</h1><p>If you're seeing this, email sending is working!</p>",
                IsBodyHtml: true
            );

            await _emailService.SendEmailAsync(emailRequest);
            _logger.LogInformation("Test email sent successfully to {Email}", request.ToEmail);
            
            return Ok(new { Success = true, Message = "Test email sent successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email to {Email}", request.ToEmail);
            return StatusCode(500, new { Success = false, Message = $"Failed to send test email: {ex.Message}" });
        }
    }
}

public class TestEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
}
