using Bhbk.Lib.Identity.Models;
using System;

namespace Bhbk.Lib.Identity.Factory
{
    public class LoginFactory<T> : AppLogin
    {
        public LoginFactory(AppLogin login)
        {
            this.Id = login.Id;
            this.LoginProvider = login.LoginProvider;
        }

        public LoginFactory(LoginCreate login)
        {
            this.Id = Guid.NewGuid();
            this.LoginProvider = login.LoginProvider;
        }

        public LoginFactory(LoginUpdate login)
        {
            this.Id = login.Id;
            this.LoginProvider = login.LoginProvider;
        }

        public LoginResult Evolve()
        {
            return new LoginResult
            {
                Id = this.Id,
                LoginProvider = this.LoginProvider
            };
        }

        public AppLogin Devolve()
        {
            return new AppLogin
            {
                Id = this.Id,
                LoginProvider = this.LoginProvider
            };
        }
    }

    public class LoginCreate
    {
        public string LoginProvider { get; set; }
    }

    public class LoginResult
    {
        public Guid Id { get; set; }
        public string LoginProvider { get; set; }
    }

    public class LoginUpdate
    {
        public Guid Id { get; set; }
        public string LoginProvider { get; set; }
    }
}
