using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class uvw_TextActivity
    {
        public Guid Id { get; set; }
        public Guid TextId { get; set; }
        public string TwilioSid { get; set; }
        public string TwilioStatus { get; set; }
        public string TwilioMessage { get; set; }
        public DateTimeOffset StatusAtUtc { get; set; }
    }
}
