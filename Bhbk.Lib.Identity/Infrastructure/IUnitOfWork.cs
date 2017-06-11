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
        CustomIdentityDbContext CustomContext { get; set; }
        CustomConfigManager CustomConfigManager { get; }
        CustomProviderManager CustomProviderManager { get; }
        CustomRoleManager CustomRoleManager { get; }
        CustomUserManager CustomUserManager { get; }
        IGenericRepository<AppAudience, Guid> AudienceRepository { get; }
        IGenericRepository<AppClient, Guid> ClientRepository { get; }
        IGenericRepository<AppProvider, Guid> ProviderRepository { get; }
        IGenericRepository<AppRole, Guid> RoleRepository { get; }
        IGenericRepository<AppUser, Guid> UserRepository { get; }
        void Save();
        Task SaveAsync();
    }
}
