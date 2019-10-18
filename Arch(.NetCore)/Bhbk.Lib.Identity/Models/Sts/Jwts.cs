using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Sts
{
    public class ClientJwtV1 : Jwts
    {
        [Required]
        public string refresh_token { get; set; }

        public string issuer_id { get; set; }

        public string client_id { get; set; }
    }

    public class ClientJwtV2 : Jwts
    {
        [Required]
        public string refresh_token { get; set; }

        public string issuer { get; set; }

        public string client { get; set; }
    }

    public abstract class Jwts
    {
        [Required]
        public string token_type { get; set; }

        [Required]
        public string access_token { get; set; }

        public int expires_in { get; set; }
    }

    public class UserJwtV1Legacy : Jwts
    {

    }

    public class UserJwtV1 : Jwts
    {
        [Required]
        public string refresh_token { get; set; }

        public string issuer_id { get; set; }

        public string client_id { get; set; }

        public string user_id { get; set; }
    }

    public class UserJwtV2 : Jwts
    {
        [Required]
        public string refresh_token { get; set; }

        public string issuer { get; set; }

        public IList client { get; set; }

        public string user { get; set; }
    }
}
