using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
using Elmah.Contrib.WebApi;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Unity;
using Unity.Lifetime;
using BaseLib = Bhbk.Lib.Identity;

//https://tools.ietf.org/html/rfc6749

//https://docs.microsoft.com/en-us/aspnet/identity/overview/features-api/account-confirmation-and-password-recovery-with-aspnet-identity
//https://docs.microsoft.com/en-us/aspnet/identity/overview/features-api/two-factor-authentication-using-sms-and-email-with-aspnet-identity

//http://bitoftech.net/2015/01/21/asp-net-identity-2-with-asp-net-web-api-2-accounts-management/
//http://bitoftech.net/2015/02/03/asp-net-identity-2-accounts-confirmation-password-user-policy-configuration/
//http://bitoftech.net/2015/02/16/implement-oauth-json-web-tokens-authentication-in-asp-net-web-api-and-identity-2/
//http://bitoftech.net/2015/03/11/asp-net-identity-2-1-roles-based-authorization-authentication-asp-net-web-api/
//http://bitoftech.net/2015/03/31/asp-net-web-api-claims-authorization-with-asp-net-identity-2-1/

//http://johnatten.com/2015/01/19/asp-net-web-api-understanding-owinkatana-authenticationauthorization-part-i-concepts/
//http://johnatten.com/2015/01/25/asp-net-web-api-understanding-owinkatana-authenticationauthorization-part-ii-models-and-persistence/
//http://johnatten.com/2015/02/15/asp-net-web-api-understanding-owinkatana-authenticationauthorization-part-iii-adding-identity/

[assembly: OwinStartup(typeof(Bhbk.WebApi.Identity.Admin.Startup))]
namespace Bhbk.WebApi.Identity.Admin
{
    public class Startup
    {
        //http://www.vannevel.net/2015/03/21/how-to-unit-test-your-owin-configured-oauth2-implementation/
        public virtual HttpConfiguration ConfigureDependencyInjection()
        {
            HttpConfiguration config = new HttpConfiguration();
            UnityContainer container = new UnityContainer();

            container.RegisterType<IUnitOfWork, UnitOfWork>(new TransientLifetimeManager());
            container.RegisterInstance(new UnitOfWork());
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
                var audiences = injectUoW.AudienceMgmt.Store.Get(x => x.Name.StartsWith(BaseLib.Statics.ApiUnitTestAudience)
                    || x.Name.StartsWith("Bhbk.WebApi.") || x.Name.StartsWith("Bhbk.WebUi."));

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