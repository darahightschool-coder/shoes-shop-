namespace ShoeShop.Services;

public class MockEmailSender : IEmailSender
{
    private readonly ILogger<MockEmailSender> _logger;

    public MockEmailSender(ILogger<MockEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("================== MOCK EMAIL SENDER ==================");
        _logger.LogInformation("To: {Email}", email);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Body: {Body}", htmlMessage);
        _logger.LogInformation("=======================================================");

        return Task.CompletedTask;
    }
}
