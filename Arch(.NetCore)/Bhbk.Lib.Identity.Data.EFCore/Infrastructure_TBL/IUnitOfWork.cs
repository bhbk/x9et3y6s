using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EFCore.Models_TBL;
using Bhbk.Lib.Identity.Data.EFCore.Repositories_TBL;

namespace Bhbk.Lib.Identity.Data.EFCore.Infrastructure_TBL
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public ActivityRepository Activities { get; }
        public AudienceRepository Audiences { get; }
        public IGenericRepository<tbl_Claim> Claims { get; }
        public IGenericRepository<tbl_EmailActivity> EmailActivities { get; }
        public IGenericRepository<tbl_EmailQueue> EmailQueue { get; }
        public IGenericRepository<tbl_Issuer> Issuers { get; }
        public IGenericRepository<tbl_Login> Logins { get; }
        public IGenericRepository<tbl_MOTD> MOTDs { get; }
        public RefreshRepository Refreshes { get; }
        public IGenericRepository<tbl_Role> Roles { get; }
        public IGenericRepository<tbl_Setting> Settings { get; }
        public IGenericRepository<tbl_State> States { get; }
        public IGenericRepository<tbl_TextActivity> TextActivities { get; }
        public IGenericRepository<tbl_TextQueue> TextQueue { get; }
        public IGenericRepository<tbl_Url> Urls { get; }
        public UserRepository Users { get; }
    }
}
