using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.OAuthProviders
{
    public class AuthorizationCodeProviderV2
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AuthorizationCodeProviderV2(RequestDelegate next)
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
            if (!context.Request.Path.Equals("/oauth/v2/authorize", StringComparison.Ordinal))
                return _next(context);

            // exit if not POST method
            if (!context.Request.Method.Equals("POST"))
                return _next(context);

            // exit if not application/x-www-form-urlencoded
            if (!context.Request.HasFormContentType)
                return _next(context);

            // exit if not include required keys
            if (!context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV2)
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
