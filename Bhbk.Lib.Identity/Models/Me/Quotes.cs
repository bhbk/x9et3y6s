using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models.Me
{
    public class Quotes
    {
        public Success success { get; set; }

        public Contents contents { get; set; }

        public class Contents
        {
            public List<Quote> quotes { get; set; }
        }

        public class Quote
        {
            public string quote { get; set; }

            public string length { get; set; }

            public string author { get; set; }

            public List<string> tags { get; set; }

            public string category { get; set; }

            public string date { get; set; }

            public string title { get; set; }

            public string background { get; set; }

            public string id { get; set; }
        }

        public class Success
        {
            public int total { get; set; }
        }
    }
}
