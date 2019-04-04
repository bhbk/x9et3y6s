﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class DeviceCodes
    {
        [Required]
        public string device_code { get; set; }

        [Required]
        public string user_code { get; set; }

        [Required]
        [RegularExpression("device_code")]
        public string grant_type { get; set; }
    }

    public class DeviceCodeV1 : DeviceCodes
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }
    }

    public class DeviceCodeV2 : DeviceCodes
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }
    }
}
