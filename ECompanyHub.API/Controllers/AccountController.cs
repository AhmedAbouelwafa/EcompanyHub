using AutoMapper;
using ECompanyHub.Application.DTOs;
using ECompanyHub.Application.DTOs.Account_DTOs;
using ECompanyHub.Application.InterfaceService;
using ECompanyHub.Application.Wrappers.Handlers;
using ECompanyHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ECompanyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;


        public AccountController(UserManager<ApplicationUser> userManager, IMapper mapper, IAuthService authService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _authService = authService;
        }

        [HttpPost("Register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] AccountRegisterDto accountRegister)
        {
            // ✅ Validations
            if (string.IsNullOrWhiteSpace(accountRegister.email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(accountRegister.websiteUrl))
                return BadRequest("Website URL is required");

            if (string.IsNullOrWhiteSpace(accountRegister.englishName))
                return BadRequest("English name is required");

            if (string.IsNullOrWhiteSpace(accountRegister.arabicName))
                return BadRequest("Arabic name is required");

            if (!string.IsNullOrWhiteSpace(accountRegister.phone) && accountRegister.phone.Length < 11)
                return BadRequest("Invalid phone number format. Must be 11 digits.");

            if (!new EmailAddressAttribute().IsValid(accountRegister.email))
                return BadRequest("Invalid email format");

            if (!Uri.IsWellFormedUriString(accountRegister.websiteUrl, UriKind.Absolute))
                return BadRequest("Invalid website URL format");

            if (!System.Text.RegularExpressions.Regex.IsMatch(accountRegister.englishName, "^[a-zA-Z ]+$"))
                return BadRequest("English name must contain only English letters");

            if (!System.Text.RegularExpressions.Regex.IsMatch(accountRegister.arabicName, "^[\u0621-\u064A ]+$"))
                return BadRequest("Arabic name must contain only Arabic letters");

            // ✅ رفع الصورة إن وجدت
            string? savedLogoPath = null;
            if (accountRegister.companyLogo != null && accountRegister.companyLogo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logos");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(accountRegister.companyLogo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await accountRegister.companyLogo.CopyToAsync(stream);
                }

                savedLogoPath = $"images/logos/{uniqueFileName}";
            }

            // ✅ نمرر مسار الصورة يدويًا للـ DTO
            var result = await _authService.RegisterAsync(accountRegister, savedLogoPath);
            return Ok(result);
        }


        [Authorize] // لازم يكون Authorized علشان نقدر نقرأ التوكن
        [HttpPost("setpass")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
        {
            var result = await _authService.SetPasswordAsync(dto);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(AccountLoginDto accountLogin)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                   .SelectMany(v => v.Errors)
                   .Select(e => e.ErrorMessage)
                   .ToArray();

                return BadRequest(ResponseHandler<LoginResponseDto>.FailureResponse(errors));

            }

            var result = await _authService.LoginAsync(accountLogin);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);

        }


        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users == null)
            {
                return NotFound();
            }
            return Ok(users);
        }


    }
}
