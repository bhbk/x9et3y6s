using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            HttpStatusCode code;
            var msg = String.Empty;
            var ex = context.Exception.GetType();

            if (ex == typeof(UnauthorizedAccessException))
            {
                code = HttpStatusCode.Unauthorized;
                msg = context.Exception.Message;
            }
            else if (ex == typeof(SecurityTokenDecryptionFailedException)
                || ex == typeof(SecurityTokenEncryptionFailedException)
                || ex == typeof(SecurityTokenEncryptionKeyNotFoundException)
                || ex == typeof(SecurityTokenException)
                || ex == typeof(SecurityTokenExpiredException)
                || ex == typeof(SecurityTokenInvalidAudienceException)
                || ex == typeof(SecurityTokenInvalidIssuerException)
                || ex == typeof(SecurityTokenInvalidLifetimeException)
                || ex == typeof(SecurityTokenInvalidSignatureException)
                || ex == typeof(SecurityTokenInvalidSigningKeyException)
                || ex == typeof(SecurityTokenKeyWrapException)
                || ex == typeof(SecurityTokenNoExpirationException)
                || ex == typeof(SecurityTokenNotYetValidException)
                || ex == typeof(SecurityTokenReplayAddFailedException)
                || ex == typeof(SecurityTokenReplayDetectedException)
                || ex == typeof(SecurityTokenSignatureKeyNotFoundException)
                || ex == typeof(SecurityTokenValidationException))
            {
                code = HttpStatusCode.Unauthorized;
                msg = context.Exception.Message;
            }
            else if (ex == typeof(NotImplementedException)
                || ex == typeof(NullReferenceException))
            {
                code = HttpStatusCode.InternalServerError;
                msg = context.Exception.Message + ":" + context.Exception.StackTrace;
            }
            else
            {
                code = HttpStatusCode.NotFound;
                msg = context.Exception.Message;
            }

            context.ExceptionHandled = true;
            context.HttpContext.Response.StatusCode = (int)code;
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(msg,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                }));
        }
    }
}
