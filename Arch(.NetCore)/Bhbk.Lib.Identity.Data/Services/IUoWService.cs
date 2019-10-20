using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Identity.Data.Repositories;

namespace Bhbk.Lib.Identity.Data.Services
{
    public interface IUoWService : IGenericUnitOfWorkAsync
    {
        ActivityRepository Activities { get; }
        ClaimRepository Claims { get; }
        ClientRepository Clients { get; }
        IssuerRepository Issuers { get; }
        LoginRepository Logins { get; }
        MotDRepository MOTDs { get; }
        QueueEmailRepository QueueEmails { get; }
        QueueTextRepository QueueTexts { get; }
        RoleRepository Roles { get; }
        RefreshRepository Refreshes { get; }
        SettingRepository Settings { get; }
        StateRepository States { get; }
        UrlRepository Urls { get; }
        UserRepository Users { get; }
    }
}
