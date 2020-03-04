using AutoMapper;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Infrastructure
{
    public class AutoMapperProfile_EF6_DIRECT : Profile
    {
        public AutoMapperProfile_EF6_DIRECT()
        {
            /*
             * activity models
             */
            CreateMap<ActivityV1, tbl_Activities>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_Users, src => src.Ignore())
                .ForMember(dest => dest.KeyValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.KeyValues)))
                .ForMember(dest => dest.OriginalValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.OriginalValues)))
                .ForMember(dest => dest.CurrentValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.CurrentValues)));

            CreateMap<tbl_Activities, ActivityV1>();

            /*
             * audience models
             */
            CreateMap<AudienceV1, tbl_Audiences>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.SecurityStamp, src => src.Ignore())
                .ForMember(dest => dest.tbl_Activities, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_Issuers, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Roles, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ForMember(dest => dest.tbl_Urls, src => src.Ignore());

            CreateMap<tbl_Audiences, AudienceV1>();

            /*
             * claim models
             */
            CreateMap<ClaimV1, tbl_Claims>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.tbl_Issuers, src => src.Ignore())
                .ForMember(dest => dest.tbl_Users, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserClaims, src => src.Ignore());

            CreateMap<tbl_Claims, ClaimV1>();

            /*
             * email models
             */
            CreateMap<EmailV1, tbl_QueueEmails>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.SendAt, src => src.MapFrom(val => (val.SendAt == default || val.SendAt == null) ? DateTime.Now : val.SendAt))
                .ForMember(dest => dest.tbl_Users, src => src.Ignore());

            CreateMap<tbl_QueueEmails, EmailV1>();

            /*
             * issuer models
             */
            CreateMap<IssuerV1, tbl_Issuers>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.tbl_Claims, src => src.Ignore())
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore());

            CreateMap<tbl_Issuers, IssuerV1>();

            /*
             * login models
             */
            CreateMap<LoginV1, tbl_Logins>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.LoginKey, src => src.Ignore())
                .ForMember(dest => dest.tbl_Users, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserLogins, src => src.Ignore());

            CreateMap<tbl_Logins, LoginV1>();

            /*
             * message of the day models
             */
            CreateMap<MOTDV1, tbl_MOTDs>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.id == default || val.id == null) ? Guid.NewGuid().ToString() : val.id))
                .ForMember(dest => dest.Title, src => src.MapFrom(val => val.title))
                .ForMember(dest => dest.Author, src => src.MapFrom(val => val.author))
                .ForMember(dest => dest.Quote, src => src.MapFrom(val => val.quote))
                .ForMember(dest => dest.Category, src => src.MapFrom(val => val.category))
                .ForMember(dest => dest.Date, src => src.MapFrom(val => val.date))
                .ForMember(dest => dest.Tags, src => src.MapFrom(val => string.Join(",", val.tags.Select(x => x))))
                .ForMember(dest => dest.Length, src => src.MapFrom(val => val.length))
                .ForMember(dest => dest.Background, src => src.MapFrom(val => val.background));

            CreateMap<tbl_MOTDs, MOTDV1>()
                .ForMember(dest => dest.id, src => src.MapFrom(val => val.Id))
                .ForMember(dest => dest.title, src => src.MapFrom(val => val.Title))
                .ForMember(dest => dest.author, src => src.MapFrom(val => val.Author))
                .ForMember(dest => dest.quote, src => src.MapFrom(val => val.Quote))
                .ForMember(dest => dest.category, src => src.MapFrom(val => val.Category))
                .ForMember(dest => dest.date, src => src.MapFrom(val => val.Date))
                .ForMember(dest => dest.tags, src => src.MapFrom(val => val.Tags.Split(',', StringSplitOptions.None).ToList()))
                .ForMember(dest => dest.length, src => src.MapFrom(val => val.Length))
                .ForMember(dest => dest.background, src => src.MapFrom(val => val.Background));

            /*
             * refresh models
             */
            CreateMap<RefreshV1, tbl_Refreshes>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_Issuers, src => src.Ignore())
                .ForMember(dest => dest.tbl_Users, src => src.Ignore());

            CreateMap<tbl_Refreshes, RefreshV1>();

            /*
             * role models
             */
            CreateMap<RoleV1, tbl_Roles>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore());

            CreateMap<tbl_Roles, RoleV1>();

            /*
             * setting models
             */
            CreateMap<SettingV1, tbl_Settings>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_Issuers, src => src.Ignore())
                .ForMember(dest => dest.tbl_Users, src => src.Ignore());

            /*
             * state models
             */
            CreateMap<StateV1, tbl_States>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.IssuedUtc, src => src.MapFrom(val => (val.IssuedUtc == default || val.IssuedUtc == null) ? DateTime.Now : val.IssuedUtc))
                .ForMember(dest => dest.LastPolling, src => src.Ignore())
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_Issuers, src => src.Ignore())
                .ForMember(dest => dest.tbl_Users, src => src.Ignore());

            /*
             * text models
             */
            CreateMap<TextV1, tbl_QueueTexts>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.SendAt, src => src.MapFrom(val => (val.SendAt == default || val.SendAt == null) ? DateTime.Now : val.SendAt))
                .ForMember(dest => dest.tbl_Users, src => src.Ignore());

            CreateMap<tbl_QueueTexts, TextV1>();

            /*
             * url models
             */
            CreateMap<UrlV1, tbl_Urls>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore());

            CreateMap<tbl_Urls, UrlV1>();

            /*
             * user models
             */
            CreateMap<UserV1, tbl_Users>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.AccessFailedCount, src => src.MapFrom(val => (val.AccessFailedCount == default) ? 0 : val.AccessFailedCount))
                .ForMember(dest => dest.AccessSuccessCount, src => src.MapFrom(val => (val.AccessSuccessCount == default) ? 0 : val.AccessSuccessCount))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.SecurityStamp, src => src.Ignore())
                .ForMember(dest => dest.tbl_Activities, src => src.Ignore())
                .ForMember(dest => dest.tbl_Claims, src => src.Ignore())
                .ForMember(dest => dest.tbl_Logins, src => src.Ignore())
                .ForMember(dest => dest.tbl_QueueEmails, src => src.Ignore())
                .ForMember(dest => dest.tbl_QueueTexts, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserLogins, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore());

            CreateMap<tbl_Users, UserV1>()
                .ForMember(dest => dest.IssuerId, src => src.Ignore());
        }
    }
}
