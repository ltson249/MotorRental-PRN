namespace MotorRental.Api.Services;

public interface IEmailService
{
    Task<(bool Success, string Message)> SendOtpEmailAsync(string toEmail, string fullName, string otp);
}
