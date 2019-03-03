using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Repository;

namespace Bhbk.Lib.Identity.Internal.Interfaces
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IIdentityContext<TContext> : IGenericUnitOfWork
        where TContext : AppDbContext
    {
        IMapper Convert { get; }
        ActivityRepository ActivityRepo { get; }
        ClientRepository ClientRepo { get; }
        ConfigRepository ConfigRepo { get; }
        IssuerRepository IssuerRepo { get; }
        LoginRepository LoginRepo { get; }
        RoleRepository RoleRepo { get; }
        UserRepository UserRepo { get; }
    }
}
