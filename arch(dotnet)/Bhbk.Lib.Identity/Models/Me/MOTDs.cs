using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        [JsonProperty("author")]
        public string author { get; set; }

        [Required]
        [JsonProperty("quote")]
        public string quote { get; set; }
    }

    public class MOTDTssV1 : MOTDs
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("length")]
        public string length { get; set; }

        [JsonProperty("tags")]
        public ICollection<string> tags { get; set; }

        [JsonProperty("category")]
        public string category { get; set; }

        [JsonProperty("date")]
        public string date { get; set; }

        [JsonProperty("title")]
        public string title { get; set; }

        [JsonProperty("background")]
        public string background { get; set; }
    }

    public class MOTDTssV1Response
    {
        [JsonProperty("success")]
        public Success success { get; set; }

        [JsonProperty("contents")]
        public Contents contents { get; set; }

        public class Contents
        {
            [JsonProperty("quotes")]
            public List<MOTDTssV1> quotes { get; set; }
        }

        public class Success
        {
            [JsonProperty("total")]
            public int total { get; set; }
        }
    }
}
