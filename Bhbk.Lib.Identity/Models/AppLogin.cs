using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppLogin
    {
        public AppLogin()
        {
            AppUserLogin = new HashSet<AppUserLogin>();
        }

        public Guid Id { get; set; }
        public string LoginProvider { get; set; }

        public ICollection<AppUserLogin> AppUserLogin { get; set; }
    }
}
