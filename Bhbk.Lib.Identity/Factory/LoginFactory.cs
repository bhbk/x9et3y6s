using Bhbk.Lib.Identity.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class LoginFactory<T> : AppLogin
    {
        public LoginFactory(AppLogin login)
        {
            this.Id = login.Id;
            this.ActorId = login.ActorId;
            this.LoginProvider = login.LoginProvider;
            this.Immutable = login.Immutable;
        }

        public LoginFactory(LoginCreate login)
        {
            this.Id = Guid.NewGuid();
            this.ActorId = login.ActorId;
            this.LoginProvider = login.LoginProvider;
            this.Immutable = login.Immutable;
        }

        public LoginFactory(LoginUpdate login)
        {
            this.Id = login.Id;
            this.ActorId = login.ActorId;
            this.LoginProvider = login.LoginProvider;
            this.Immutable = login.Immutable;
        }

        public AppLogin ToStore()
        {
            return new AppLogin
            {
                ActorId = this.ActorId,
                Id = this.Id,
                LoginProvider = this.LoginProvider,
                Immutable = this.Immutable,
            };
        }

        public LoginResult ToClient()
        {
            return new LoginResult
            {
                Id = this.Id,
                LoginProvider = this.LoginProvider,
                Immutable = this.Immutable,
            };
        }
    }

    public abstract class LoginBase
    {
        [Required]
        public string LoginProvider { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class LoginCreate : LoginBase
    {
        public Guid ActorId { get; set; }
    }

    public class LoginResult : LoginBase
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class LoginUpdate : LoginBase
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }
    }
}
