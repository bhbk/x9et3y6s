using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class RefreshTokens
    {
        [Required]
        public string refresh_token { get; set; }

        [Required]
        [RegularExpression("refresh_token")]
        public string grant_type { get; set; }
    }

    public class RefreshTokenV1 : RefreshTokens
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }
    }

    public class RefreshTokenV2 : RefreshTokens
    {
        [Required]
        public string issuer { get; set; }

        public string client { get; set; }
    }
}
