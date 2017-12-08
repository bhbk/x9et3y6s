using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

//https://blogs.ibs.com/2017/12/12/token-based-authentication-using-asp-net-core-2-0/

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class ExtendAccessTokenProviderV1
    {
        public static IApplicationBuilder UseAccessTokenProviderV1(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AccessTokenProviderV1>();
        }
    }

    public class AccessTokenProviderV1
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AccessTokenProviderV1(RequestDelegate next)
        {
            _next = next;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            //check for correct parameters
            if (!context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrUserIDV1)
                || !context.Request.Form.ContainsKey("password"))
                return _next(context);

            var postValues = context.Request.ReadFormAsync().Result;

            string clientValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientIDV1).Value;
            string audienceValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAudienceIDV1).Value;
            string grantTypeValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrGrantTypeIDV1).Value;
            string userValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrUserIDV1).Value;
            string passwordValue = postValues.FirstOrDefault(x => x.Key == "password").Value;

            //check for correct parameter format
            if (string.IsNullOrEmpty(clientValue)
                || string.IsNullOrEmpty(audienceValue)
                || !grantTypeValue.Equals("password")
                || string.IsNullOrEmpty(userValue)
                || string.IsNullOrEmpty(passwordValue))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_values", _serializer));
            }

            Guid clientID, audienceID;
            AppClient client;
            AppAudience audience;

            var ioc = context.RequestServices.GetService<IIdentityContext>();

            if (ioc == null)
                throw new ArgumentNullException();

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = ioc.ClientMgmt.FindByIdAsync(clientID).Result;
            else
                client = ioc.ClientMgmt.FindByNameAsync(clientValue).Result;

            if (client == null || !client.Enabled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
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
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_audience", _serializer));
            }

            var user = ioc.UserMgmt.FindByNameAsync(userValue).Result;

            //check that user exists...
            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
            }

            //check that user is confirmed...
            //check that user is not locked...
            else if (ioc.UserMgmt.IsLockedOutAsync(user).Result
                || !user.EmailConfirmed)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
            }

            var logins = ioc.UserMgmt.GetLoginsAsync(user).Result;

            //check that login provider exists...
            if (logins == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_login", _serializer));
            }

            //check if login provider is local...
            //check if login provider is transient for unit/integration test...
            else if (logins.Contains(BaseLib.Statics.ApiDefaultLogin)
                || (logins.Where(x => x.StartsWith(BaseLib.Statics.ApiUnitTestLogin)).Any() && ioc.ContextStatus == ContextType.UnitTest))
            {
                //check that password is valid...
                if (!ioc.UserMgmt.CheckPasswordAsync(user, passwordValue).Result)
                {
                    ioc.UserMgmt.AccessFailedAsync(user).Wait();

                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_login", _serializer));
            }

            var access = JwtHelperV1.GenerateAccessToken(context, client, audience, user).Result;
            var refresh = JwtHelperV1.GenerateRefreshToken(context, client, audience, user).Result;

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

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
        }
    }
}