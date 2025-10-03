using System;

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_JobSetting
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsSecret { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_Job Job { get; set; }
    }
}
