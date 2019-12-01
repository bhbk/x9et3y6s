﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Me
{
    public abstract class Emails
    {
        [Required]
        public Guid EntityId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string NewEmail { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string NewEmailConfirm { get; set; }
    }

    public class EmailAddModel : Emails
    {

    }

    public class EmailChangeModel : Emails
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string CurrentEmail { get; set; }
    }
}
