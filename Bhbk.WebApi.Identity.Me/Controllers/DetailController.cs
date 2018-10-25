﻿using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("detail")]
    public class DetailController : BaseController
    {
        public DetailController() { }

        public DetailController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetDetailV1()
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.ToClient());
        }

        [Route("v1/claims"), HttpGet]
        public IActionResult GetClaimsV1()
        {
            var user = User.Identity as ClaimsIdentity;

            return Ok(user.Claims);
        }

        [Route("v1/quote-of-day"), HttpGet]
        public IActionResult GetQuoteOfDayV1()
        {
            var task = (MaintainQuotesTask)Tasks.Single(x => x.GetType() == typeof(MaintainQuotesTask));

            return Ok(task.QuoteOfDay);
        }

        [Route("v1/set-password"), HttpPut]
        public async Task<IActionResult> SetPasswordV1([FromBody] UserChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (!await UoW.CustomUserMgr.CheckPasswordAsync(user, model.CurrentPassword))
                return BadRequest(Strings.MsgUserInvalidCurrentPassword);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(Strings.MsgUserInvalidPasswordConfirm);

            if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            user.ActorId = GetUserGUID();

            var remove = await UoW.CustomUserMgr.RemovePasswordAsync(user);

            if (!remove.Succeeded)
                return GetErrorResult(remove);

            var add = await UoW.CustomUserMgr.AddPasswordAsync(user, model.NewPassword);

            if (!add.Succeeded)
                return GetErrorResult(add);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/two-factor/{status}"), HttpPut]
        public async Task<IActionResult> SetTwoFactorV1([FromRoute] bool status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            bool two = await UoW.CustomUserMgr.GetTwoFactorEnabledAsync(user);

            if (two == status)
                return BadRequest(Strings.MsgUserInvalidTwoFactor);

            var result = await UoW.CustomUserMgr.SetTwoFactorEnabledAsync(user, status);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        public async Task<IActionResult> UpdateDetailV1([FromBody] UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(GetUserGUID().ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            if (user.Id != model.Id)
                return BadRequest(Strings.MsgUserInvalid);

            else if (!user.HumanBeing)
                return BadRequest(Strings.MsgUserInvalid);

            var update = new UserFactory<UserUpdate>(model);
            var result = await UoW.CustomUserMgr.UpdateAsync(update.ToStore());

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return Ok(update.ToClient());
        }
    }
}
