using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Data.EF.Repositories;

namespace Bhbk.Lib.Identity.Data.EF.Infrastructure
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public AudienceRepository Audiences { get; }
        public IGenericRepository<tbl_UserActivity> UserActivities { get; }
        public IGenericRepository<tbl_AudienceActivity> AudienceActivities { get; }
        public IGenericRepository<tbl_ChatConversation> ChatConversations { get; }
        public IGenericRepository<tbl_ChatFavorite> ChatFavorites { get; }
        public IGenericRepository<tbl_ChatFile> ChatFiles { get; }
        public IGenericRepository<tbl_ChatMessage> ChatMessages { get; }
        public IGenericRepository<tbl_ChatPrompt> ChatPrompts { get; }
        public IGenericRepository<tbl_ChatPromptHistory> ChatPromptHistories { get; }
        public IGenericRepository<tbl_Claim> Claims { get; }
        public IGenericRepository<tbl_EmailActivity> EmailActivity { get; }
        public IGenericRepository<tbl_EmailQueue> EmailQueue { get; }
        public IGenericRepository<tbl_UserEntitlement> UserEntitlements { get; }
        public IGenericRepository<tbl_AudienceEntitlement> AudienceEntitlements { get; }
        public IGenericRepository<tbl_EntitlementScope> EntitlementScopes { get; }
        public IGenericRepository<tbl_EntitlementType> EntitlementTypes { get; }
        public IGenericRepository<tbl_Issuer> Issuers { get; }
        public IGenericRepository<tbl_Job> Jobs { get; }
        public IGenericRepository<tbl_JobSetting> JobSettings { get; }
        public IGenericRepository<tbl_LLMProvider> LLMProviders { get; }
        public IGenericRepository<tbl_LLMProviderSetting> LLMProviderSettings { get; }
        public IGenericRepository<tbl_LoginProvider> LoginProviders { get; }
        public IGenericRepository<tbl_Quote> Quotes { get; }
        public IGenericRepository<tbl_Refresh> Refreshes { get; }
        public IGenericRepository<tbl_Role> Roles { get; }
        public IGenericRepository<tbl_Setting> Settings { get; }
        public IGenericRepository<tbl_State> States { get; }
        public IGenericRepository<tbl_TextActivity> TextActivity { get; }
        public IGenericRepository<tbl_TextQueue> TextQueue { get; }
        public IGenericRepository<tbl_Url> Urls { get; }
        public UserRepository Users { get; }
    }
}
