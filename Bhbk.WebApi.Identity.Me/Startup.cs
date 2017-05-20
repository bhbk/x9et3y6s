using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Repository;
using Elmah.Contrib.WebApi;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
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

[assembly: OwinStartup(typeof(Bhbk.WebApi.Identity.Me.Startup))]
namespace Bhbk.WebApi.Identity.Me
{
    public class Startup
    {
        //http://www.vannevel.net/2015/03/21/how-to-unit-test-your-owin-configured-oauth2-implementation/
        public virtual HttpConfiguration ConfigureDependencyInjection()
        {
            HttpConfiguration config = new HttpConfiguration();
            UnityContainer container = new UnityContainer();
            CustomIdentityDbContext context = new CustomIdentityDbContext();

            container.RegisterType<IdentityDbContext<AppUser, AppRole, Guid, AppUserLogin, AppUserRole, AppUserClaim>, CustomIdentityDbContext>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppAudience, Guid>, AudienceRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppClient, Guid>, ClientRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppRealm, Guid>, RealmRepository>(new TransientLifetimeManager());
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
            ConfigureOAuthTokenConsumption(app);

            app.UseWebApi(config);
        }

        private void ConfigureWebApi(HttpConfiguration config)
        {
            config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());
            config.MapHttpAttributeRoutes();

            var format = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            format.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            var issuer = "Bhbk";

            try
            {
                var injectConfig = ConfigureDependencyInjection();
                var injectUoW = (IUnitOfWork)injectConfig.DependencyResolver.GetService(typeof(IUnitOfWork));
                var audiences = injectUoW.AudienceRepository.Get(x => x.Name.StartsWith("Bhbk.WebApi.") || x.Name.StartsWith("Bhbk.WebUi."));

                app.CreatePerOwinContext<IUnitOfWork>(UnitOfWork.Create);

                app.UseJwtBearerAuthentication(
                    new JwtBearerAuthenticationOptions
                    {
                        AuthenticationMode = AuthenticationMode.Active,
                        AllowedAudiences = audiences.Select(x => x.Id.ToString().ToLower()),
                        IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                        {
                            new SymmetricKeyIssuerSecurityTokenProvider(issuer, audiences.Select(x => TextEncodings.Base64Url.Decode(x.AudienceKey)))
                        }
                    });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }
    }
}