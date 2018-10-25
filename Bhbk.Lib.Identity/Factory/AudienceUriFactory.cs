using Bhbk.Lib.Identity.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class AudienceUriFactory<T> : AppAudienceUri
    {
        public AudienceUriFactory(AppAudienceUri audienceUri)
        {
            this.Id = audienceUri.Id;
            this.AudienceId = audienceUri.AudienceId;
            this.ActorId = audienceUri.ActorId;
            this.AbsoluteUri = audienceUri.AbsoluteUri;
            this.Enabled = audienceUri.Enabled;
            this.Created = audienceUri.Created;
            this.LastUpdated = audienceUri.LastUpdated;
        }

        public AudienceUriFactory(AudienceUriCreate audienceUri)
        {
            this.Id = Guid.NewGuid();
            this.AudienceId = audienceUri.AudienceId;
            this.ActorId = audienceUri.ActorId;
            this.AbsoluteUri = audienceUri.AbsoluteUri;
            this.Enabled = audienceUri.Enabled;
            this.Created = DateTime.Now;
        }

        public AudienceUriFactory(AudienceUriUpdate audience)
        {
            this.Id = audience.Id;
            this.AudienceId = audience.AudienceId;
            this.ActorId = audience.ActorId;
            this.AbsoluteUri = audience.AbsoluteUri;
            this.Enabled = audience.Enabled;
            this.Created = audience.Created;
            this.LastUpdated = DateTime.Now;
        }

        public AppAudienceUri ToStore()
        {
            return new AppAudienceUri
            {
                Id = this.Id,
                AudienceId = this.AudienceId,
                ActorId = this.ActorId,
                AbsoluteUri = this.AbsoluteUri,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
            };
        }

        public AudienceUriResult ToClient()
        {
            return new AudienceUriResult()
            {
                Id = this.Id,
                AudienceId = this.AudienceId,
                ActorId = this.ActorId,
                AbsoluteUri = this.AbsoluteUri,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
            };
        }
    }

    public abstract class AudienceUriBase
    {
        [Required]
        public Guid AudienceId { get; set; }

        [Required]
        public Guid ActorId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string AbsoluteUri { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; }
    }

    public class AudienceUriCreate : AudienceUriBase { }

    public class AudienceUriResult : AudienceUriBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }

    public class AudienceUriUpdate : AudienceUriBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}