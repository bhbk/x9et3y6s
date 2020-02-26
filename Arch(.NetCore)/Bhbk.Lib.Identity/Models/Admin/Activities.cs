using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Activities
    {
        public Guid? AudienceId { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        public string ActivityType { get; set; }

        public string TableName { get; set; }

        public string KeyValues { get; set; }

        public string OriginalValues { get; set; }

        public string CurrentValues { get; set; }

        [Required]
        public bool Immutable { get; set; }
    }

    public class ActivityV1 : Activities
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime Created { get; set; }
    }
}
