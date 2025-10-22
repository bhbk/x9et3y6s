using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_LoginProvider
    {
        public tbl_LoginProvider()
        {
            tbl_UserLoginProviders = new HashSet<tbl_UserLoginProvider>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProviderKey { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual ICollection<tbl_UserLoginProvider> tbl_UserLoginProviders { get; set; }
    }
}
