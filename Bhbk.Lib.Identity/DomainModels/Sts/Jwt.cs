﻿using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public class JwtV1Legacy
    {
        [Required]
        public string token_type { get; set; }

        [Required]
        public string access_token { get; set; }

        [Required]
        public int expires_in { get; set; }
    }

    public class JwtV1
    {
        [Required]
        public string token_type { get; set; }

        [Required]
        public string access_token { get; set; }

        [Required]
        public string refresh_token { get; set; }

        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string user_id { get; set; }
    }

    public class JwtV2
    {
        [Required]
        public string token_type { get; set; }

        [Required]
        public string access_token { get; set; }

        [Required]
        public string refresh_token { get; set; }

        [Required]
        public string issuer { get; set; }

        [Required]
        public IList client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
