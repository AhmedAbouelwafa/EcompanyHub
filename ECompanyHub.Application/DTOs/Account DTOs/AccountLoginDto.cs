using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECompanyHub.Application.DTOs
{
    public class AccountLoginDto
    {
        //data annotations can be added here for validation if needed
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        
        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }
    }
}
