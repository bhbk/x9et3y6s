using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
using Bhbk.Lib.Identity.Data_EF6.Repositories_Tbl;

namespace Bhbk.Lib.Identity.Data_EF6.Infrastructure_Tbl
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        AudienceRepository Audiences { get; }
        AuthActivityRepository AuthActivity { get; }
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
