using Bhbk.Lib.Identity.Managers;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using Bhbk.Lib.Primitives.Enums;
using System;

namespace Bhbk.Lib.Identity.Interfaces
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IIdentityContext : IDisposable
    {
        AppDbContext GetContext();
        ContextType ContextStatus { get; }
        ActivityStore Activity { get; }
        AudienceManager AudienceMgmt { get; }
        ClientManager ClientMgmt { get; }
        ConfigManager ConfigMgmt { get; }
        LoginManager LoginMgmt { get; }
        CustomRoleManager RoleMgmt { get; }
        CustomUserManager UserMgmt { get; }
    }
}
