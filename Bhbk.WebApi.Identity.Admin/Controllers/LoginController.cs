using Bhbk.Lib.Core.DomainModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("login")]
    public class LoginController : BaseController
    {
        public LoginController() { }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateLoginV1([FromBody] LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.LoginRepo.GetAsync(x => x.Name == model.Name);

            if (check.Any())
            {
                ModelState.AddModelError(MsgType.LoginAlreadyExists.ToString(), Strings.MsgLoginAlreadyExists);
                return BadRequest(ModelState);
            }

            var result = await UoW.LoginRepo.CreateAsync(model);

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<LoginModel>(result));
        }

        [Route("v1/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteLoginV1([FromRoute] Guid loginID)
        {
            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MsgType.LoginNotFound.ToString(), $"loginID: { loginID }");
                return NotFound(ModelState);
            }

            else if (login.Immutable)
                return BadRequest(Strings.MsgLoginImmutable);

            login.ActorId = GetUserGUID();

            if (!await UoW.LoginRepo.DeleteAsync(login.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{loginValue}"), HttpGet]
        public async Task<IActionResult> GetLoginV1([FromRoute] string loginValue)
        {
            Guid loginID;
            TLogins login = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(loginValue, out loginID))
                login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();
            else
                login = (await UoW.LoginRepo.GetAsync(x => x.Name == loginValue)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            return Ok(UoW.Transform.Map<LoginModel>(login));
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetLoginsPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<TLogins, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.LoginRepo.CountAsync(preds);
                var result = await UoW.LoginRepo.GetAsync(preds,
                    x => x.Include(l => l.TUserLogins),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<LoginModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.PagerException.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{loginID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetLoginUsersV1([FromRoute] Guid loginID)
        {
            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var users = await UoW.LoginRepo.GetUsersAsync(loginID);

            var result = users.Select(x => UoW.Transform.Map<UserModel>(x));

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateLoginV1([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var result = await UoW.LoginRepo.UpdateAsync(UoW.Transform.Map<TLogins>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<LoginModel>(result));
        }
    }
}