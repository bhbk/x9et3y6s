using AutoMapper;
using Bhbk.Lib.Identity.Data.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

/*
 * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware
 */

namespace Bhbk.WebApi.Identity.Sts.Middlewares
{
    public static class SandboxMiddlewareExtension
    {
        public static IApplicationBuilder UseSandboxMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SandboxMiddleware>();
        }
    }

    public class SandboxMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public SandboxMiddleware(RequestDelegate next)
        {
            _next = next;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            var mapper = context.RequestServices.GetRequiredService<IMapper>();
            var uow = context.RequestServices.GetRequiredService<IUoWService>();

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/ropg-sandbox", StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(RealConstants.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrAudienceIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrResourceOwnerIDV1)))
            {
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrIssuerIDV1).Value;
                string audienceValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrAudienceIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrResourceOwnerIDV1).Value;
            }

            return _next(context);
        }
    }
}