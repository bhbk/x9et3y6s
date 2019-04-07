using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class DeviceCodeRequests
    {
        [Required]
        [DefaultValue("device_code")]
        [RegularExpression("device_code")]
        public string grant_type { get; set; }
    }

    public class DeviceCodeRequestV1 : DeviceCodeRequests
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }
    }

    public class DeviceCodeRequestV2 : DeviceCodeRequests
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
