using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_Job
    {
        public tbl_Job()
        {
            tbl_JobSettings = new HashSet<tbl_JobSetting>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? ModifiedUtc { get; set; }

        public virtual ICollection<tbl_JobSetting> tbl_JobSettings { get; set; }
    }
}
