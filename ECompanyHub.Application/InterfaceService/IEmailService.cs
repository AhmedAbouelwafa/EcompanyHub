using ECompanyHub.Application.DTOs.Email_DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECompanyHub.Application.InterfaceService
{
    public interface IEmailService
    {
       public Task SendEmailAsync(EmailDTO emailDto);

    }
}
