using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models.Me
{
    public abstract class MsgOfTheDay
    {
        public string id { get; set; }

        public string quote { get; set; }

        public string author { get; set; }
    }

    public class MOTDType1Response
    {
        public Success success { get; set; }

        public Contents contents { get; set; }

        public class Contents
        {
            public List<MOTDType1Model> quotes { get; set; }
        }

        public class Success
        {
            public int total { get; set; }
        }
    }

    public class MOTDType1Model : MsgOfTheDay
    {
        public string length { get; set; }

        public List<string> tags { get; set; }

        public string category { get; set; }

        public string date { get; set; }

        public string title { get; set; }

        public string background { get; set; }
    }
}
