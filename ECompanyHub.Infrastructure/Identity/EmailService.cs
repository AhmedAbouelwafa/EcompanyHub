using ECompanyHub.Application.DTOs.Email_DTO;
using ECompanyHub.Application.InterfaceService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ECompanyHub.Infrastructure.Identity
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(EmailDTO emailDto)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com") // لو Gmail
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Email"],
                    _configuration["EmailSettings:Password"]
                ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Email"]),
                Subject = emailDto.Subject,
                Body = emailDto.Body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(emailDto.To);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }

}
