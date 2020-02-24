using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models.Me
{
    public abstract class MOTDs
    {
        public string id { get; set; }

        public string quote { get; set; }

        public string author { get; set; }
    }

    public class MOTDCreate : MOTDs
    {

    }

    public class MOTDModel : MOTDs
    {
        public string length { get; set; }

        public List<string> tags { get; set; }

        public string category { get; set; }

        public string date { get; set; }

        public string title { get; set; }

        public string background { get; set; }
    }

    public class MOTDType1Response
    {
        public Success success { get; set; }

        public Contents contents { get; set; }

        public class Contents
        {
            public List<MOTDModel> quotes { get; set; }
        }

        public class Success
        {
            public int total { get; set; }
        }
    }
}
