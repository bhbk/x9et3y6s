using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class Codes
    {
        [Required]
        public Guid IssuerId { get; set; }

        public Guid? ClientId { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        public string CodeValue { get; set; }

        [Required]
        public string CodeType { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public DateTime ValidFromUtc { get; set; }

        [Required]
        public DateTime ValidToUtc { get; set; }
    }

    public class CodeCreate : Codes
    {

    }

    public class CodeModel : Codes
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }
    }
}
