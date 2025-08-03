using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECompanyHub.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(100, ErrorMessage = "Arabic name can't be longer than 100 characters")]
        public string arabicName { get; set; }

        [Required(ErrorMessage = "English name is required")]
        [StringLength(100, ErrorMessage = "English name can't be longer than 100 characters")]
        public string englishName { get; set; }


        [Required(ErrorMessage = "Website URL is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string websiteUrl { get; set; }


        public string? companyLogo { get; set; }
    }
}
