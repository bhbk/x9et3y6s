using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class ResourceOwners
    {
        [Required]
        public string password { get; set; }

        [Required]
        [DefaultValue("password")]
        [RegularExpression("password")]
        public string grant_type { get; set; }
    }

    public class ResourceOwnerV1 : ResourceOwners
    {
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }
    }

    public class ResourceOwnerV2 : ResourceOwners
    {
        [Required]
        public string issuer { get; set; }

        public string client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
