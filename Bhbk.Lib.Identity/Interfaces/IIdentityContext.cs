using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Identity.Managers;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Repository;

namespace Bhbk.Lib.Identity.Interfaces
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IIdentityContext<TContext> : IGenericUnitOfWork
        where TContext : AppDbContext
    {
        IMapper Convert { get; }
        ActivityRepository ActivityRepo { get; }
        AudienceRepository AudienceRepo { get; }
        ClientRepository ClientRepo { get; }
        ConfigRepository ConfigRepo { get; }
        CustomRoleManager CustomRoleMgr { get; }
        CustomUserManager CustomUserMgr { get; }
        LoginRepository LoginRepo { get; }
    }
}
