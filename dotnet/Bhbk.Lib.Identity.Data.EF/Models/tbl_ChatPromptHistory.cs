using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_ChatPromptHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string PromptText { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_User User { get; set; }
    }
}
