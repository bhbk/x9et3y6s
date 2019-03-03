using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class IdentityMaps : Profile
    {
        public IdentityMaps()
        {
            //activity models
            CreateMap<ActivityCreate, AppActivity>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.KeyValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.KeyValues)))
                .ForMember(dest => dest.OriginalValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.OriginalValues)))
                .ForMember(dest => dest.CurrentValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.CurrentValues)))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));

            CreateMap<ActivityModel, AppActivity>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ReverseMap();

            //client models
            CreateMap<ClientCreate, AppClient>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.AppClientUri, src => src.Ignore())
                .ForMember(dest => dest.AppRole, src => src.Ignore());

            CreateMap<ClientModel, AppClient>()
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.AppClientUri, src => src.Ignore())
                .ForMember(dest => dest.AppRole, src => src.Ignore());

            CreateMap<AppClient, ClientModel>()
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.AppRole
                    .ToDictionary(x => x.Id, x => x.Name)));

            //client uri models
            CreateMap<ClientUriCreate, AppClientUri>()
                    .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                    .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                    .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                    .ForMember(dest => dest.Actor, src => src.Ignore())
                    .ForMember(dest => dest.Client, src => src.Ignore());

            CreateMap<AppClientUri, ClientUriModel>();

            //issuer models
            CreateMap<IssuerCreate, AppIssuer>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.AppClient, src => src.Ignore())
                .ForMember(dest => dest.AppUserRefresh, src => src.Ignore());

            CreateMap<AppIssuer, IssuerModel>()
                .ForMember(dest => dest.Clients, src => src.MapFrom(val => val.AppClient
                    .ToDictionary(x => x.Id, x => x.Name)));

            //login models
            CreateMap<LoginCreate, AppLogin>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.AppUserLogin, src => src.Ignore());

            CreateMap<AppLogin, LoginModel>()
                .ForMember(dest => dest.Users, src => src.MapFrom(val => val.AppUserLogin.Where(x => x.LoginId == val.Id)
                    .ToDictionary(x => x.User.Id, x => x.User.Email)));

            //role models
            CreateMap<RoleCreate, AppRole>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.NormalizedName, src => src.MapFrom(val => val.Name))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.AppRoleClaim, src => src.Ignore())
                .ForMember(dest => dest.AppUserRole, src => src.Ignore());

            CreateMap<AppRole, RoleModel>()
                .ForMember(dest => dest.Users, src => src.MapFrom(val => val.AppUserRole.Where(x => x.RoleId == val.Id)
                    .ToDictionary(x => x.User.Id, x => x.User.Email)));

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

            CreateMap<UserModel, AppUser>()
                .ForMember(dest => dest.UserName, src => src.Ignore())
                .ForMember(dest => dest.NormalizedUserName, src => src.Ignore())
                .ForMember(dest => dest.NormalizedEmail, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.Ignore())
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
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

            //user login models
            CreateMap<UserLoginCreate, AppUserLogin>()
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Login, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<AppUserLogin, UserLoginModel>();

            //user refresh models
            CreateMap<UserRefreshCreate, AppUserRefresh>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<AppUserRefresh, UserRefreshModel>();
        }
    }
}
