using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.OAuthProviders
{
    public class AuthorizationCodeProviderV1
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AuthorizationCodeProviderV1(RequestDelegate next)
        {
            _next = next;

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            // exit if path does not match
            if (!context.Request.Path.Equals("/oauth/v1/authorize", StringComparison.Ordinal))
                return _next(context);

            // exit if not POST method
            if (!context.Request.Method.Equals("POST"))
                return _next(context);

            // exit if not application/x-www-form-urlencoded
            if (!context.Request.HasFormContentType)
                return _next(context);

            // exit if not include required keys
            if (!context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV1)
                || !context.Request.Form.ContainsKey("redirect_uri")
                || !context.Request.Form.ContainsKey("code"))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_values", _serializer));
            }

            return _next(context);
        }
    }
}
