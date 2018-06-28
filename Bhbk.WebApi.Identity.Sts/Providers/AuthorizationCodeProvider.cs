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
    public static class AuthorizationCodeExtension
    {
        public static IApplicationBuilder UseAuthorizationCodeProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthorizationCodeProvider>();
        }
    }

    public class AuthorizationCodeProvider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AuthorizationCodeProvider(RequestDelegate next)
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
            if (context.Request.Path.Equals("/oauth/v2/authorize", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV2)
                || context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV2)
                || context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV2)
                || context.Request.Form.ContainsKey("redirect_uri")
                || context.Request.Form.ContainsKey("code")))
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
