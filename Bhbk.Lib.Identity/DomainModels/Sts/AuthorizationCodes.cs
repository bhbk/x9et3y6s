using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class AuthorizationCodes
    {
        [Required]
        public string redirect_uri { get; set; }

        [Required]
        [RegularExpression("authorization_code")]
        public string grant_type { get; set; }

        [Required]
        public string authorization_code { get; set; }

        [Required]
        public string state { get; set; }
    }

    public class AuthorizationCodeV1 : AuthorizationCodes
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }
    }

    public class AuthorizationCodeV2 : AuthorizationCodes
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
