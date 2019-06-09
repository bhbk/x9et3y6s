using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private LoginProvider _provider;

        public LoginController(IConfiguration conf, IContextService instance)
        {
            _provider = new LoginProvider(conf, instance);
        }

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

            var result = await UoW.LoginRepo.CreateAsync(Mapper.Map<tbl_Logins>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<LoginModel>(result));
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

            return Ok(Mapper.Map<LoginModel>(login));
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetLoginsV1([FromBody] DataPagerV3 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Logins, bool>> predicates;

            if (string.IsNullOrEmpty(model.Filter.First().Value))
                predicates = x => true;
            else
                predicates = x => x.Name.Contains(model.Filter.First().Value, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.LoginRepo.CountAsync(predicates);
                var result = await UoW.LoginRepo.GetAsync(predicates,
                    x => x.Include(l => l.tbl_UserLogins).ThenInclude(l => l.User),
                    x => x.OrderBy(string.Format("{0} {1}", model.Sort.First().Field, model.Sort.First().Dir)),
                    model.Skip,
                    model.Take);

                return Ok(new
                {
                    Data = Mapper.Map<IEnumerable<LoginModel>>(result),
                    Total = total
                });
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

            var result = users.Select(x => Mapper.Map<UserModel>(x));

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

            var result = await UoW.LoginRepo.UpdateAsync(Mapper.Map<tbl_Logins>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<LoginModel>(result));
        }
    }
}