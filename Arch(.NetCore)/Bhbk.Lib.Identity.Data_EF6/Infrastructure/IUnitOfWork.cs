using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Data_EF6.Repositories;

namespace Bhbk.Lib.Identity.Data_EF6.Infrastructure
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        AudienceRepository Audiences { get; }
        AuthActivityRepository AuthActivity { get; }
        IGenericRepository<E_Claim> Claims { get; }
        IGenericRepository<E_EmailQueue> EmailQueue { get; }
        IGenericRepository<E_Issuer> Issuers { get; }
        IGenericRepository<E_Login> Logins { get; }
        IGenericRepository<E_MOTD> MOTDs { get; }
        RefreshRepository Refreshes { get; }
        IGenericRepository<E_Role> Roles { get; }
        IGenericRepository<E_Setting> Settings { get; }
        IGenericRepository<E_State> States { get; }
        IGenericRepository<E_TextQueue> TextQueue { get; }
        IGenericRepository<E_Url> Urls { get; }
        UserRepository Users { get; }
    }
}
