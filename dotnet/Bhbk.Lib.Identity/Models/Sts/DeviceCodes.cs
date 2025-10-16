using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Sts
{
    public abstract class DeviceCodes
    {
        [Required]
        public string device_code { get; set; }

        [Required]
        public string user_code { get; set; }

        [Required]
        [DefaultValue("device_code")]
        [RegularExpression("device_code")]
        public string grant_type { get; set; }

        public string verification_url { get; set; }

        public uint interval { get; set; }
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
