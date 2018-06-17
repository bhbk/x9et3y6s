using Bhbk.Lib.Identity.Models;
using System;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class LoginFactory<T> : AppLogin
    {
        public LoginFactory(AppLogin login)
        {
            this.Id = login.Id;
            this.LoginProvider = login.LoginProvider;
            this.Immutable = login.Immutable;
        }

        public LoginFactory(LoginCreate login)
        {
            this.Id = Guid.NewGuid();
            this.LoginProvider = login.LoginProvider;
            this.Immutable = login.Immutable;
        }

        public LoginResult Evolve()
        {
            return new LoginResult
            {
                Id = this.Id,
                LoginProvider = this.LoginProvider,
                Immutable = this.Immutable,
            };
        }

        public AppLogin Devolve()
        {
            return new AppLogin
            {
                Id = this.Id,
                LoginProvider = this.LoginProvider,
                Immutable = this.Immutable,
            };
        }

        public void Update(LoginUpdate login)
        {
            this.Id = login.Id;
            this.LoginProvider = login.LoginProvider;
            this.Immutable = login.Immutable;
        }
    }

    public class LoginCreate
    {
        public string LoginProvider { get; set; }
        public bool Immutable { get; set; }
    }

    public class LoginResult
    {
        public Guid Id { get; set; }
        public string LoginProvider { get; set; }
        public bool Immutable { get; set; }
    }

    public class LoginUpdate
    {
        public Guid Id { get; set; }
        public string LoginProvider { get; set; }
        public bool Immutable { get; set; }
    }
}
