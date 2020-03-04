using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_Settings
    {
        public Guid Id { get; set; }
        public Guid? IssuerId { get; set; }
        public Guid? AudienceId { get; set; }
        public Guid? UserId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }
    }
}
