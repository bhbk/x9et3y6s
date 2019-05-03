using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
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
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> CreateLoginV1([FromBody] LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.LoginRepo.GetAsync(x => x.Name == model.Name)).Any())
            {
                ModelState.AddModelError(MessageType.LoginAlreadyExists.ToString(), $"Login:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.LoginRepo.CreateAsync(UoW.Mapper.Map<tbl_Logins>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Mapper.Map<LoginModel>(result));
        }

        [Route("v1/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteLoginV1([FromRoute] Guid loginID)
        {
            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login: { loginID }");
                return NotFound(ModelState);
            }
            else if (login.Immutable)
            {
                ModelState.AddModelError(MessageType.LoginImmutable.ToString(), $"Login:{login.Id}");
                return BadRequest(ModelState);
            }

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
            tbl_Logins login = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(loginValue, out loginID))
                login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();
            else
                login = (await UoW.LoginRepo.GetAsync(x => x.Name == loginValue)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginValue}");
                return NotFound(ModelState);
            }

            return Ok(UoW.Mapper.Map<LoginModel>(login));
        }

        [Route("v1/page"), HttpGet]
        public async Task<IActionResult> GetLoginsV1([FromQuery] SimplePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<tbl_Logins, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.LoginRepo.CountAsync(preds);
                var result = await UoW.LoginRepo.GetAsync(preds,
                    x => x.Include(l => l.tbl_UserLogins),
                    x => x.OrderBy(string.Format("{0} {1}", model.OrderBy, model.Order)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Mapper.Map<IEnumerable<LoginModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }

        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetLoginsV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Logins, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.LoginRepo.CountAsync(preds);
                var result = await UoW.LoginRepo.GetAsync(preds,
                    x => x.Include(l => l.tbl_UserLogins),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Mapper.Map<IEnumerable<LoginModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{loginID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetLoginUsersV1([FromRoute] Guid loginID)
        {
            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            var users = await UoW.LoginRepo.GetUsersAsync(loginID);

            var result = users.Select(x => UoW.Mapper.Map<UserModel>(x));

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> UpdateLoginV1([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{model.Id}");
                return NotFound(ModelState);
            }
            else if (login.Immutable
                && login.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.LoginImmutable.ToString(), $"Client:{login.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.LoginRepo.UpdateAsync(UoW.Mapper.Map<tbl_Logins>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Mapper.Map<LoginModel>(result));
        }
    }
}