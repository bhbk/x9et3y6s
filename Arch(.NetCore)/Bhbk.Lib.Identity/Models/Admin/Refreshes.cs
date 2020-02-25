using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Refreshes
    {
        [Required]
        public Guid IssuerId { get; set; }

        public Guid? AudienceId { get; set; }

        public Guid? UserId { get; set; }

        public string RefreshValue { get; set; }

        [Required]
        public string RefreshType { get; set; }

        [Required]
        public DateTime ValidFromUtc { get; set; }

        [Required]
        public DateTime ValidToUtc { get; set; }

        [Required]
        public DateTime IssuedUtc { get; set; }
    }

    public class RefreshCreate : Refreshes
    {

    }

    public class RefreshModel : Refreshes
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime Created { get; set; }
    }
}
