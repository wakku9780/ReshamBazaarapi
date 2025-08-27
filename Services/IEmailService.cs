using System.Threading.Tasks;
using ReshamBazaar.Api.DTOs;

namespace ReshamBazaar.Api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailRequest emailRequest);
    }
}
