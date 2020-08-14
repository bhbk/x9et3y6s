using AutoMapper;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Infrastructure
{
    public class AutoMapperProfile_EFCore : Profile
    {
        public AutoMapperProfile_EFCore()
        {
            /*
             * activity models
             */
            CreateMap<ActivityV1, uvw_Activity>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.KeyValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.KeyValues)))
                .ForMember(dest => dest.OriginalValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.OriginalValues)))
                .ForMember(dest => dest.CurrentValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.CurrentValues)));

            CreateMap<uvw_Activity, ActivityV1>();

            /*
             * audience models
             */
            CreateMap<AudienceV1, uvw_Audience>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore());

            CreateMap<uvw_Audience, AudienceV1>();

            /*
             * claim models
             */
            CreateMap<ClaimV1, uvw_Claim>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created));

            CreateMap<uvw_Claim, ClaimV1>();

            /*
             * email models
             */
            CreateMap<EmailV1, uvw_QueueEmail>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.SendAt, src => src.MapFrom(val => (val.SendAt == default || val.SendAt == null) ? DateTime.Now : val.SendAt));

            CreateMap<uvw_QueueEmail, EmailV1>();

            /*
             * issuer models
             */
            CreateMap<IssuerV1, uvw_Issuer>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created));

            CreateMap<uvw_Issuer, IssuerV1>();

            /*
             * login models
             */
            CreateMap<LoginV1, uvw_Login>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.LoginKey, src => src.Ignore());

            CreateMap<uvw_Login, LoginV1>();

            /*
             * message of the day models
             */
            CreateMap<MOTDTssV1, uvw_MOTD>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.globalId == default || val.globalId == null) ? Guid.NewGuid() : val.globalId))
                .ForMember(dest => dest.Author, src => src.MapFrom(val => val.author))
                .ForMember(dest => dest.Quote, src => src.MapFrom(val => val.quote))
                .ForMember(dest => dest.TssId, src => src.MapFrom(val => val.id))
                .ForMember(dest => dest.TssTitle, src => src.MapFrom(val => val.title))
                .ForMember(dest => dest.TssCategory, src => src.MapFrom(val => val.category))
                .ForMember(dest => dest.TssLength, src => src.MapFrom(val => val.length))
                .ForMember(dest => dest.TssDate, src => src.MapFrom(val => val.date))
                .ForMember(dest => dest.TssTags, src => src.MapFrom(val => string.Join(",", val.tags.Select(x => x))))
                .ForMember(dest => dest.TssBackground, src => src.MapFrom(val => val.background));

            CreateMap<uvw_MOTD, MOTDTssV1>()
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
            CreateMap<RefreshV1, uvw_Refresh>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id));

            CreateMap<uvw_Refresh, RefreshV1>();

            /*
             * role models
             */
            CreateMap<RoleV1, uvw_Role>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created));

            CreateMap<uvw_Role, RoleV1>();

            /*
             * setting models
             */
            CreateMap<SettingV1, uvw_Setting>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created));

            /*
             * state models
             */
            CreateMap<StateV1, uvw_State>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.IssuedUtc, src => src.MapFrom(val => (val.IssuedUtc == default || val.IssuedUtc == null) ? DateTime.Now : val.IssuedUtc))
                .ForMember(dest => dest.LastPolling, src => src.Ignore());

            /*
             * text models
             */
            CreateMap<TextV1, uvw_QueueText>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.SendAt, src => src.MapFrom(val => (val.SendAt == default || val.SendAt == null) ? DateTime.Now : val.SendAt));

            CreateMap<uvw_QueueText, TextV1>();

            /*
             * url models
             */
            CreateMap<UrlV1, uvw_Url>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created));

            CreateMap<uvw_Url, UrlV1>();

            /*
             * user models
             */
            CreateMap<UserV1, uvw_User>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => (val.Id == default || val.Id == null) ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.UserName))
                .ForMember(dest => dest.EmailAddress, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(val => val.EmailConfirmed))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => (val.Created == default || val.Created == null) ? DateTime.Now : val.Created))
                .ForMember(dest => dest.AccessFailedCount, src => src.MapFrom(val => (val.AccessFailedCount == default) ? 0 : val.AccessFailedCount))
                .ForMember(dest => dest.AccessSuccessCount, src => src.MapFrom(val => (val.AccessSuccessCount == default) ? 0 : val.AccessSuccessCount))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore());

            CreateMap<uvw_User, UserV1>()
                .ForMember(dest => dest.IssuerId, src => src.Ignore())
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.UserName))
                .ForMember(dest => dest.Email, src => src.MapFrom(val => val.EmailAddress))
                .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(val => val.EmailConfirmed));
        }
    }
}
