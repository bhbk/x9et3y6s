using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.EntityModels
{
    public partial class AppLogin
    {
        public AppLogin()
        {
            AppUserLogin = new HashSet<AppUserLogin>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string LoginProvider { get; set; }
        public bool Immutable { get; set; }

        public virtual ICollection<AppUserLogin> AppUserLogin { get; set; }
    }
}
