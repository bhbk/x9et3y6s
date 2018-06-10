using Bhbk.Lib.Identity.Models;
using System;

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
            this.Immutable = false;
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
            this.Immutable = false;
        }

        public AppUserLogin Devolve()
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
                Immutable = this.Immutable
            };
        }

        public UserLoginResult Evolve()
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
                Immutable = this.Immutable
            };
        }

        public void Update(UserLoginUpdate login)
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
    }

    public class UserLoginCreate
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderKey { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class UserLoginResult
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }

    public class UserLoginUpdate
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderKey { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }
}
