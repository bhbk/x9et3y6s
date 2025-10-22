using AutoMapper;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            /*
             * activity models
             */

            CreateMap<UserAuthActivityV1, tbl_UserAuthActivity>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.tbl_AudienceAuthActivities, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<tbl_UserAuthActivity, UserAuthActivityV1>()
                .ForMember(dest => dest.AudienceIds, src => src.MapFrom(val => val.tbl_AudienceAuthActivities.Select(x => x.AudienceId).ToList()));

            /*
             * audience models
             */

            CreateMap<AudienceV1, tbl_Audience>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceAuthActivities, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserEntitlements, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceEntitlements, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Roles, src => src.MapFrom(val => val.Roles))
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ForMember(dest => dest.tbl_Urls, src => src.Ignore());

            CreateMap<tbl_Audience, AudienceV1>()
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.tbl_Roles));

            /*
             * claim models
             */

            CreateMap<ClaimV1, tbl_Claim>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserClaims, src => src.Ignore());

            CreateMap<tbl_Claim, ClaimV1>();

            /*
             * email models
             */

            CreateMap<EmailV1, tbl_EmailQueue>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.SendAt, src => src.MapFrom(val => val.SendAt == default ? DateTime.UtcNow : val.SendAt))
                .ForMember(dest => dest.tbl_EmailActivities, src => src.Ignore());

            CreateMap<tbl_EmailQueue, EmailV1>();

            /*
             * entitlement type models
             */

            CreateMap<EntitlementTypeV1, tbl_EntitlementType>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.tbl_UserEntitlements, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceEntitlements, src => src.Ignore());

            CreateMap<tbl_EntitlementType, EntitlementTypeV1>();

            /*
             * entitlement scope models
             */

            CreateMap<EntitlementScopeV1, tbl_EntitlementScope>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.tbl_UserEntitlements, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceEntitlements, src => src.Ignore());

            CreateMap<tbl_EntitlementScope, EntitlementScopeV1>();

            /*
             * user entitlement models
             */

            CreateMap<UserEntitlementV1, tbl_UserEntitlement>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.User, src => src.Ignore())
                .ForMember(dest => dest.EntitlementType, src => src.Ignore())
                .ForMember(dest => dest.EntitlementScope, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.Audience, src => src.Ignore());

            CreateMap<tbl_UserEntitlement, UserEntitlementV1>()
                .ForMember(dest => dest.UserName, src => src.MapFrom(val => val.User != null ? val.User.UserName : null))
                .ForMember(dest => dest.EntitlementTypeName, src => src.MapFrom(val => val.EntitlementType != null ? val.EntitlementType.Name : null))
                .ForMember(dest => dest.EntitlementScopeName, src => src.MapFrom(val => val.EntitlementScope != null ? val.EntitlementScope.Name : null))
                .ForMember(dest => dest.IssuerName, src => src.MapFrom(val => val.Issuer != null ? val.Issuer.Name : null))
                .ForMember(dest => dest.AudienceName, src => src.MapFrom(val => val.Audience != null ? val.Audience.Name : null));

            /*
             * audience entitlement models
             */

            CreateMap<AudienceEntitlementV1, tbl_AudienceEntitlement>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.EntitlementType, src => src.Ignore())
                .ForMember(dest => dest.EntitlementScope, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore());

            CreateMap<tbl_AudienceEntitlement, AudienceEntitlementV1>()
                .ForMember(dest => dest.AudienceName, src => src.MapFrom(val => val.Audience != null ? val.Audience.Name : null))
                .ForMember(dest => dest.EntitlementTypeName, src => src.MapFrom(val => val.EntitlementType != null ? val.EntitlementType.Name : null))
                .ForMember(dest => dest.EntitlementScopeName, src => src.MapFrom(val => val.EntitlementScope != null ? val.EntitlementScope.Name : null))
                .ForMember(dest => dest.IssuerName, src => src.MapFrom(val => val.Issuer != null ? val.Issuer.Name : null));

            /*
             * issuer models
             */

            CreateMap<IssuerV1, tbl_Issuer>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.IssuerKey, src => src.Ignore())
                .ForMember(dest => dest.tbl_Audiences, src => src.MapFrom(val => val.Audiences))
                .ForMember(dest => dest.tbl_Claims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserEntitlements, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceEntitlements, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore());

            CreateMap<tbl_Issuer, IssuerV1>()
                .ForMember(dest => dest.Audiences, src => src.MapFrom(val => val.tbl_Audiences));

            /*
             * login provider models
             */

            CreateMap<LoginProviderV1, tbl_LoginProvider>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.tbl_UserLoginProviders, src => src.Ignore());

            CreateMap<tbl_LoginProvider, LoginProviderV1>()
                .ForMember(dest => dest.Users, src => src.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrEmpty(src.ProviderKey))
                        dest.ProviderKey = "********";
                });

            /*
             * quote models
             */

            CreateMap<QuoteV1, tbl_Quote>()
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

            CreateMap<tbl_Quote, QuoteV1>()
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
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.tbl_AudienceRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_RoleClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore());

            CreateMap<tbl_Role, RoleV1>()
                .ForMember(dest => dest.Audiences, src => src.Ignore())
                .ForMember(dest => dest.Users, src => src.Ignore());

            /*
             * setting models
             */

            CreateMap<SettingV1, tbl_Setting>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            /*
             * state models
             */

            CreateMap<StateV1, tbl_State>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Issued, src => src.MapFrom(val => val.Issued == default ? DateTime.UtcNow : val.Issued))
                .ForMember(dest => dest.LastPolling, src => src.Ignore())
                .ForMember(dest => dest.Audience, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            /*
             * text models
             */

            CreateMap<TextV1, tbl_TextQueue>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.SendAt, src => src.MapFrom(val => val.SendAt == default ? DateTime.UtcNow : val.SendAt))
                .ForMember(dest => dest.tbl_TextActivities, src => src.Ignore());

            CreateMap<tbl_TextQueue, TextV1>();

            /*
             * url models
             */

            CreateMap<UrlV1, tbl_Url>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.Audience, src => src.Ignore());

            CreateMap<tbl_Url, UrlV1>();

            /*
             * user models
             */

            CreateMap<UserV1, tbl_User>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.EmailAddress, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => val.ConcurrencyStamp == null ? Guid.NewGuid().ToString() : val.ConcurrencyStamp))
                .ForMember(dest => dest.SecurityStamp, src => src.MapFrom(val => val.SecurityStamp == null ? Guid.NewGuid().ToString() : val.SecurityStamp))
                .ForMember(dest => dest.PasswordHashPBKDF2, src => src.Ignore())
                .ForMember(dest => dest.PasswordHashSHA256, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserAuthActivities, src => src.Ignore())
                .ForMember(dest => dest.tbl_ChatConversations, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserEntitlements, src => src.Ignore())
                .ForMember(dest => dest.tbl_Refreshes, src => src.Ignore())
                .ForMember(dest => dest.tbl_Settings, src => src.Ignore())
                .ForMember(dest => dest.tbl_States, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserClaims, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserLoginProviders, src => src.Ignore())
                .ForMember(dest => dest.tbl_UserRoles, src => src.Ignore())
                .ForMember(dest => dest.tbl_ChatFavorites, src => src.Ignore())
                .ForMember(dest => dest.tbl_ChatPromptHistories, src => src.Ignore());

            CreateMap<tbl_User, UserV1>()
                .ForMember(dest => dest.Email, src => src.MapFrom(val => val.EmailAddress))
                .ForMember(dest => dest.Claims, src => src.Ignore())
                .ForMember(dest => dest.LoginProviders, src => src.Ignore())
                .ForMember(dest => dest.Roles, src => src.Ignore());

            /*
             * llm provider models
             */

            CreateMap<LLMProviderV1, tbl_LLMProvider>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.tbl_LLMProviderSettings, src => src.Ignore());

            CreateMap<tbl_LLMProvider, LLMProviderV1>()
                .ForMember(dest => dest.Settings, src => src.MapFrom(val => val.tbl_LLMProviderSettings));

            CreateMap<LLMProviderSettingV1, tbl_LLMProviderSetting>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.Provider, src => src.Ignore());

            CreateMap<tbl_LLMProviderSetting, LLMProviderSettingV1>()
                .AfterMap((src, dest) =>
                {
                    if (src.IsSecret)
                        dest.ConfigValue = "********";
                });

            /*
             * job models
             */

            CreateMap<JobV1, tbl_Job>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.tbl_JobSettings, src => src.Ignore());

            CreateMap<tbl_Job, JobV1>()
                .ForMember(dest => dest.Settings, src => src.MapFrom(val => val.tbl_JobSettings));

            CreateMap<JobSettingV1, tbl_JobSetting>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.Id == default ? Guid.NewGuid() : val.Id))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => val.Created == default ? DateTime.UtcNow : val.Created))
                .ForMember(dest => dest.Job, src => src.Ignore());

            CreateMap<tbl_JobSetting, JobSettingV1>()
                .AfterMap((src, dest) =>
                {
                    if (src.IsSecret)
                        dest.ConfigValue = "********";
                });
        }
    }
}
