﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class uvw_Activities
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? AudienceId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OriginalValues { get; set; }
        public string CurrentValues { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }
    }
}
