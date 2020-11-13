using AutoMapper;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Infrastructure
{
    public class AutoMapperProfile_EFCore_DIRECT : Profile
    {
        public AutoMapperProfile_EFCore_DIRECT()
        {
            /*
             * activity models
             */
            CreateMap<ActivityV1, tbl_Activity>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore())
                .ForMember(dest => dest.KeyValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.KeyValues)))
                .ForMember(dest => dest.OriginalValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.OriginalValues)))
                .ForMember(dest => dest.CurrentValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.CurrentValues)));

            CreateMap<tbl_Activity, ActivityV1>();

            /*
             * audience models
             */
            CreateMap<AudienceV1, tbl_Audience>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.tbl_Activities, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_Roles, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ForMember(dest => dest.tbl_Urls, src => src.Ignore());

            CreateMap<tbl_Audience, AudienceV1>();

            /*
             * claim models
             */
            CreateMap<ClaimV1, tbl_Claim>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserClaims, src => src.Ignore());

            CreateMap<tbl_Claim, ClaimV1>();

            /*
             * email models
             */
            CreateMap<EmailV1, tbl_EmailQueue>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.SendAtUtc, src => src.MapFrom(val => val.SendAtUtc == default ? DateTime.UtcNow : val.SendAtUtc))
                .ForMember(dest => dest.From, src => src.Ignore());

            CreateMap<tbl_EmailQueue, EmailV1>();

            /*
             * issuer models
             */
            CreateMap<IssuerV1, tbl_Issuer>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.tbl_Claims, src => src.Ignore())
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore());

            CreateMap<tbl_Issuer, IssuerV1>();

            /*
             * login models
             */
            CreateMap<LoginV1, tbl_Login>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserLogins, src => src.Ignore());

            CreateMap<tbl_Login, LoginV1>();

            /*
             * message of the day models
             */
            CreateMap<MOTDTssV1, tbl_MOTD>()
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

            CreateMap<tbl_MOTD, MOTDTssV1>()
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
            CreateMap<RefreshV1, tbl_Refresh>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<tbl_Refresh, RefreshV1>();

            /*
             * role models
             */
            CreateMap<RoleV1, tbl_Role>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore());

            CreateMap<tbl_Role, RoleV1>();

            /*
             * setting models
             */
            CreateMap<SettingV1, tbl_Setting>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            /*
             * state models
             */
            CreateMap<StateV1, tbl_State>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.IssuedUtc, src => src.MapFrom(val => val.IssuedUtc == default ? DateTime.UtcNow : val.IssuedUtc))
                .ForMember(dest => dest.LastPollingUtc, src => src.Ignore())
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            /*
             * text models
             */
            CreateMap<TextV1, tbl_TextQueue>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.SendAtUtc, src => src.MapFrom(val => val.SendAtUtc == default ? DateTime.UtcNow : val.SendAtUtc))
                .ForMember(dest => dest.From, src => src.Ignore());

            CreateMap<tbl_TextQueue, TextV1>();

            /*
             * url models
             */
            CreateMap<UrlV1, tbl_Url>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.Audience, src => src.Ignore());

            CreateMap<tbl_Url, UrlV1>();

            /*
             * user models
             */
            CreateMap<UserV1, tbl_User>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.UserName))
                .ForMember(dest => dest.EmailAddress, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.AccessFailedCount, src => src.MapFrom(val => (val.AccessFailedCount == default) ? 0 : val.AccessFailedCount))
                .ForMember(dest => dest.AccessSuccessCount, src => src.MapFrom(val => (val.AccessSuccessCount == default) ? 0 : val.AccessSuccessCount))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => val.ConcurrencyStamp == null ? Guid.NewGuid().ToString() : val.ConcurrencyStamp))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore())
                .ForMember(dest => dest.SecurityStamp, src => src.MapFrom(val => val.SecurityStamp == null ? Guid.NewGuid().ToString() : val.SecurityStamp))
                .ForMember(dest => dest.tbl_Activities, src => src.Ignore())
                .ForMember(dest => dest.tbl_Claims, src => src.Ignore())
                .ForMember(dest => dest.tbl_Logins, src => src.Ignore())
                .ForMember(dest => dest.tbl_EmailQueues, src => src.Ignore())
                .ForMember(dest => dest.tbl_TextQueues, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserLogins, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore());

            CreateMap<tbl_User, UserV1>()
                .ForMember(dest => dest.IssuerId, src => src.Ignore())
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.UserName))
                .ForMember(dest => dest.Email, src => src.MapFrom(val => val.EmailAddress))
                .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(val => val.EmailConfirmed));
        }
    }
}
