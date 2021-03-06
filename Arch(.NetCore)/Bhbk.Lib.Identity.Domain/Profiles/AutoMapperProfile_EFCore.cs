﻿using AutoMapper;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Profiles
{
    public class AutoMapperProfile_EFCore : Profile
    {
        public AutoMapperProfile_EFCore()
        {
            /*
             * activity models
             */

            CreateMap<AuthActivityV1, uvw_AuthActivity>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc));

            CreateMap<uvw_AuthActivity, AuthActivityV1>();

            /*
             * audience models
             */

            CreateMap<AudienceV1, uvw_Audience>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore());

            CreateMap<uvw_Audience, AudienceV1>()
                .ForMember(dest => dest.Roles, src => src.Ignore());

            /*
             * claim models
             */

            CreateMap<ClaimV1, uvw_Claim>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc));

            CreateMap<uvw_Claim, ClaimV1>();

            /*
             * email models
             */

            CreateMap<EmailV1, uvw_EmailQueue>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.SendAtUtc, src => src.MapFrom(val => val.SendAtUtc == default ? DateTime.UtcNow : val.SendAtUtc));

            CreateMap<uvw_EmailQueue, EmailV1>();

            /*
             * issuer models
             */

            CreateMap<IssuerV1, uvw_Issuer>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc));

            CreateMap<uvw_Issuer, IssuerV1>()
                .ForMember(dest => dest.Audiences, src => src.Ignore());

            /*
             * login models
             */

            CreateMap<LoginV1, uvw_Login>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc));

            CreateMap<uvw_Login, LoginV1>();

            /*
             * message of the day models
             */

            CreateMap<MOTDTssV1, uvw_MOTD>()
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
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id));

            CreateMap<uvw_Refresh, RefreshV1>();

            /*
             * role models
             */

            CreateMap<RoleV1, uvw_Role>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc));

            CreateMap<uvw_Role, RoleV1>();

            /*
             * setting models
             */

            CreateMap<SettingV1, uvw_Setting>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc));

            /*
             * state models
             */

            CreateMap<StateV1, uvw_State>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.IssuedUtc, src => src.MapFrom(val => val.IssuedUtc == default ? DateTime.UtcNow : val.IssuedUtc))
                .ForMember(dest => dest.LastPollingUtc, src => src.Ignore());

            /*
             * text models
             */

            CreateMap<TextV1, uvw_TextQueue>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.SendAtUtc, src => src.MapFrom(val => val.SendAtUtc == default ? DateTime.UtcNow : val.SendAtUtc));

            CreateMap<uvw_TextQueue, TextV1>();

            /*
             * url models
             */

            CreateMap<UrlV1, uvw_Url>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc));

            CreateMap<uvw_Url, UrlV1>();

            /*
             * user models
             */

            CreateMap<UserV1, uvw_User>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.EmailAddress, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.CreatedUtc, src => src.MapFrom(val => val.CreatedUtc == default ? DateTime.UtcNow : val.CreatedUtc))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore());

            CreateMap<uvw_User, UserV1>()
                .ForMember(dest => dest.IssuerId, src => src.Ignore())
                .ForMember(dest => dest.Email, src => src.MapFrom(val => val.EmailAddress));
        }
    }
}
