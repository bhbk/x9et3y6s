using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    public class ClientCredentialV1
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_secret { get; set; }

        [Required]
        [RegularExpression("client_secret")]
        public string grant_type { get; set; }
    }

    public class ClientCredentialV2
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client_secret { get; set; }

        [Required]
        [RegularExpression("client_secret")]
        public string grant_type { get; set; }
    }
}