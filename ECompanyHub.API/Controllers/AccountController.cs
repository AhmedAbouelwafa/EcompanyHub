using AutoMapper;
using ECompanyHub.Application.DTOs;
using ECompanyHub.Application.DTOs.Account_DTOs;
using ECompanyHub.Application.InterfaceService;
using ECompanyHub.Application.Wrappers.Handlers;
using ECompanyHub.Infrastructure.Identity;
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
            if (string.IsNullOrWhiteSpace(accountRegister.email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(accountRegister.websiteUrl))
                return BadRequest("Website URL is required");

            if (string.IsNullOrWhiteSpace(accountRegister.englishName))
                return BadRequest("English name is required");

            if (string.IsNullOrWhiteSpace(accountRegister.arabicName))
                return BadRequest("Arabic name is required");

            if (!string.IsNullOrWhiteSpace(accountRegister.phone))
            {
                if (!accountRegister.phone.All(char.IsDigit) || accountRegister.phone.Length != 11)
                    return BadRequest("Invalid phone number format. Must be 11 digits.");
            }

            // تحقق من صحة البريد الإلكتروني
            if (!new EmailAddressAttribute().IsValid(accountRegister.email))
                return BadRequest("Invalid email format");

            // تحقق من صحة URL
            if (!Uri.IsWellFormedUriString(accountRegister.websiteUrl, UriKind.Absolute))
                return BadRequest("Invalid website URL format");

            // تحقق من صحة الاسم الإنجليزي (مسموح به فقط الأحرف الإنجليزية)
            if (!System.Text.RegularExpressions.Regex.IsMatch(accountRegister.englishName, "^[a-zA-Z ]+$"))
                return BadRequest("English name must contain only English letters");

            // تحقق من صحة الاسم العربي (مسموح به فقط الأحرف العربية)
            if (!System.Text.RegularExpressions.Regex.IsMatch(accountRegister.arabicName, "^[\u0621-\u064A ]+$"))
                return BadRequest("Arabic name must contain only Arabic letters");

            var user = _mapper.Map<ApplicationUser>(accountRegister);
            user.UserName = accountRegister.email;

            var result = await _userManager.CreateAsync(user, "TemporaryPass@123");

            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(ResponseHandler<string>.FailureResponse(identityErrors));
            }

            // ✅ رفع الصورة لو موجودة
            if (accountRegister.companyLogo != null && accountRegister.companyLogo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logos");
                Directory.CreateDirectory(uploadsFolder); // لو مش موجود

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(accountRegister.companyLogo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await accountRegister.companyLogo.CopyToAsync(stream);
                }

                user.companyLogo = $"images/logos/{uniqueFileName}";
                await _userManager.UpdateAsync(user); // تخزين المسار في الداتابيز
            }

            var resultt = await _authService.RegisterAsync(accountRegister);
            return Ok(resultt);
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
