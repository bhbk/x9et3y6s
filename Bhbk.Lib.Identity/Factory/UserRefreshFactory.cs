using Bhbk.Lib.Identity.Models;
using System;
using System.ComponentModel.DataAnnotations;
using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class UserRefreshFactory<T> : AppUserRefresh
    {
        public UserRefreshFactory(AppUserRefresh refresh)
        {
            this.Id = refresh.Id;
            this.ClientId = refresh.ClientId;
            this.UserId = refresh.UserId;
            this.ProtectedTicket = refresh.ProtectedTicket;
            this.IssuedUtc = refresh.IssuedUtc;
            this.ExpiresUtc = refresh.ExpiresUtc;
        }

        public UserRefreshFactory(UserRefreshCreate refresh)
        {
            this.Id = Guid.NewGuid();
            this.ClientId = refresh.ClientId;
            this.UserId = refresh.UserId;
            this.ProtectedTicket = refresh.ProtectedTicket;
            this.IssuedUtc = refresh.IssuedUtc;
            this.ExpiresUtc = refresh.ExpiresUtc;
        }

        public AppUserRefresh Devolve()
        {
            return new AppUserRefresh
            {
                Id = this.Id,
                ClientId = this.ClientId,
                UserId = this.UserId,
                ProtectedTicket = this.ProtectedTicket,
                IssuedUtc = this.IssuedUtc,
                ExpiresUtc = this.ExpiresUtc,
            };
        }

        public UserRefreshResult Evolve()
        {
            return new UserRefreshResult
            {
                Id = this.Id,
                ClientId = this.ClientId,
                UserId = this.UserId,
                ProtectedTicket = this.ProtectedTicket,
                IssuedUtc = this.IssuedUtc,
                ExpiresUtc = this.ExpiresUtc,
            };
        }
    }

    public class UserRefreshCreate
    {
        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public Guid AudienceId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string ProtectedTicket { get; set; }

        [Required]
        public DateTime IssuedUtc { get; set; }

        [Required]
        public DateTime ExpiresUtc { get; set; }
    }

    public class UserRefreshResult
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid AudienceId { get; set; }
        public Guid UserId { get; set; }
        public string ProtectedTicket { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
    }
}
