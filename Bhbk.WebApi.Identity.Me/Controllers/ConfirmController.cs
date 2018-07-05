using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("confirm")]
    [AllowAnonymous]
    public class ConfirmController : BaseController
    {
        public ConfirmController() { }

        public ConfirmController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }


        [Route("v1/email/{userId}"), HttpPut]
        public async Task<IActionResult> ConfirmEmailV1([FromRoute] Guid userId, [FromBody] string email, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userId.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            if (!await new ProtectProvider(IoC.ContextStatus.ToString()).ValidateAsync(email, token, user))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            await IoC.UserMgmt.Store.SetEmailConfirmedAsync(user, true);

            return NoContent();
        }

        [Route("v1/password/{userId}"), HttpPut]
        public async Task<IActionResult> ConfirmPasswordV1([FromRoute] Guid userId, [FromBody] string password, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userId.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            if (!await new ProtectProvider(IoC.ContextStatus.ToString()).ValidateAsync(password, token, user))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            await IoC.UserMgmt.Store.SetPasswordConfirmedAsync(user, true);

            return NoContent();
        }

        [Route("v1/phone/{userId}"), HttpPut]
        public async Task<IActionResult> ConfirmPhoneV1([FromRoute] Guid userId, [FromBody] string phoneNumber, [FromBody] string token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userId.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            if (!await new TotpProvider(8, 10).ValidateAsync(phoneNumber, token, user))
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            await IoC.UserMgmt.Store.SetPhoneNumberConfirmedAsync(user, true);

            return NoContent();
        }
    }
}
