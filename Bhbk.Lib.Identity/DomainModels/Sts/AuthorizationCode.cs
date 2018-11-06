using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public class AuthorizationCodeV1
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }

        [Required]
        public string redirect_uri { get; set; }

        [Required]
        [RegularExpression("code")]
        public string grant_type { get; set; }
    }

    public class AuthorizationCodeV2
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }

        [Required]
        public string username { get; set; }

        [Required]
        public string redirect_uri { get; set; }

        [Required]
        [RegularExpression("code")]
        public string grant_type { get; set; }
    }
}
