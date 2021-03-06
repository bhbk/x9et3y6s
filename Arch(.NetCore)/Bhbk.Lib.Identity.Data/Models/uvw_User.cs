﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class uvw_User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHashPBKDF2 { get; set; }
        public string PasswordHashSHA256 { get; set; }
        public bool PasswordConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsHumanBeing { get; set; }
        public bool IsDeletable { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEndUtc { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
