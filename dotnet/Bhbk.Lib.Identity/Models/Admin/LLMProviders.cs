using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class LLMProviders
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public int FailoverOrder { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class LLMProviderV1 : LLMProviders
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Modified { get; set; }

        public List<LLMProviderSettingV1> Settings { get; set; }
    }

    public class LLMProviderSettingV1
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public string ConfigKey { get; set; }

        [Required]
        public string ConfigValue { get; set; }

        public bool IsSecret { get; set; }

        public bool IsDeletable { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; }
    }

    public class LLMProviderOrderV1
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public int FailoverOrder { get; set; }
    }
}
