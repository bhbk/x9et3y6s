using Bhbk.Lib.Identity.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class UserLoginFactory<T> : AppUserLogin
    {
        public UserLoginFactory(AppUserLogin login)
        {
            this.UserId = login.UserId;
            this.LoginId = login.LoginId;
            this.LoginProvider = login.LoginProvider;
            this.ProviderDisplayName = login.ProviderDisplayName ?? string.Empty;
            this.ProviderDescription = login.ProviderDescription ?? string.Empty;
            this.ProviderKey = login.ProviderKey;
            this.Enabled = login.Enabled;
            this.Created = login.Created;
            this.LastUpdated = login.LastUpdated ?? null;
            this.Immutable = login.Immutable;
        }

        public UserLoginFactory(UserLoginCreate login)
        {
            this.UserId = login.UserId;
            this.LoginId = login.LoginId;
            this.LoginProvider = login.LoginProvider;
            this.ProviderDisplayName = login.ProviderDisplayName ?? string.Empty;
            this.ProviderDescription = login.ProviderDescription ?? string.Empty;
            this.ProviderKey = login.ProviderKey;
            this.Enabled = login.Enabled;
            this.Created = DateTime.Now;
            this.Immutable = login.Immutable;
        }

        public UserLoginFactory(UserLoginUpdate login)
        {
            this.LoginId = login.LoginId;
            this.UserId = login.UserId;
            this.LoginProvider = login.LoginProvider;
            this.ProviderDisplayName = login.ProviderDisplayName ?? string.Empty;
            this.ProviderDescription = login.ProviderDescription ?? string.Empty;
            this.ProviderKey = login.ProviderKey;
            this.Enabled = login.Enabled;
            this.LastUpdated = DateTime.Now;
            this.Immutable = login.Immutable;
        }

        public AppUserLogin ToStore()
        {
            return new AppUserLogin
            {
                UserId = this.UserId,
                LoginId = this.LoginId,
                LoginProvider = this.LoginProvider,
                ProviderDisplayName = this.ProviderDisplayName ?? string.Empty,
                ProviderDescription = this.ProviderDescription ?? string.Empty,
                ProviderKey = this.ProviderKey,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable,
            };
        }

        public UserLoginResult ToClient()
        {
            return new UserLoginResult
            {
                UserId = this.UserId,
                LoginId = this.LoginId,
                LoginProvider = this.LoginProvider,
                ProviderDisplayName = this.ProviderDisplayName ?? string.Empty,
                ProviderDescription = this.ProviderDescription ?? string.Empty,
                ProviderKey = this.ProviderKey,
                Enabled = this.Enabled,
                Created = this.Created,
                LastUpdated = this.LastUpdated ?? null,
                Immutable = this.Immutable,
            };
        }
    }

    public abstract class UserLoginBase
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid LoginId { get; set; }

        [Required]
        public string LoginProvider { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ProviderDescription { get; set; }

        public string ProviderKey { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class UserLoginCreate : UserLoginBase { }

    public class UserLoginResult : UserLoginBase
    {
        [Required]
        public DateTime Created { get; set; }

        [Required]
        public Nullable<DateTime> LastUpdated { get; set; }
    }

    public class UserLoginUpdate : UserLoginBase
    {
        [Required]
        public DateTime Created { get; set; }

        [Required]
        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
