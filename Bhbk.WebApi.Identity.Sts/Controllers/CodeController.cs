using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("code")]
    [AllowAnonymous]
    public class CodeController : BaseController
    {
        public CodeController() { }

        public CodeController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v2"), HttpGet]
        public async Task<IActionResult> GetCodeV2([FromQuery(Name = "client")] Guid clientID,
            [FromQuery(Name = "user")] Guid userID,
            [FromQuery(Name = "response_type")] string responseType,
            [FromQuery(Name = "redirect_uri")] string redirectUri,
            [FromQuery(Name = "scope")] string scope)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var state = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(32);
            var link = UrlBuilder.UiAuthorizeCodeRequest(Conf, client, user, redirectUri, scope, state);

            /*
             * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-2.1#cookies
             * 
             * do some more stuff here...
             */

            var cookie = new CookieOptions { Expires = DateTime.Now.AddSeconds(IoC.ConfigMgmt.Store.DefaultsBrowserCookieExpire) };

            return Ok(link.AbsoluteUri);
        }
    }
}
