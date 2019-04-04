using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class AuthCodes
    {
        [Required]
        public string redirect_uri { get; set; }

        [Required]
        [RegularExpression("authorization_code")]
        public string grant_type { get; set; }

        [Required]
        public string code { get; set; }

        [Required]
        public string nonce { get; set; }
    }

    public class AuthCodeV1 : AuthCodes
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }
    }

    public class AuthCodeV2 : AuthCodes
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
