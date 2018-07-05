using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class ClientCredentialsExtension
    {
        public static IApplicationBuilder UseClientCredentialsProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ClientCredentialsProvider>();
        }
    }

    public class ClientCredentialsProvider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public ClientCredentialsProvider(RequestDelegate next)
        {
            _next = next;

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            #region v1 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v1/client", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV1)
                || context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV1)
                || context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientSecretIDV1)))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgSysNotImplemented }, _serializer));
            }

            #endregion

            #region v2 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v2/client", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV2)
                || context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV2)
                || context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientSecretIDV2)))
            {
                var formValues = context.Request.ReadFormAsync().Result;

                string clientValue = formValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientIDV2).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrGrantTypeIDV2).Value;
                string secretValue = formValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientSecretIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
                    || !grantTypeValue.Equals(BaseLib.Statics.AttrGrantTypeIDV2)
                    || string.IsNullOrEmpty(secretValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgSysParamsInvalid }, _serializer));
                }

                var ioc = context.RequestServices.GetRequiredService<IIdentityContext>();

                if (ioc == null)
                    throw new ArgumentNullException();
            }

            #endregion

            return _next(context);
        }
    }
}
