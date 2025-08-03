using ECompanyHub.Application.DTOs;
using ECompanyHub.Application.DTOs.Account_DTOs;
using ECompanyHub.Application.DTOs.Email_DTO;
using ECompanyHub.Application.InterfaceService;
using ECompanyHub.Application.Wrappers.Handlers;
using ECompanyHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sweets.Application.Services
{
    public class IdentityAuthService  : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        private readonly IEmailService _emailService;

        public IdentityAuthService(UserManager<ApplicationUser> userManager, IConfiguration config , IEmailService emailService)
        {
            _userManager = userManager;
            _config = config;
            _emailService = emailService;
        }

        public async Task<ResponseHandler<LoginResponseDto>> RegisterAsync(AccountRegisterDto user)
        {
            var acc = new ApplicationUser
            {
                UserName = user.email, // Assuming email is used as username
                Email = user.email,
                arabicName = user.arabicName,
                englishName = user.englishName,
                websiteUrl = user.websiteUrl,
                PhoneNumber = user.phone,
                //companyLogo = user.companyLogo != null ? $"images/logos/{user.companyLogo.FileName}" : null
            };

            if (user != null)
            {
                List<Claim> claims = new List<Claim>
                {
                    // token generated id 
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                    new Claim(ClaimTypes.NameIdentifier, acc.Id),
                    new Claim(ClaimTypes.Name, acc.UserName),
                };

                var UserRoles = await _userManager.GetRolesAsync(acc);
                foreach (var roleName in UserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }

                //generate token
                JwtSecurityToken mytoken = new JwtSecurityToken(
                    issuer: _config["JWT:IssuerIp"],
                    audience: _config["JWT:AudienceIP"],
                    expires: DateTime.UtcNow.AddHours(1),
                    claims: claims,
                    signingCredentials:
                    new SigningCredentials(
                        new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(_config["JWT:SecretKey"])),
                        SecurityAlgorithms.HmacSha256
                    )

                );

                var token = new JwtSecurityTokenHandler().WriteToken(mytoken);
                var loginResponseDto = new LoginResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                    Expiration = DateTime.UtcNow.AddHours(1),
                    message = "Login successful"
                };

                Console.WriteLine("Trying to send email...");
                var emailBody = $"<h3>Welcome {user.englishName}</h3><p>Your token is: <b>{token}</b></p>";
                await _emailService.SendEmailAsync(new EmailDTO
                {
                    To = user.email,
                    Subject = "Welcome to ECompanyHub",
                    Body = emailBody
                });
            
                Console.WriteLine("Email sent!");

                return ResponseHandler<LoginResponseDto>.SuccessResponse(loginResponseDto);
            }
            return ResponseHandler<LoginResponseDto>.FailureResponse("Invalid username or password");

        }

        public async Task<ResponseHandler<LoginResponseDto>> LoginAsync(AccountLoginDto loginDto)
        {

            ApplicationUser user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user != null)
            {     
                List<Claim> claims = new List<Claim>
                {
                    // token generated id 
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                };

                var UserRoles = await _userManager.GetRolesAsync(user);
                foreach (var roleName in UserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }

                //generate token
                JwtSecurityToken mytoken = new JwtSecurityToken(
                    issuer: _config["JWT:IssuerIp"],
                    audience: _config["JWT:AudienceIP"],
                    expires: DateTime.UtcNow.AddHours(1),
                    claims: claims,
                    signingCredentials:
                    new SigningCredentials(
                        new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(_config["JWT:SecretKey"])),
                        SecurityAlgorithms.HmacSha256
                    )

                );


                var loginResponseDto = new LoginResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                    Expiration = DateTime.UtcNow.AddHours(1),
                    message = "Login successful"
                };


                return ResponseHandler<LoginResponseDto>.SuccessResponse(loginResponseDto);
            }
            return ResponseHandler<LoginResponseDto>.FailureResponse("Invalid username or password");

        }
      
    
        
    
    }
}