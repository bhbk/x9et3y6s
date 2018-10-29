using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppClient
    {

    }

    public abstract class ClientBase
    {
        [Required]
        public Guid IssuerId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string ClientKey { get; set; }

        [Required]
        public string ClientType { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class ClientCreate : ClientBase
    {
        public Guid ActorId { get; set; }
    }

    public class ClientResult : ClientBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }

        public IList<string> Roles { get; set; }
    }

    public class ClientUpdate : ClientBase
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
