using AutoMapper;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Repositories;

namespace Bhbk.Lib.Identity.Internal.Interfaces
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IIdentityUnitOfWork<TContext> : IGenericUnitOfWork
        where TContext : IdentityDbContext
    {
        IMapper Shape { get; }
        ActivityRepository ActivityRepo { get; }
        ClaimRepository ClaimRepo { get; }
        ClientRepository ClientRepo { get; }
        ConfigRepository ConfigRepo { get; }
        IssuerRepository IssuerRepo { get; }
        LoginRepository LoginRepo { get; }
        RoleRepository RoleRepo { get; }
        RefreshRepository RefreshRepo { get; }
        StateRepository StateRepo { get; }
        UserRepository UserRepo { get; }
    }
}
