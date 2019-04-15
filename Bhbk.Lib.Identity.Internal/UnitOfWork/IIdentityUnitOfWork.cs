using AutoMapper;
using Bhbk.Lib.Core.UnitOfWork;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Repositories;

namespace Bhbk.Lib.Identity.Internal.UnitOfWork
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IIdentityUnitOfWork<TContext> : IGenericUnitOfWorkAsync
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
