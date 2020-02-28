using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Data.EF6.Repositories;

namespace Bhbk.Lib.Identity.Data.EF6.Services
{
    public interface IUoWService : IGenericUnitOfWork
    {
        ActivityRepository Activities { get; }
        AudienceRepository Audiences { get; }
        ClaimRepository Claims { get; }
        IssuerRepository Issuers { get; }
        LoginRepository Logins { get; }
        MOTDRepository MOTDs { get; }
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
