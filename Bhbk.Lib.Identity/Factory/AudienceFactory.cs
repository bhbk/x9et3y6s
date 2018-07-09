using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class AudienceFactory<T> : AppAudience
    {
        public AudienceFactory(AppAudience audience)
        {
            this.Id = audience.Id;
            this.ClientId = audience.ClientId;
            this.ActorId = audience.ActorId;
            this.Name = audience.Name;
            this.Description = audience.Description ?? string.Empty;
            this.AudienceType = audience.AudienceType;
            this.Created = audience.Created;
            this.LastUpdated = audience.LastUpdated ?? null;
            this.Enabled = audience.Enabled;
            this.Immutable = audience.Immutable;
            this.AppRole = audience.AppRole;
        }

        public AudienceFactory(AudienceCreate audience)
        {
            this.Id = Guid.NewGuid();
            this.ClientId = audience.ClientId;
            this.ActorId = audience.ActorId;
            this.Name = audience.Name;
            this.Description = audience.Description ?? string.Empty;
            this.AudienceType = audience.AudienceType;
            this.Created = DateTime.Now;
            this.Enabled = audience.Enabled;
            this.Immutable = audience.Immutable;
        }

        public AppAudience Devolve()
        {
            return new AppAudience
            {
                Id = this.Id,
                ClientId = this.ClientId,
                ActorId = this.ActorId,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                AudienceType = this.AudienceType,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable,
                AppRole = this.AppRole,
            };
        }

        public AudienceResult Evolve()
        {
            return new AudienceResult()
            {
                Id = this.Id,
                ClientId = this.ClientId,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                AudienceType = this.AudienceType,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable,
                Roles = AppRole.Where(x => x.AudienceId == this.Id).Select(x => x.Id.ToString()).ToList()
            };
        }

        public void Update(AudienceUpdate audience)
        {
            this.Id = audience.Id;
            this.ClientId = audience.ClientId;
            this.ActorId = audience.ActorId;
            this.Name = audience.Name;
            this.Description = audience.Description ?? string.Empty;
            this.AudienceType = audience.AudienceType;
            this.LastUpdated = DateTime.Now;
            this.Enabled = audience.Enabled;
            this.Immutable = audience.Immutable;
        }
    }

    public abstract class AudienceBase
    {
        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string AudienceType { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class AudienceCreate : AudienceBase
    {
        public Guid ActorId { get; set; }
    }

    public class AudienceResult : AudienceBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }

        public IList<string> Roles { get; set; }
    }

    public class AudienceUpdate : AudienceBase
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
