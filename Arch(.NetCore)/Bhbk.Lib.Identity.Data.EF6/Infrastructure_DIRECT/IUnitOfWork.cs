using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT;

namespace Bhbk.Lib.Identity.Data.EF6.Infrastructure_DIRECT
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        ActivityRepository Activities { get; }
        AudienceRepository Audiences { get; }
        IGenericRepository<tbl_Claim> Claims { get; }
        IGenericRepository<tbl_EmailQueue> EmailQueue { get; }
        IGenericRepository<tbl_Issuer> Issuers { get; }
        IGenericRepository<tbl_Login> Logins { get; }
        IGenericRepository<tbl_MOTD> MOTDs { get; }
        IGenericRepository<tbl_Role> Roles { get; }
        RefreshRepository Refreshes { get; }
        IGenericRepository<tbl_Setting> Settings { get; }
        IGenericRepository<tbl_State> States { get; }
        IGenericRepository<tbl_TextQueue> TextQueue { get; }
        IGenericRepository<tbl_Url> Urls { get; }
        UserRepository Users { get; }
    }
}
