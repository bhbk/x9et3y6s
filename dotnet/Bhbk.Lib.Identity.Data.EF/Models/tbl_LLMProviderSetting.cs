using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_LLMProviderSetting
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsSecret { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_LLMProvider Provider { get; set; }
    }
}
