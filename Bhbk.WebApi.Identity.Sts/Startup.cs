﻿using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Repository;
using Elmah.Contrib.WebApi;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

//http://bitoftech.net/2015/01/21/asp-net-identity-2-with-asp-net-web-api-2-accounts-management/
//http://bitoftech.net/2015/02/03/asp-net-identity-2-accounts-confirmation-password-user-policy-configuration/
//http://bitoftech.net/2015/02/16/implement-oauth-json-web-tokens-authentication-in-asp-net-web-api-and-identity-2/
//http://bitoftech.net/2015/03/11/asp-net-identity-2-1-roles-based-authorization-authentication-asp-net-web-api/
//http://bitoftech.net/2015/03/31/asp-net-web-api-claims-authorization-with-asp-net-identity-2-1/

//http://johnatten.com/2015/01/19/asp-net-web-api-understanding-owinkatana-authenticationauthorization-part-i-concepts/
//http://johnatten.com/2015/01/25/asp-net-web-api-understanding-owinkatana-authenticationauthorization-part-ii-models-and-persistence/
//http://johnatten.com/2015/02/15/asp-net-web-api-understanding-owinkatana-authenticationauthorization-part-iii-adding-identity/

//https://tools.ietf.org/html/rfc6749

[assembly: OwinStartup(typeof(Bhbk.WebApi.Identity.Sts.Startup))]
namespace Bhbk.WebApi.Identity.Sts
{
    public class Startup
    {
        private OAuthAuthorizationServerOptions _oauthServerOptions { get; set; }
        private OAuthBearerAuthenticationOptions _oauthBearerOptions { get; set; }

        private readonly ConcurrentDictionary<string, string> _codes =
                new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        //http://www.vannevel.net/2015/03/21/how-to-unit-test-your-owin-configured-oauth2-implementation/
        public virtual HttpConfiguration ConfigureDependencyInjection()
        {
            HttpConfiguration config = new HttpConfiguration();
            UnityContainer container = new UnityContainer();
            CustomIdentityDbContext context = new CustomIdentityDbContext();

            container.RegisterType<IdentityDbContext<AppUser, AppRole, Guid, AppUserProvider, AppUserRole, AppUserClaim>, CustomIdentityDbContext>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppAudience, Guid>, AudienceRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppClient, Guid>, ClientRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppProvider, Guid>, ProviderRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppRole, Guid>, RoleRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppUser, Guid>, UserRepository>(new TransientLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new TransientLifetimeManager());
            container.RegisterInstance(context);
            container.RegisterInstance(new UnitOfWork(context));
            config.DependencyResolver = new CustomDependencyResolver(container);

            return config;
        }

        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            ConfigureWebApi(config);
            ConfigureOAuthAuthorization(app);

            app.UseWebApi(config);
        }

        public void ConfigureWebApi(HttpConfiguration config)
        {
            config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());
            config.MapHttpAttributeRoutes();

            var format = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            format.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public void ConfigureOAuthAuthorization(IAppBuilder app)
        {
            var issuer = "Bhbk";

            try
            {
                var injectConfig = ConfigureDependencyInjection();
                var injectUoW = (IUnitOfWork)injectConfig.DependencyResolver.GetService(typeof(IUnitOfWork));
                
                app.CreatePerOwinContext<IUnitOfWork>(UnitOfWork.Create);

                _oauthServerOptions = new OAuthAuthorizationServerOptions()
                {
#if DEBUG
                    AllowInsecureHttp = true,
                    ApplicationCanDisplayErrors = true,
#else
                    AllowInsecureHttp = false,
                    ApplicationCanDisplayErrors = false,
#endif
                    AuthorizeEndpointPath = new PathString("/oauth/v1/authorize"),
                    TokenEndpointPath = new PathString("/oauth/v1/token"),

                    Provider = new Provider.CustomAuthorizationServer(injectUoW),
                    AccessTokenFormat = new Provider.CustomSecureDataFormat(issuer, injectUoW),
                    AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),

                    AuthorizationCodeProvider = new Provider.CustomAuthorizationCode(injectUoW),
                    AuthorizationCodeFormat = new Provider.CustomSecureDataFormat(issuer, injectUoW),
                    AuthorizationCodeExpireTimeSpan = TimeSpan.FromDays(7),

                    RefreshTokenProvider = new Provider.CustomRefreshToken(injectUoW),
                    //RefreshTokenFormat = new Provider.CustomSecureDataFormat(issuer, injectUoW),
                };

                app.UseOAuthAuthorizationServer(_oauthServerOptions);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _codes[context.Token] = context.SerializeTicket();
        }

        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;

            if (_codes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
            }
        }
    }
}