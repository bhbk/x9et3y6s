using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class IdentityMappings : Profile
    {
        public IdentityMappings()
        {
            //activity models
            CreateMap<ActivityCreate, AppActivity>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.KeyValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.KeyValues)))
                .ForMember(dest => dest.OriginalValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.OriginalValues)))
                .ForMember(dest => dest.CurrentValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.CurrentValues)))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));
            CreateMap<AppActivity, ActivityResult>();

            //audience models
            CreateMap<AudienceCreate, AppAudience>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.AppAudienceUri, src => src.Ignore())
                .ForMember(dest => dest.AppRole, src => src.Ignore());
            CreateMap<AppAudience, AudienceResult>()
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.AppRole
                    .ToDictionary(x => x.Id, x => x.Name)));
            CreateMap<AudienceUpdate, AppAudience>()
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.AppAudienceUri, src => src.Ignore())
                .ForMember(dest => dest.AppRole, src => src.Ignore());

            //audience uri models
            CreateMap<AudienceUriCreate, AppAudienceUri>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Audience, src => src.Ignore());
            CreateMap<AppAudienceUri, AudienceUriResult>();
            CreateMap<AudienceUriUpdate, AppAudienceUri>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Audience, src => src.Ignore());

            //client models
            CreateMap<ClientCreate, AppClient>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.AppAudience, src => src.Ignore())
                .ForMember(dest => dest.AppUserRefresh, src => src.Ignore());
            CreateMap<AppClient, ClientResult>()
                .ForMember(dest => dest.Audiences, src => src.MapFrom(val => val.AppAudience
                    .ToDictionary(x => x.Id, x => x.Name)));
            CreateMap<ClientUpdate, AppClient>()
                .ForMember(dest => dest.AppAudience, src => src.Ignore())
                .ForMember(dest => dest.AppUserRefresh, src => src.Ignore());

            //login models
            CreateMap<LoginCreate, AppLogin>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.AppUserLogin, src => src.Ignore());
            CreateMap<AppLogin, LoginResult>()
                .ForMember(dest => dest.Users, src => src.MapFrom(val => val.AppUserLogin.Where(x => x.LoginId == val.Id)
                    .ToDictionary(x => x.UserId, x => x.User.Email)));
            CreateMap<LoginUpdate, AppLogin>()
                .ForMember(dest => dest.AppUserLogin, src => src.Ignore());

            //role models
            CreateMap<RoleCreate, AppRole>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.NormalizedName, src => src.MapFrom(val => val.Name))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.AppRoleClaim, src => src.Ignore())
                .ForMember(dest => dest.AppUserRole, src => src.Ignore());
            CreateMap<AppRole, RoleResult>()
                .ForMember(dest => dest.Users, src => src.MapFrom(val => val.AppUserRole.Where(x => x.RoleId == val.Id)
                    .ToDictionary(x => x.UserId, x => x.User.Email)));
            CreateMap<RoleUpdate, AppRole>()
                .ForMember(dest => dest.NormalizedName, src => src.MapFrom(val => val.Name))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.AppRoleClaim, src => src.Ignore())
                .ForMember(dest => dest.AppUserRole, src => src.Ignore());

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
                .ForMember(dest => dest.AppAudienceUri, src => src.Ignore())
                .ForMember(dest => dest.AppUserClaim, src => src.Ignore())
                .ForMember(dest => dest.AppUserLogin, src => src.Ignore())
                .ForMember(dest => dest.AppUserRefresh, src => src.Ignore())
                .ForMember(dest => dest.AppUserRole, src => src.Ignore())
                .ForMember(dest => dest.AppUserToken, src => src.Ignore());
            CreateMap<AppUser, UserResult>()
                .ForMember(dest => dest.Logins, src => src.MapFrom(val => val.AppUserLogin.Where(x => x.UserId == val.Id)
                    .ToDictionary(x => x.LoginId, x => x.LoginProvider)))
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.AppUserRole.Where(x => x.UserId == val.Id)
                    .ToDictionary(x => x.RoleId, x => x.Role.Name)));
            CreateMap<UserUpdate, AppUser>()
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.NormalizedUserName, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.NormalizedEmail, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.PhoneNumberConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
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
                .ForMember(dest => dest.AppAudienceUri, src => src.Ignore())
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
            CreateMap<AppUserClaim, UserClaimResult>()
                .ForMember(dest => dest.Properties, src => src.Ignore());
            CreateMap<UserClaimUpdate, AppUserClaim>()
                .ForMember(dest => dest.User, src => src.Ignore());

            //user login models
            CreateMap<UserLoginCreate, AppUserLogin>()
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Login, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());
            CreateMap<AppUserLogin, UserLoginResult>();
            CreateMap<UserLoginUpdate, AppUserLogin>()
                .ForMember(dest => dest.Login, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            //user refresh models
            CreateMap<UserRefreshCreate, AppUserRefresh>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());
            CreateMap<AppUserRefresh, UserRefreshResult>();
        }
    }
}
