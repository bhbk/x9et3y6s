using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Identity.Data.Repositories;

namespace Bhbk.Lib.Identity.Data.Services
{
    public interface IUoWService : IGenericUnitOfWork
    {
        public ActivityRepository Activities { get; }
        public ActivityRepository_Deprecate Activities_Deprecate { get; }
        public ClaimRepository Claims { get; }
        public ClientRepository Clients { get; }
        public IssuerRepository Issuers { get; }
        public LoginRepository Logins { get; }
        public MotDRepository MOTDs { get; }
        public QueueEmailRepository QueueEmails { get; }
        public QueueTextRepository QueueTexts { get; }
        public RoleRepository Roles { get; }
        public RefreshRepository Refreshes { get; }
        public SettingRepository Settings { get; }
        public StateRepository States { get; }
        public UrlRepository Urls { get; }
        public UserRepository Users { get; }
    }
}
