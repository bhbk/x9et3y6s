using Bhbk.Lib.Identity.Manager;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Repository;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    public interface IUnitOfWork : IDisposable
    {
        void Save();
        Task SaveAsync();
        CustomIdentityDbContext CustomContext { get; set; }
        CustomUserManager CustomUserManager { get; }
        CustomRoleManager CustomRoleManager { get; }
        IGenericRepository<AppAudience, Guid> AudienceRepository { get; }
        IGenericRepository<AppClient, Guid> ClientRepository { get; }
        IGenericRepository<AppRealm, Guid> RealmRepository { get; }
        IGenericRepository<AppRole, Guid> RoleRepository { get; }
        IGenericRepository<AppUser, Guid> UserRepository { get; }
    }
}
