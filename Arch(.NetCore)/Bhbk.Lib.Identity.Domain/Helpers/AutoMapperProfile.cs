using AutoMapper;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            /*
             * activity models
             */
            CreateMap<ActivityCreate, tbl_Activities>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore())
                .ForMember(dest => dest.KeyValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.KeyValues)))
                .ForMember(dest => dest.OriginalValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.OriginalValues)))
                .ForMember(dest => dest.CurrentValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.CurrentValues)))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));

            CreateMap<ActivityModel, tbl_Activities>()
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore())
                .ReverseMap();

            /*
             * claim models
             */
            CreateMap<ClaimCreate, tbl_Claims>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserClaims, src => src.Ignore());

            CreateMap<ClaimModel, tbl_Claims>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserClaims, src => src.Ignore())
                .ReverseMap();

            /*
             * client models
             */
            CreateMap<AudienceCreate, tbl_Audiences>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.LockoutEnd, src => src.Ignore())
                .ForMember(dest => dest.LastLoginFailure, src => src.Ignore())
                .ForMember(dest => dest.LastLoginSuccess, src => src.Ignore())
                .ForMember(dest => dest.AccessFailedCount, src => src.MapFrom(val => 0))
                .ForMember(dest => dest.AccessSuccessCount, src => src.MapFrom(val => 0))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => AlphaNumeric.CreateString(32)))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.SecurityStamp, src => src.MapFrom(val => AlphaNumeric.CreateString(32)))
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.tbl_Activities, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_Roles, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ForMember(dest => dest.tbl_Urls, src => src.Ignore());

            CreateMap<AudienceModel, tbl_Audiences>()
                .ForMember(dest => dest.ConcurrencyStamp, src => src.Ignore())
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.SecurityStamp, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.tbl_Activities, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Roles, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ForMember(dest => dest.tbl_Urls, src => src.Ignore())
                .ReverseMap();

            /*
             * email models
             */
            CreateMap<EmailCreate, tbl_QueueEmails>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.SendAt, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.From, src => src.Ignore());

            /*
             * issuer models
             */
            CreateMap<IssuerCreate, tbl_Issuers>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.IssuerKey, src => src.MapFrom(val => AlphaNumeric.CreateString(32)))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.tbl_Claims, src => src.Ignore())
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore());

            CreateMap<IssuerModel, tbl_Issuers>()
                .ForMember(dest => dest.IssuerKey, src => src.Ignore())
                .ForMember(dest => dest.tbl_Claims, src => src.Ignore())
                .ForMember(dest => dest.tbl_Audiences, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ReverseMap();

            /*
             * login models
             */
            CreateMap<LoginCreate, tbl_Logins>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserLogins, src => src.Ignore());

            CreateMap<LoginModel, tbl_Logins>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.LoginKey, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserLogins, src => src.Ignore())
                .ReverseMap();

            /*
             * message of the day models
             */
            CreateMap<MOTDType1Model, tbl_MotDType1>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.id))
                .ForMember(dest => dest.Title, src => src.MapFrom(val => val.title))
                .ForMember(dest => dest.Author, src => src.MapFrom(val => val.author))
                .ForMember(dest => dest.Quote, src => src.MapFrom(val => val.quote))
                .ForMember(dest => dest.Category, src => src.MapFrom(val => val.category))
                .ForMember(dest => dest.Date, src => src.MapFrom(val => val.date))
                .ForMember(dest => dest.Tags, src => src.MapFrom(val => string.Join(",", val.tags.Select(x => x))))
                .ForMember(dest => dest.Length, src => src.MapFrom(val => val.length))
                .ForMember(dest => dest.Background, src => src.MapFrom(val => val.background));

            CreateMap<tbl_MotDType1, MOTDType1Model>()
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
            CreateMap<RefreshCreate, tbl_Refreshes>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<RefreshModel, tbl_Refreshes>()
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore())
                .ForMember(dest => dest.RefreshValue, src => src.Ignore())
                .ReverseMap();

            /*
             * role models
             */
            CreateMap<RoleCreate, tbl_Roles>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => Base64.CreateString(32)))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore());

            CreateMap<RoleModel, tbl_Roles>()
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore())
                .ReverseMap();

            /*
             * setting models
             */
            CreateMap<SettingCreate, tbl_Settings>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            /*
             * state models
             */
            CreateMap<StateCreate, tbl_States>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastPolling, src => src.Ignore())
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            /*
             * text models
             */
            CreateMap<TextCreate, tbl_QueueTexts>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.SendAt, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.From, src => src.Ignore());

            /*
             * url models
             */
            CreateMap<UrlCreate, tbl_Urls>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Audience, src => src.Ignore());

            /*
             * user models
             */
            CreateMap<UserCreate, tbl_Users>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.LockoutEnd, src => src.Ignore())
                .ForMember(dest => dest.LastLoginFailure, src => src.Ignore())
                .ForMember(dest => dest.LastLoginSuccess, src => src.Ignore())
                .ForMember(dest => dest.AccessFailedCount, src => src.MapFrom(val => 0))
                .ForMember(dest => dest.AccessSuccessCount, src => src.MapFrom(val => 0))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => AlphaNumeric.CreateString(32)))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.PasswordConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.SecurityStamp, src => src.MapFrom(val => AlphaNumeric.CreateString(32)))
                .ForMember(dest => dest.TwoFactorEnabled, src => src.MapFrom(val => false))
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

            CreateMap<UserModel, tbl_Users>()
                .ForMember(dest => dest.ConcurrencyStamp, src => src.Ignore())
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
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore())
                .ReverseMap();
        }
    }
}
