using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_LLMProvider
    {
        public tbl_LLMProvider()
        {
            tbl_LLMProviderSettings = new HashSet<tbl_LLMProviderSetting>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Context { get; set; }
        public bool IsEnabled { get; set; }
        public int FailoverOrder { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Modified { get; set; }

        public virtual ICollection<tbl_LLMProviderSetting> tbl_LLMProviderSettings { get; set; }
    }
}
