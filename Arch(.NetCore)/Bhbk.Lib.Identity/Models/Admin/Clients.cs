﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Clients
    {
        public Guid ActorId { get; set; }

        [Required]
        public Guid IssuerId { get; set; }

        /*
         * do not allow commas...
         */
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\._\-^%$#!~@?\[\]{}() \\/=+]+$")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string ClientType { get; set; }

        public DateTime Created { get; set; }

        public bool LockoutEnabled { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class ClientCreate : Clients
    {

    }

    public class ClientModel : Clients
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime? LastUpdated { get; set; }

        public Nullable<DateTimeOffset> LockoutEnd { get; set; }

        public Nullable<DateTime> LastLoginFailure { get; set; }

        public Nullable<DateTime> LastLoginSuccess { get; set; }

        public int AccessFailedCount { get; set; }

        public int AccessSuccessCount { get; set; }
    }
}