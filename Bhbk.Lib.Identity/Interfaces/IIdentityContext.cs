using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Managers;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using System;

namespace Bhbk.Lib.Identity.Interfaces
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IIdentityContext : IDisposable
    {
        AppDbContext GetContext();
        ContextType Status { get; }
        ActivityStore Activity { get; }
        AudienceManager AudienceMgmt { get; }
        ClientManager ClientMgmt { get; }
        ConfigStore ConfigStore { get; }
        LoginManager LoginMgmt { get; }
        CustomRoleManager RoleMgmt { get; }
        CustomUserManager UserMgmt { get; }
    }
}
