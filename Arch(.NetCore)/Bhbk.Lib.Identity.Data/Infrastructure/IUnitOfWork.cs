using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Identity.Data.Repositories;

namespace Bhbk.Lib.Identity.Data.Infrastructure
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public AudienceRepository Audiences { get; }
        public AuthActivityRepository AuthActivity { get; }
        public ClaimRepository Claims { get; }
        public EmailActivityRepository EmailActivity { get; }
        public EmailQueueRepository EmailQueue { get; }
        public IssuerRepository Issuers { get; }
        public LoginRepository Logins { get; }
        public MOTDRepository MOTDs { get; }
        public RoleRepository Roles { get; }
        public RefreshRepository Refreshes { get; }
        public SettingRepository Settings { get; }
        public StateRepository States { get; }
        public TextActivityRepository TextActivity { get; }
        public TextQueueRepository TextQueue { get; }
        public UrlRepository Urls { get; }
        public UserRepository Users { get; }
    }
}
