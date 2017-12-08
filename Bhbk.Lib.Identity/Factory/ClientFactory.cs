using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Factory
{
    public class ClientFactory<T> : AppClient
    {
        public ClientFactory(AppClient client)
        {
            this.Id = client.Id;
            this.Name = client.Name;
            this.Description = client.Description ?? string.Empty;
            this.Enabled = client.Enabled;
            this.Created = client.Created;
            this.LastUpdated = client.LastUpdated ?? null;
            this.Immutable = client.Immutable;
        }

        public ClientFactory(ClientCreate client)
        {
            this.Id = Guid.NewGuid();
            this.Name = client.Name;
            this.Description = client.Description ?? string.Empty;
            this.Enabled = client.Enabled;
            this.Created = DateTime.Now;
            this.Immutable = false;
        }

        public ClientFactory(ClientUpdate client)
        {
            this.Id = client.Id;
            this.Name = client.Name;
            this.Description = client.Description;
            this.Enabled = client.Enabled;
            this.LastUpdated = DateTime.Now;
            this.Immutable = client.Immutable;
        }

        public AppClient Devolve()
        {
            return new AppClient
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable
            };
        }

        public ClientResult Evolve()
        {
            return new ClientResult
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description ?? string.Empty,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable
            };
        }
    }

    public class ClientCreate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class ClientResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
        public IList<AudienceResult> Audiences { get; set; }
    }

    public class ClientUpdate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }
}
