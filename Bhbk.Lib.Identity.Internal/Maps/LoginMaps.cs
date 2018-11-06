using AutoMapper;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.EntityModels;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Maps
{
    public class LoginMaps : Profile
    {
        public LoginMaps()
        {
            //login models
            CreateMap<LoginCreate, AppLogin>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.AppUserLogin, src => src.Ignore());

            CreateMap<AppLogin, LoginModel>()
                .ForMember(dest => dest.Users, src => src.MapFrom(val => val.AppUserLogin.Where(x => x.LoginId == val.Id)
                    .ToDictionary(x => x.User.Id, x => x.User.Email)));

            CreateMap<LoginUpdate, AppLogin>()
                .ForMember(dest => dest.AppUserLogin, src => src.Ignore());
        }
    }
}
