using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_ChatFavorite
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Prompt { get; set; }
        public bool Pinned { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_User User { get; set; }
    }
}
