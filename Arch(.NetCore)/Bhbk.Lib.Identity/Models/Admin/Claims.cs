﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Claims
    {
        public Guid? ActorId { get; set; }

        [Required]
        public Guid IssuerId { get; set; }

        public string Subject { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Value { get; set; }

        public string ValueType { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsDeletable { get; set; }
    }

    public class ClaimV1 : Claims
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreatedUtc { get; set; }

        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}