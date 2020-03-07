using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Me
{
    public abstract class MOTDs
    {
        [Required]
        public Guid globalId { get; set; }

        [Required]
        public string author { get; set; }

        [Required]
        public string quote { get; set; }
    }

    public class MOTDTssV1 : MOTDs
    {
        public string id { get; set; }

        public string length { get; set; }

        public List<string> tags { get; set; }

        public string category { get; set; }

        public string date { get; set; }

        public string title { get; set; }

        public string background { get; set; }
    }

    public class MOTDTssV1Response
    {
        public Success success { get; set; }

        public Contents contents { get; set; }

        public class Contents
        {
            public List<MOTDTssV1> quotes { get; set; }
        }

        public class Success
        {
            public int total { get; set; }
        }
    }
}
