using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class ClientCredentialExtension
    {
        public static IApplicationBuilder UseClientCredentialProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ClientCredentialProvider>();
        }
    }

    public class ClientCredentialProvider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public ClientCredentialProvider(RequestDelegate next)
        {
            _next = next;

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            #region v2 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v2/client", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrIssuerIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrClientSecretIDV2)))
            {
                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrIssuerIDV2).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV2).Value;
                string secretValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientSecretIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || !grantTypeValue.Equals(Strings.AttrClientSecretIDV2)
                    || string.IsNullOrEmpty(secretValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysParamsInvalid }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IIdentityContext<AppDbContext>>();

                if (uow == null)
                    throw new ArgumentNullException();

                Guid issuerID;
                AppIssuer issuer;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(issuerValue, out issuerID))
                    issuer = uow.IssuerRepo.GetAsync(issuerID).Result;
                else
                    issuer = (uow.IssuerRepo.GetAsync(x => x.Name == issuerValue).Result).Single();

                if (issuer == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgIssuerNotExist }, _serializer));
                }

                if (!issuer.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgIssuerInvalid }, _serializer));
                }

                //provider not implemented yet...
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysNotImplemented }, _serializer));
            }

            #endregion

            #region v1 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/client", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrClientSecretIDV1)))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysNotImplemented }, _serializer));
            }

            #endregion

            return _next(context);
        }
    }
}
