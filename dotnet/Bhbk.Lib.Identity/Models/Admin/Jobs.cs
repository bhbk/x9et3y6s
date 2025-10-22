using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Jobs
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class JobV1 : Jobs
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Modified { get; set; }

        public List<JobSettingV1> Settings { get; set; }
    }

    public class JobSettingV1
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid JobId { get; set; }

        [Required]
        public string ConfigKey { get; set; }

        [Required]
        public string ConfigValue { get; set; }

        public bool IsSecret { get; set; }

        public bool IsDeletable { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; }
    }
}
