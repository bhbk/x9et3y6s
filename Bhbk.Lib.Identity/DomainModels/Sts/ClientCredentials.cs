using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class ClientCredentials
    {
        [Required]
        [RegularExpression("client_secret")]
        public string grant_type { get; set; }
    }

    public class ClientCredentialV1 : ClientCredentials
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string client_secret { get; set; }
    }

    public class ClientCredentialV2 : ClientCredentials
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }

        [Required]
        public string client_secret { get; set; }
    }
}