using AutoMapper;
using ECompanyHub.Application.DTOs;
using ECompanyHub.Application.DTOs.Account_DTOs;
using ECompanyHub.Application.DTOs.Email_DTO;
using ECompanyHub.Application.InterfaceService;
using ECompanyHub.Application.Wrappers.Handlers;
using ECompanyHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sweets.Application.Services
{
    public class IdentityAuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityAuthService(UserManager<ApplicationUser> userManager, IConfiguration config, IEmailService emailService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _config = config;
            _emailService = emailService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        //MinifyToken
        public string MinifyToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be null, empty, or whitespace", nameof(token));
            }


            token = token.Trim();

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hashString.Substring(0, 8);
            }
        }

        public async Task<ResponseHandler<LoginResponseDto>> RegisterAsync(AccountRegisterDto user, string? logoPath)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.email);
            if (existingUser != null)
            {
                return ResponseHandler<LoginResponseDto>.FailureResponse("Email is already registered.");
            }

            var acc = _mapper.Map<ApplicationUser>(user);
            acc.companyLogo = logoPath; // 👈 إدخال مسار الصورة

            var result = await _userManager.CreateAsync(acc, "TemporaryPass@123");
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ResponseHandler<LoginResponseDto>.FailureResponse(string.Join(", ", errors));
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, acc.Id),
                new Claim(ClaimTypes.Name, acc.UserName),
            };

            var token = CreateToken(claims, out DateTime expiration);
            //token = MinifyToken(token);

            var emailBody = $"<h3>Welcome {user.englishName}</h3><p>Your token is: <b>{token}</b></p>";
            await _emailService.SendEmailAsync(new EmailDTO
            {
                To = user.email,
                Subject = "Welcome to ECompanyHub",
                Body = emailBody
            });

            var loginResponseDto = new LoginResponseDto
            {
                Token = token,
                Expiration = expiration,
                message = "Registration successful. Please set your password."
            };

            return ResponseHandler<LoginResponseDto>.SuccessResponse(loginResponseDto);
        }


        private string CreateToken(List<Claim> claims, out DateTime expiration)
        {
            expiration = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:IssuerIp"],
                audience: _config["JWT:AudienceIP"],
                expires: expiration,
                claims: claims,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"])),
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<ResponseHandler<bool>> SetPasswordAsync(SetPasswordDto setPasswordDto)
        {

            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            if (user == null)
            {
                return ResponseHandler<bool>.FailureResponse("User not found or unauthorized.");
            }


            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                return ResponseHandler<bool>.FailureResponse("Password is already set.");
            }

            // نحط الباسورد
            var result = await _userManager.AddPasswordAsync(user, setPasswordDto.Password);

            if (result.Succeeded)
            {
                return ResponseHandler<bool>.SuccessResponse(true);
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ResponseHandler<bool>.FailureResponse(string.Join(", ", errors));
            }
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