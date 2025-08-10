using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Data.EF.Repositories;

namespace Bhbk.Lib.Identity.Data.EF.Infrastructure
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public AudienceRepository Audiences { get; }
        public AuthActivityRepository AuthActivity { get; }
        public IGenericRepository<tbl_AuthActivityAudience> AuthActivityAudiences { get; }
        public IGenericRepository<tbl_Claim> Claims { get; }
        public IGenericRepository<tbl_EmailActivity> EmailActivity { get; }
        public IGenericRepository<tbl_EmailQueue> EmailQueue { get; }
        public IssuerRepository Issuers { get; }
        public LoginRepository Logins { get; }
        public IGenericRepository<tbl_MOTD> MOTDs { get; }
        public RefreshRepository Refreshes { get; }
        public RoleRepository Roles { get; }
        public IGenericRepository<tbl_Setting> Settings { get; }
        public IGenericRepository<tbl_State> States { get; }
        public IGenericRepository<tbl_TextActivity> TextActivity { get; }
        public IGenericRepository<tbl_TextQueue> TextQueue { get; }
        public IGenericRepository<tbl_Url> Urls { get; }
        public UserRepository Users { get; }
    }
}
