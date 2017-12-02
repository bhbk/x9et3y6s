using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.OAuthProviders
{
    public class RefreshTokenProviderV1
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public RefreshTokenProviderV1(RequestDelegate next)
        {
            _next = next;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            //check if correct path
            if (!context.Request.Path.Equals("/oauth/v1/token", StringComparison.Ordinal))
                return _next(context);

            //check if POST method
            if (!context.Request.Method.Equals("POST"))
                return _next(context);

            //check for application/x-www-form-urlencoded
            if (!context.Request.HasFormContentType)
                return _next(context);

            //check for correct parameter
            if (!context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV1)
                || !context.Request.Form.ContainsKey("refresh_token"))
                return _next(context);

            var postValues = context.Request.ReadFormAsync().Result;

            string clientValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientIDV1).Value;
            string audienceValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAudienceIDV1).Value;
            string grantTypeValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrGrantTypeIDV1).Value;
            string refreshTokenValue = postValues.FirstOrDefault(x => x.Key == "refresh_token").Value;

            //check for correct parameter format
            if (string.IsNullOrEmpty(clientValue)
                || string.IsNullOrEmpty(audienceValue)
                || !grantTypeValue.Equals("refresh_token")
                || string.IsNullOrEmpty(refreshTokenValue))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_values", _serializer));
            }

            var ioc = context.RequestServices.GetService<IIdentityContext>();

            if (ioc == null)
                throw new ArgumentNullException();

            var current = ioc.UserMgmt.FindRefreshTokenAsync(refreshTokenValue).Result;

            if (current == null)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
            }

            else if (current.IssuedUtc >= DateTime.UtcNow || current.ExpiresUtc <= DateTime.UtcNow)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_timestamps", _serializer));
            }

            Guid clientID, audienceID;
            ClientModel client;
            AudienceModel audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = ioc.ClientMgmt.FindByIdAsync(clientID).Result;
            else
                client = ioc.ClientMgmt.FindByNameAsync(clientValue).Result;

            if (client == null || !client.Enabled)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_client", _serializer));
            }

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = ioc.AudienceMgmt.FindByIdAsync(audienceID).Result;
            else
                audience = ioc.AudienceMgmt.FindByNameAsync(audienceValue).Result;

            if (audience == null || !audience.Enabled)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_audience", _serializer));
            }

            var user = ioc.UserMgmt.FindByIdAsync(current.UserId).Result;

            //check that user exists...
            if (user == null)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
            }

            //check that user is not locked...
            else if (ioc.UserMgmt.IsLockedOutAsync(user.Id).Result)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
            }

            var access = JwtHelperV2.GenerateAccessToken(context, client, audience, user).Result;
            var refresh = JwtHelperV2.GenerateRefreshToken(context, client, audience, user).Result;

            var result = new
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                client_id = client.Id.ToString(),
                audience_id = audience.Id.ToString(),
                user = user.Id.ToString(),
                issued = access.begin,
                expires = access.end
            };

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
        }
    }
}
