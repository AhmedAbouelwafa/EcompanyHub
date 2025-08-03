using ECompanyHub.Application.DTOs;
using ECompanyHub.Application.DTOs.Account_DTOs;
using ECompanyHub.Application.Wrappers.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ECompanyHub.Application.InterfaceService
{
    public interface IAuthService
    {
        public Task<ResponseHandler<LoginResponseDto>> RegisterAsync(AccountRegisterDto user);
        public Task<ResponseHandler<LoginResponseDto>> LoginAsync(AccountLoginDto loginDto);

    }
}
