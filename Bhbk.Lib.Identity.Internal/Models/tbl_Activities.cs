using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.Models
{
    public partial class tbl_Activities
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ClientId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OriginalValues { get; set; }
        public string CurrentValues { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Clients Client { get; set; }
        public virtual tbl_Users User { get; set; }
    }
}
