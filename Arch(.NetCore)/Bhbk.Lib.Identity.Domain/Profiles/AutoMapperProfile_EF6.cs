using AutoMapper;
using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Profiles
{
    public class AutoMapperProfile_EF6 : Profile
    {
        public AutoMapperProfile_EF6()
        {
            /*
             * activity models
             */

            CreateMap<AuthActivityV1, E_AuthActivity>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<E_AuthActivity, AuthActivityV1>();

            /*
             * audience models
             */

            CreateMap<AudienceV1, E_Audience>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore())
                .ForMember(dest => dest.AuthActivity, src => src.Ignore())
                .ForMember(dest => dest.AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.Refreshes, src => src.Ignore())
                .ForMember(dest => dest.Settings, src => src.Ignore())
                .ForMember(dest => dest.States, src => src.Ignore())
                .ForMember(dest => dest.Urls, src => src.Ignore());

            CreateMap<E_Audience, AudienceV1>()
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.Roles));

            /*
             * claim models
             */

            CreateMap<ClaimV1, E_Claim>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.UserClaims, src => src.Ignore());

            CreateMap<E_Claim, ClaimV1>();

            /*
             * email models
             */

            CreateMap<EmailV1, E_EmailQueue>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.SendAtUtc, src => src.MapFrom(val => val.SendAtUtc == default ? DateTime.UtcNow : val.SendAtUtc))
                .ForMember(dest => dest.EmailActivity, src => src.Ignore());

            CreateMap<E_EmailQueue, EmailV1>();

            /*
             * issuer models
             */

            CreateMap<IssuerV1, E_Issuer>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Claims, src => src.Ignore())
                .ForMember(dest => dest.Refreshes, src => src.Ignore())
                .ForMember(dest => dest.Settings, src => src.Ignore())
                .ForMember(dest => dest.States, src => src.Ignore());

            CreateMap<E_Issuer, IssuerV1>()
                .ForMember(dest => dest.Audiences, src => src.MapFrom(val => val.Audiences));

            /*
             * login models
             */

            CreateMap<LoginV1, E_Login>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.UserLogins, src => src.Ignore());

            CreateMap<E_Login, LoginV1>();

            /*
             * message of the day models
             */

            CreateMap<MOTDTssV1, E_MOTD>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.globalId == default ? Guid.NewGuid() : val.globalId))
                .ForMember(dest => dest.Author, src => src.MapFrom(val => val.author))
                .ForMember(dest => dest.Quote, src => src.MapFrom(val => val.quote))
                .ForMember(dest => dest.TssId, src => src.MapFrom(val => val.id))
                .ForMember(dest => dest.TssTitle, src => src.MapFrom(val => val.title))
                .ForMember(dest => dest.TssCategory, src => src.MapFrom(val => val.category))
                .ForMember(dest => dest.TssLength, src => src.MapFrom(val => val.length))
                .ForMember(dest => dest.TssDate, src => src.MapFrom(val => val.date))
                .ForMember(dest => dest.TssTags, src => src.MapFrom(val => string.Join(",", val.tags.Select(x => x))))
                .ForMember(dest => dest.TssBackground, src => src.MapFrom(val => val.background));

            CreateMap<E_MOTD, MOTDTssV1>()
                .ForMember(dest => dest.globalId, src => src.MapFrom(val => val.Id))
                .ForMember(dest => dest.author, src => src.MapFrom(val => val.Author))
                .ForMember(dest => dest.quote, src => src.MapFrom(val => val.Quote))
                .ForMember(dest => dest.id, src => src.MapFrom(val => val.TssId))
                .ForMember(dest => dest.title, src => src.MapFrom(val => val.TssTitle))
                .ForMember(dest => dest.category, src => src.MapFrom(val => val.TssCategory))
                .ForMember(dest => dest.length, src => src.MapFrom(val => val.TssLength))
                .ForMember(dest => dest.date, src => src.MapFrom(val => val.TssDate))
                .ForMember(dest => dest.tags, src => src.MapFrom(val => val.TssTags.Split(',', StringSplitOptions.None).ToList()))
                .ForMember(dest => dest.background, src => src.MapFrom(val => val.TssBackground));

            /*
             * refresh models
             */

            CreateMap<RefreshV1, E_Refresh>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<E_Refresh, RefreshV1>();

            /*
             * role models
             */

            CreateMap<RoleV1, E_Role>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.UserRoles, src => src.Ignore());

            CreateMap<E_Role, RoleV1>();

            /*
             * setting models
             */

            CreateMap<SettingV1, E_Setting>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            /*
             * state models
             */

            CreateMap<StateV1, E_State>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.IssuedUtc, src => src.MapFrom(val => val.IssuedUtc == default ? DateTime.UtcNow : val.IssuedUtc))
                .ForMember(dest => dest.LastPollingUtc, src => src.Ignore())
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            /*
             * text models
             */

            CreateMap<TextV1, E_TextQueue>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.SendAtUtc, src => src.MapFrom(val => val.SendAtUtc == default ? DateTime.UtcNow : val.SendAtUtc))
                .ForMember(dest => dest.TextActivity, src => src.Ignore());

            CreateMap<E_TextQueue, TextV1>();

            /*
             * url models
             */

            CreateMap<UrlV1, E_Url>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Audience, src => src.Ignore());

            CreateMap<E_Url, UrlV1>();

            /*
             * user models
             */

            CreateMap<UserV1, E_User>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.EmailAddress, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => val.ConcurrencyStamp == null ? Guid.NewGuid().ToString() : val.ConcurrencyStamp))
                .ForMember(dest => dest.SecurityStamp, src => src.MapFrom(val => val.SecurityStamp == null ? Guid.NewGuid().ToString() : val.SecurityStamp))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore())
                .ForMember(dest => dest.AuthActivity, src => src.Ignore())
                .ForMember(dest => dest.Refreshes, src => src.Ignore())
                .ForMember(dest => dest.Settings, src => src.Ignore())
                .ForMember(dest => dest.States, src => src.Ignore())
                .ForMember(dest => dest.UserClaims, src => src.Ignore())
                .ForMember(dest => dest.UserLogins, src => src.Ignore())
                .ForMember(dest => dest.UserRoles, src => src.Ignore());

            CreateMap<E_User, UserV1>()
                .ForMember(dest => dest.IssuerId, src => src.Ignore())
                .ForMember(dest => dest.Email, src => src.MapFrom(val => val.EmailAddress));
        }
    }
}
