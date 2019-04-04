using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class ImplicitRequests
    {
        [Required]
        public string redirect_uri { get; set; }

        [Required]
        [RegularExpression("token")]
        public string response_type { get; set; }

        [Required]
        public string scope { get; set; }

        [Required]
        public string state { get; set; }
    }

    public class ImplicitRequestV1 : ImplicitRequests
    {
        [Required]
        public string issuer_id { get; set; }

        [Required]
        public string client_id { get; set; }

        [Required]
        public string username { get; set; }
    }

    public class ImplicitRequestV2 : ImplicitRequests
    {
        [Required]
        public string issuer { get; set; }

        [Required]
        public string client { get; set; }

        [Required]
        public string user { get; set; }
    }
}
