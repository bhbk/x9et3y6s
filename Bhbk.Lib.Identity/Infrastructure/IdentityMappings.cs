using AutoMapper;
using Bhbk.Lib.Identity.Models;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class IdentityMappings : Profile
    {
        public IdentityMappings()
        {
            //activity models
            CreateMap<ActivityCreate, AppActivity>()
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
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));
            CreateMap<AppAudience, AudienceResult>();
            CreateMap<AudienceUpdate, AppAudience>();

            //audience uri models
            CreateMap<AudienceUriCreate, AppAudienceUri>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));
            CreateMap<AppAudienceUri, AudienceUriResult>();
            CreateMap<AudienceUriUpdate, AppAudienceUri>();

            //client models
            CreateMap<ClientCreate, AppClient>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));
            CreateMap<AppClient, ClientResult>();
            CreateMap<ClientUpdate, AppClient>();

            //login models
            CreateMap<LoginCreate, AppLogin>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()));
            CreateMap<AppLogin, LoginResult>();
            CreateMap<LoginUpdate, AppLogin>();

            //role models
            CreateMap<RoleCreate, AppRole>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));
            CreateMap<AppRole, RoleResult>();
            CreateMap<RoleUpdate, AppRole>();

            //user models
            CreateMap<UserCreate, AppUser>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.NormalizedUserName, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.Email, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.NormalizedEmail, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));
            CreateMap<AppUser, UserResult>();
            CreateMap<UserUpdate, AppUser>();

            //user claim models
            CreateMap<UserClaimCreate, AppUserClaim>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));
            CreateMap<AppUserClaim, UserClaimResult>();
            CreateMap<AppUserClaim, Claim>()
                .ForMember(dest => dest.Type, src => src.MapFrom(val => val.ClaimType))
                .ForMember(dest => dest.Value, src => src.MapFrom(val => val.ClaimValue))
                .ForMember(dest => dest.ValueType, src => src.MapFrom(val => val.ClaimValueType));
            CreateMap<UserClaimUpdate, AppUserClaim>();

            //user login models
            CreateMap<UserLoginCreate, AppUserLogin>()
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));
            CreateMap<AppUserLogin, UserLoginResult>();
            CreateMap<UserLoginUpdate, AppUserLogin>();

            //user refresh models
            CreateMap<UserRefreshCreate, AppUserRefresh>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()));
            CreateMap<AppUserRefresh, UserRefreshResult>();
        }
    }
}
