using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Factory
{
    public class RoleFactory<T> : AppRole
    {
        public RoleFactory(AppRole role)
        {
            this.Id = role.Id;
            this.AudienceId = role.AudienceId;
            this.Name = role.Name;
            this.Description = role.Description ?? string.Empty;
            this.Enabled = role.Enabled;
            this.Created = role.Created;
            this.LastUpdated = role.LastUpdated ?? null;
            this.Immutable = role.Immutable;
            this.AppUserRole = role.AppUserRole;
        }
        
        public RoleFactory(RoleCreate role)
        {
            this.Id = Guid.NewGuid();
            this.AudienceId = role.AudienceId;
            this.Name = role.Name;
            this.Description = role.Description ?? string.Empty;
            this.Enabled = role.Enabled;
            this.Created = DateTime.Now;
            this.Immutable = false;
        }

        public RoleFactory(RoleUpdate role)
        {
            this.Id = role.Id;
            this.AudienceId = role.AudienceId;
            this.Name = role.Name;
            this.Description = role.Description ?? string.Empty;
            this.Enabled = role.Enabled;
            this.LastUpdated = DateTime.Now;
            this.Immutable = role.Immutable;
        }

        public AppRole Devolve()
        {
            return new AppRole
            {
                Id = this.Id,
                AudienceId = this.AudienceId,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable
            };
        }

        public RoleResult Evolve()
        {
            return new RoleResult
            {
                Id = this.Id,
                AudienceId = this.AudienceId,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable,
                Users = AppUserRole.Where(x => x.RoleId == this.Id).Select(x => x.UserId.ToString()).ToList()                
            };
        }
    }

    public class RoleCreate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
        public Guid AudienceId { get; set; }
    }

    public class RoleResult
    {
        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
        public IList<string> Users { get; set; }
    }

    public class RoleUpdate
    {
        public Guid AudienceId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }
}
