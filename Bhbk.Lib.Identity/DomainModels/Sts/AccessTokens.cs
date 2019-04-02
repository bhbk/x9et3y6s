using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class AccessTokens
    {
        [Required]
        public string password { get; set; }

        [Required]
        [RegularExpression("password")]
        public string grant_type { get; set; }
    }

    public class AccessTokenV1 : AccessTokens
    {
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }
    }

    public class AccessTokenV2 : AccessTokens
    {
        [Required]
        public string issuer { get; set; }

        public string client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
