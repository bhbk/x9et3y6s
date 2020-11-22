using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EFCore.Repositories_TSQL;

namespace Bhbk.Lib.Identity.Data.EFCore.Infrastructure_TSQL
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public ActivityRepository Activities { get; }
        public AudienceRepository Audiences { get; }
        public ClaimRepository Claims { get; }
        public EmailActivityRepository EmailActivities { get; }
        public EmailQueueRepository EmailQueue { get; }
        public IssuerRepository Issuers { get; }
        public LoginRepository Logins { get; }
        public MOTDRepository MOTDs { get; }
        public RoleRepository Roles { get; }
        public RefreshRepository Refreshes { get; }
        public SettingRepository Settings { get; }
        public StateRepository States { get; }
        public TextActivityRepository TextActivities { get; }
        public TextQueueRepository TextQueue { get; }
        public UrlRepository Urls { get; }
        public UserRepository Users { get; }
    }
}
