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

    public class EntityAddEmail : Emails
    {

    }

    public class EntityChangeEmail : Emails
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string CurrentEmail { get; set; }
    }
}
