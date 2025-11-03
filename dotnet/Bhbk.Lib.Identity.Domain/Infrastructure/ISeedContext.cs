using Bhbk.Lib.Identity.Data.EF.Models;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Domain.Infrastructure
{
    public interface ISeedContext : IDisposable
    {
        tbl_Issuer Issuer { get; }
        IReadOnlyDictionary<string, tbl_Audience> Audiences { get; }
        IReadOnlyDictionary<string, tbl_Role> Roles { get; }
        IReadOnlyDictionary<string, tbl_User> Users { get; }
        IReadOnlyDictionary<string, tbl_LoginProvider> LoginProviders { get; }
        IReadOnlyDictionary<string, tbl_Claim> Claims { get; }
        IReadOnlyDictionary<string, tbl_Url> Urls { get; }
        IReadOnlyList<tbl_State> States { get; }
        IReadOnlyList<tbl_Refresh> Refreshes { get; }

        /*
         * passwords keyed by username for tests that need to authenticate
         */
        IReadOnlyDictionary<string, string> UserPasswords { get; }

        ISeedContext Seed();
        void Destroy();
    }
}
