using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Data.EF.Repositories;

namespace Bhbk.Lib.Identity.Data.EF.Infrastructure
{
    public interface IUnitOfWorkAsync : IGenericUnitOfWorkAsync
    {
        public AudienceRepositoryAsync Audiences { get; }
        public IGenericRepositoryAsync<tbl_UserAuthActivity> UserAuthActivities { get; }
        public IGenericRepositoryAsync<tbl_AudienceAuthActivity> AudienceAuthActivities { get; }
        public IGenericRepositoryAsync<tbl_ChatConversation> ChatConversations { get; }
        public IGenericRepositoryAsync<tbl_ChatFavorite> ChatFavorites { get; }
        public IGenericRepositoryAsync<tbl_ChatFile> ChatFiles { get; }
        public IGenericRepositoryAsync<tbl_ChatMessage> ChatMessages { get; }
        public IGenericRepositoryAsync<tbl_ChatPrompt> ChatPrompts { get; }
        public IGenericRepositoryAsync<tbl_Claim> Claims { get; }
        public IGenericRepositoryAsync<tbl_EmailActivity> EmailActivity { get; }
        public IGenericRepositoryAsync<tbl_EmailQueue> EmailQueue { get; }
        public IGenericRepositoryAsync<tbl_UserEntitlement> UserEntitlements { get; }
        public IGenericRepositoryAsync<tbl_AudienceEntitlement> AudienceEntitlements { get; }
        public IGenericRepositoryAsync<tbl_EntitlementScope> EntitlementScopes { get; }
        public IGenericRepositoryAsync<tbl_EntitlementType> EntitlementTypes { get; }
        public IGenericRepositoryAsync<tbl_Issuer> Issuers { get; }
        public IGenericRepositoryAsync<tbl_Job> Jobs { get; }
        public IGenericRepositoryAsync<tbl_JobSetting> JobSettings { get; }
        public IGenericRepositoryAsync<tbl_LLMProvider> LLMProviders { get; }
        public IGenericRepositoryAsync<tbl_LLMProviderSetting> LLMProviderSettings { get; }
        public IGenericRepositoryAsync<tbl_LoginProvider> LoginProviders { get; }
        public IGenericRepositoryAsync<tbl_Quote> Quotes { get; }
        public IGenericRepositoryAsync<tbl_Refresh> Refreshes { get; }
        public IGenericRepositoryAsync<tbl_Role> Roles { get; }
        public IGenericRepositoryAsync<tbl_Setting> Settings { get; }
        public IGenericRepositoryAsync<tbl_State> States { get; }
        public IGenericRepositoryAsync<tbl_TextActivity> TextActivity { get; }
        public IGenericRepositoryAsync<tbl_TextQueue> TextQueue { get; }
        public IGenericRepositoryAsync<tbl_Url> Urls { get; }
        public UserRepositoryAsync Users { get; }
    }
}
