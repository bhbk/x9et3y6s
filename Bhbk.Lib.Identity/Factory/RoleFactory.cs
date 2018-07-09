using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class RoleFactory<T> : AppRole
    {
        public RoleFactory(AppRole role)
        {
            this.Id = role.Id;
            this.AudienceId = role.AudienceId;
            this.ActorId = role.ActorId;
            this.Name = role.Name;
            this.NormalizedName = role.Name;
            this.Description = role.Description ?? string.Empty;
            this.Enabled = role.Enabled;
            this.Created = role.Created;
            this.LastUpdated = role.LastUpdated ?? null;
            this.Immutable = role.Immutable;
            this.AppRoleClaim = role.AppRoleClaim;
            this.AppUserRole = role.AppUserRole;
        }
        
        public RoleFactory(RoleCreate role)
        {
            this.Id = Guid.NewGuid();
            this.AudienceId = role.AudienceId;
            this.ActorId = role.ActorId;
            this.Name = role.Name;
            this.NormalizedName = role.Name;
            this.Description = role.Description ?? string.Empty;
            this.Enabled = role.Enabled;
            this.Created = DateTime.Now;
            this.Immutable = role.Immutable;
        }

        public AppRole Devolve()
        {
            return new AppRole
            {
                ActorId = this.ActorId,
                Id = this.Id,
                AudienceId = this.AudienceId,
                Name = this.Name,
                NormalizedName = this.Name,
                Description = this.Description ?? string.Empty,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable,
                AppRoleClaim = this.AppRoleClaim,
                AppUserRole = this.AppUserRole,
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
                Claims = AppRoleClaim.Where(x => x.RoleId == this.Id).Select(x => x.RoleId.ToString()).ToList(),
                Users = AppUserRole.Where(x => x.RoleId == this.Id).Select(x => x.UserId.ToString()).ToList(),
            };
        }

        public void Update(RoleUpdate role)
        {
            this.Id = role.Id;
            this.AudienceId = role.AudienceId;
            this.ActorId = role.ActorId;
            this.Name = role.Name;
            this.NormalizedName = role.Name;
            this.Description = role.Description ?? string.Empty;
            this.Enabled = role.Enabled;
            this.LastUpdated = DateTime.Now;
            this.Immutable = role.Immutable;
        }
    }

    public abstract class RoleBase
    {
        [Required]
        public Guid AudienceId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class RoleCreate : RoleBase
    {
        public Guid ActorId { get; set; }
    }

    public class RoleResult : RoleBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }

        public IList<string> Claims { get; set; }

        public IList<string> Users { get; set; }
    }

    public class RoleUpdate : RoleBase
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
