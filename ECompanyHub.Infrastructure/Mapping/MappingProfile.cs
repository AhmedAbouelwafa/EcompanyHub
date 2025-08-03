using AutoMapper;
using ECompanyHub.Application.DTOs;
using ECompanyHub.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECompanyHub.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AccountRegisterDto, ApplicationUser>()
                .ForMember(dest => dest.arabicName, opt => opt.MapFrom(src => src.arabicName))
                .ForMember(dest => dest.englishName, opt => opt.MapFrom(src => src.englishName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.websiteUrl, opt => opt.MapFrom(src => src.websiteUrl))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.phone));
                //.ForMember(dest => dest.companyLogo, opt => opt.MapFrom(src => src.companyLogo));


        }




    }
}
