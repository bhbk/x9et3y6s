using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Sts
{
    public abstract class Implicits
    {
        [Required]
        public string access_token { get; set; }

        [Required]
        public string expires_in { get; set; }

        [Required]
        [RegularExpression("implicit")]
        public string grant_type { get; set; }

        [Required]
        public string state { get; set; }
    }

    public class ImplicitV1 : Implicits
    {

    }

    public class ImplicitV2 : Implicits
    {

    }
}
