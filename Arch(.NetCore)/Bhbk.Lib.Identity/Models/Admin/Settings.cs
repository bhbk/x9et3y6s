using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Settings
    {
        public Guid? IssuerId { get; set; }

        public Guid? AudienceId { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        public string ConfigKey { get; set; }

        [Required]
        public string ConfigValue { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class SettingV1 : Settings
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }
    }
}
