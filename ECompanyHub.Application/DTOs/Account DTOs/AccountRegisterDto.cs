using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECompanyHub.Application.DTOs
{
    public class AccountRegisterDto
    {
        public string arabicName { get; set; } 
        public string englishName { get; set; } 
        public string email { get; set; }        
        public string websiteUrl { get; set; }  
        public string? phone { get; set; }
        public IFormFile? companyLogo { get; set; }

    }
}
