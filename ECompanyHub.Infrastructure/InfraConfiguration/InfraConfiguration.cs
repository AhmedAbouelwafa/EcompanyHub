
using ECompanyHub.Application.InterfaceService;
using ECompanyHub.Infrastructure.Context;
using ECompanyHub.Infrastructure.Identity;
using ECompanyHub.Infrastructure.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sweets.Application.Services;
using System.Text;

namespace ECompanyHub.Infrastructure.InfraConfiguration
{
    public static class InfraConfiguration
    {
        public static IServiceCollection InfrastructureDependencyConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();

            });
            services.AddScoped<IAuthService, IdentityAuthService>();

            //services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            //services.AddScoped<ICategoryRepository, CategoryRepository>();
            //services.AddScoped<IUnitOfWork, UnitOfWork>();




            //builder.Services.AddSwaggerGen();
            services.AddAuthentication(option => // لما بنعمل add لاى حاجه معناه انها موجوده اساس  تحت زى ال useauthentication بس هيا مش موجوده عشان دا ال default
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //[authorize]
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

                /*
                 *  ✅ لو هتتأكد من الشخص (Authentication)، بص في الـ Authorization Header لو فيه Bearer <token> وحاول تفكه.

                    ✅ لو الشخص مش متحقق (Unauthorized)، ابعته رد مناسب (401 Unauthorized) وقوله لازم تبعت Bearer Token.

                    ✅ لو مفيش سكيم محدد، برده اعتبر نفسك شغال بـ Bearer.
                 */

            }).AddJwtBearer(options =>
            {

                options.SaveToken = true; // عشان نحفظ التوكن في ال HttpContext 
                options.RequireHttpsMetadata = false; // لو مش شغال على https ممكن تحطها false
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // عشان اتأكد ان انا اللى عامل التوكن عشان لو true هيحتاج تحقق من ال issuer
                    ValidIssuer = configuration["JWT:IssuerIp"], // لو عايز تتحقق من ال issuer حطها هنا
                    ValidAudience = configuration["JWT:AudienceIP"], // لو عايز تتحقق من ال audience حطها هنا


                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false, // لو عايز تتحقق من ال audience حطها true
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])),

                };

            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();


            services.AddScoped<IEmailService, EmailService>();

            return services;
        }

    }
}
