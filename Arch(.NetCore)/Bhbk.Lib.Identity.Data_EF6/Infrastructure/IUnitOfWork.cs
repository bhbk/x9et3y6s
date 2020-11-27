using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Data_EF6.Repositories;

namespace Bhbk.Lib.Identity.Data_EF6.Infrastructure
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        ActivityRepository Activities { get; }
        AudienceRepository Audiences { get; }
        IGenericRepository<uvw_Claim> Claims { get; }
        IGenericRepository<uvw_EmailQueue> EmailQueue { get; }
        IGenericRepository<uvw_Issuer> Issuers { get; }
        IGenericRepository<uvw_Login> Logins { get; }
        IGenericRepository<uvw_MOTD> MOTDs { get; }
        RefreshRepository Refreshes { get; }
        IGenericRepository<uvw_Role> Roles { get; }
        IGenericRepository<uvw_Setting> Settings { get; }
        IGenericRepository<uvw_State> States { get; }
        IGenericRepository<uvw_TextQueue> TextQueue { get; }
        IGenericRepository<uvw_Url> Urls { get; }
        UserRepository Users { get; }
    }
}
