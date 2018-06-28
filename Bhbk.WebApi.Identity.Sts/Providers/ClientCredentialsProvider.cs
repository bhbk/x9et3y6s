using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
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
            #region v2 end-point
            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v2/client", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV2)
                || context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV2)
                || context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV2)
                || context.Request.Form.ContainsKey("client_secret")))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgSysNotImplemented }, _serializer));
            }
            #endregion

            return _next(context);
        }
    }
}
