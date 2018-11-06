using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.EntityModels;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Maps
{
    public class UserMaps : Profile
    {
        public UserMaps()
        {
            //user models
            CreateMap<UserCreate, AppUser>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.NormalizedUserName, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.Email, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.NormalizedEmail, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.LockoutEnd, src => src.Ignore())
                .ForMember(dest => dest.LastLoginFailure, src => src.Ignore())
                .ForMember(dest => dest.LastLoginSuccess, src => src.Ignore())
                .ForMember(dest => dest.AccessFailedCount, src => src.MapFrom(val => 0))
                .ForMember(dest => dest.AccessSuccessCount, src => src.MapFrom(val => 0))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.PasswordConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.SecurityStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.TwoFactorEnabled, src => src.MapFrom(val => false))
                .ForMember(dest => dest.AppActivity, src => src.Ignore())
                .ForMember(dest => dest.AppClientUri, src => src.Ignore())
                .ForMember(dest => dest.AppUserClaim, src => src.Ignore())
                .ForMember(dest => dest.AppUserLogin, src => src.Ignore())
                .ForMember(dest => dest.AppUserRefresh, src => src.Ignore())
                .ForMember(dest => dest.AppUserRole, src => src.Ignore())
                .ForMember(dest => dest.AppUserToken, src => src.Ignore());

            CreateMap<AppUser, UserModel>()
                .ForMember(dest => dest.Logins, src => src.MapFrom(val => val.AppUserLogin.Where(x => x.UserId == val.Id)
                    .ToDictionary(x => x.LoginId, x => x.LoginProvider)))
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.AppUserRole.Where(x => x.UserId == val.Id)
                    .ToDictionary(x => x.Role.Id, x => x.Role.Name)));

            CreateMap<UserUpdate, AppUser>()
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.EmailConfirmed, src => src.Ignore())
                .ForMember(dest => dest.NormalizedUserName, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.NormalizedEmail, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.PhoneNumberConfirmed, src => src.Ignore())
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.LockoutEnd, src => src.Ignore())
                .ForMember(dest => dest.LastLoginFailure, src => src.Ignore())
                .ForMember(dest => dest.LastLoginSuccess, src => src.Ignore())
                .ForMember(dest => dest.AccessFailedCount, src => src.Ignore())
                .ForMember(dest => dest.AccessSuccessCount, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.PasswordConfirmed, src => src.Ignore())
                .ForMember(dest => dest.SecurityStamp, src => src.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, src => src.Ignore())
                .ForMember(dest => dest.AppActivity, src => src.Ignore())
                .ForMember(dest => dest.AppClientUri, src => src.Ignore())
                .ForMember(dest => dest.AppUserClaim, src => src.Ignore())
                .ForMember(dest => dest.AppUserLogin, src => src.Ignore())
                .ForMember(dest => dest.AppUserRefresh, src => src.Ignore())
                .ForMember(dest => dest.AppUserRole, src => src.Ignore())
                .ForMember(dest => dest.AppUserToken, src => src.Ignore());

            //user claim models
            CreateMap<UserClaimCreate, AppUserClaim>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<AppUserClaim, UserClaimModel>()
                .ForMember(dest => dest.Properties, src => src.Ignore());

            CreateMap<UserClaimUpdate, AppUserClaim>()
                .ForMember(dest => dest.User, src => src.Ignore());

            //user login models
            CreateMap<UserLoginCreate, AppUserLogin>()
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Login, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<AppUserLogin, UserLoginModel>();

            CreateMap<UserLoginUpdate, AppUserLogin>()
                .ForMember(dest => dest.Login, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            //user refresh models
            CreateMap<UserRefreshCreate, AppUserRefresh>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<AppUserRefresh, UserRefreshModel>();
        }
    }
}