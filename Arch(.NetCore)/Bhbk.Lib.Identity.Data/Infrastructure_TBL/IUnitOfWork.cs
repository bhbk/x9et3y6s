using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Data.Repositories_Tbl;

namespace Bhbk.Lib.Identity.Data.Infrastructure_Tbl
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public AudienceRepository Audiences { get; }
        public AuthActivityRepository AuthActivity { get; }
        public IGenericRepository<tbl_Claim> Claims { get; }
        public IGenericRepository<tbl_EmailActivity> EmailActivity { get; }
        public IGenericRepository<tbl_EmailQueue> EmailQueue { get; }
        public IGenericRepository<tbl_Issuer> Issuers { get; }
        public IGenericRepository<tbl_Login> Logins { get; }
        public IGenericRepository<tbl_MOTD> MOTDs { get; }
        public RefreshRepository Refreshes { get; }
        public IGenericRepository<tbl_Role> Roles { get; }
        public IGenericRepository<tbl_Setting> Settings { get; }
        public IGenericRepository<tbl_State> States { get; }
        public IGenericRepository<tbl_TextActivity> TextActivity { get; }
        public IGenericRepository<tbl_TextQueue> TextQueue { get; }
        public IGenericRepository<tbl_Url> Urls { get; }
        public UserRepository Users { get; }
    }
}
