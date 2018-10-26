﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppUserRefresh
    {

    }

    public abstract class UserRefreshBase
    {
        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public Guid AudienceId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string ProtectedTicket { get; set; }

        [Required]
        public DateTime IssuedUtc { get; set; }

        [Required]
        public DateTime ExpiresUtc { get; set; }
    }

    public class UserRefreshCreate : UserRefreshBase { }

    public class UserRefreshResult : UserRefreshBase
    {
        [Required]
        public Guid Id { get; set; }
    }
}
