using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Data.EF6.Repositories;

namespace Bhbk.Lib.Identity.Data.EF6.Services
{
    public interface IUoWService : IGenericUnitOfWork
    {
        IdentityEntities Context { get; }
        ActivityRepository Activities { get; }
    }
}
