using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class ClientFactory<T> : AppClient
    {
        public ClientFactory(AppClient client)
        {
            this.Id = client.Id;
            this.ActorId = client.ActorId;
            this.Name = client.Name;
            this.Description = client.Description ?? string.Empty;
            this.ClientKey = client.ClientKey;
            this.Enabled = client.Enabled;
            this.Created = client.Created;
            this.LastUpdated = client.LastUpdated ?? null;
            this.Immutable = client.Immutable;
            this.AppAudience = client.AppAudience;
        }

        public ClientFactory(ClientCreate client)
        {
            this.Id = Guid.NewGuid();
            this.ActorId = client.ActorId;
            this.Name = client.Name;
            this.Description = client.Description ?? string.Empty;
            this.ClientKey = client.ClientKey ?? Helpers.CryptoHelper.CreateRandomBase64(32);
            this.Enabled = client.Enabled;
            this.Created = DateTime.Now;
            this.Immutable = client.Immutable;
        }

        public AppClient Devolve()
        {
            return new AppClient
            {
                ActorId = this.ActorId,
                Id = this.Id,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                ClientKey = this.ClientKey,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable,
                AppAudience = this.AppAudience,
            };
        }

        public ClientResult Evolve()
        {
            return new ClientResult
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                ClientKey = this.ClientKey,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable,
                Audiences = AppAudience.Where(x => x.ClientId == this.Id).Select(x => x.Id.ToString()).ToList(),
            };
        }

        public void Update(ClientUpdate client)
        {
            this.Id = client.Id;
            this.ActorId = client.ActorId;
            this.Name = client.Name;
            this.Description = client.Description;
            this.Enabled = client.Enabled;
            this.LastUpdated = DateTime.Now;
            this.Immutable = client.Immutable;
        }
    }

    public abstract class ClientBase
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string ClientKey { get; set; }

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

        public IList<string> Audiences { get; set; }
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
