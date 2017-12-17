using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class AuthorizationCodeV1Extension
    {
        public static IApplicationBuilder UseAuthorizationCodeV1Provider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthorizationCodeV1Provider>();
        }
    }

    public class AuthorizationCodeV1Provider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AuthorizationCodeV1Provider(RequestDelegate next)
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
                || !context.Request.Form.ContainsKey("redirect_uri")
                || !context.Request.Form.ContainsKey("code"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgSystemParametersInvalid }, _serializer));
            }

            return _next(context);
        }
    }
}
