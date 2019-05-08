using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Identity.Data.Repositories;

namespace Bhbk.Lib.Identity.Data.Services
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IUoWService : IGenericUnitOfWorkAsync
    {
        ActivityRepository ActivityRepo { get; }
        ClaimRepository ClaimRepo { get; }
        ClientRepository ClientRepo { get; }
        IssuerRepository IssuerRepo { get; }
        LoginRepository LoginRepo { get; }
        RoleRepository RoleRepo { get; }
        RefreshRepository RefreshRepo { get; }
        SettingRepository SettingRepo { get; }
        StateRepository StateRepo { get; }
        UserRepository UserRepo { get; }
    }
}
