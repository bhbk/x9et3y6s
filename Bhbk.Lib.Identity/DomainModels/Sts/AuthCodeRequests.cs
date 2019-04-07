using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class AuthCodeRequests
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

    public class AuthCodeRequestV1 : AuthCodeRequests
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }
    }

    public class AuthCodeRequestV2 : AuthCodeRequests
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
