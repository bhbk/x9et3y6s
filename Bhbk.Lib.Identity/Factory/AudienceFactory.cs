using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib = Bhbk.Lib.Identity;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class AudienceFactory<T> : AppAudience
    {
        public AudienceFactory(AppAudience audience)
        {
            this.Id = audience.Id;
            this.ClientId = audience.ClientId;
            this.Name = audience.Name;
            this.Description = audience.Description ?? string.Empty;
            this.AudienceType = audience.AudienceType ?? BaseLib.AudienceType.user_agent.ToString();
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
            this.Name = audience.Name;
            this.Description = audience.Description ?? string.Empty;
            this.AudienceType = audience.AudienceType ?? BaseLib.AudienceType.user_agent.ToString();
            this.Created = DateTime.Now;
            this.Enabled = audience.Enabled;
            this.Immutable = false;
        }

        public AppAudience Devolve()
        {
            return new AppAudience
            {
                Id = this.Id,
                ClientId = this.ClientId,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                AudienceType = this.AudienceType,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable
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
            this.Name = audience.Name;
            this.Description = audience.Description ?? string.Empty;
            this.AudienceType = audience.AudienceType;
            this.LastUpdated = DateTime.Now;
            this.Enabled = audience.Enabled;
            this.Immutable = audience.Immutable;
        }
    }

    public class AudienceCreate
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceType { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class AudienceResult
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceType { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class AudienceUpdate
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceType { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }
}
