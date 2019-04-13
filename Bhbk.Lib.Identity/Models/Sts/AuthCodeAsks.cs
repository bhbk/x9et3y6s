using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Sts
{
    public abstract class AuthCodeAsks
    {
        [Required]
        public string redirect_uri { get; set; }

        [Required]
        [DefaultValue("code")]
        [RegularExpression("code")]
        public string response_type { get; set; }

        [Required]
        public string scope { get; set; }
    }

    public class AuthCodeAskV1 : AuthCodeAsks
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }
    }

    public class AuthCodeAskV2 : AuthCodeAsks
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
