using Bhbk.Lib.Core.DomainModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
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
    [Route("role")]
    public class RoleController : BaseController
    {
        public RoleController() { }

        [Route("v1/{roleID:guid}/add/{userID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> AddRoleToUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MsgType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (!await UoW.UserRepo.AddToRoleAsync(user, role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateRoleV1([FromBody] RoleCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.RoleRepo.GetAsync(x => x.ClientId == model.ClientId
                && x.Name == model.Name)).Any())
            {
                ModelState.AddModelError(MsgType.RoleAlreadyExists.ToString(), $"Client:{model.ClientId} Role:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var create = await UoW.RoleRepo.CreateAsync(model);

            await UoW.CommitAsync();

            var result = (await UoW.RoleRepo.GetAsync(x => x.Name == model.Name)).SingleOrDefault();

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(UoW.Transform.Map<RoleModel>(result));
        }

        [Route("v1/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteRoleV1([FromRoute] Guid roleID)
        {
            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MsgType.RoleNotFound.ToString(), $"roleID: { roleID }");
                return NotFound(ModelState);
            }
            else if (role.Immutable)
            {
                ModelState.AddModelError(MsgType.RoleImmutable.ToString(), $"Role:{role.Id}");
                return BadRequest(ModelState);
            }

            role.ActorId = GetUserGUID();

            if (!await UoW.RoleRepo.DeleteAsync(role.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{roleValue}"), HttpGet]
        public async Task<IActionResult> GetRoleV1([FromRoute] string roleValue)
        {
            Guid roleID;
            TRoles role = null;

            if (Guid.TryParse(roleValue, out roleID))
                role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();
            else
                role = (await UoW.RoleRepo.GetAsync(x => x.Name == roleValue)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MsgType.RoleNotFound.ToString(), $"Role:{roleValue}");
                return NotFound(ModelState);
            }

            return Ok(UoW.Transform.Map<RoleModel>(role));
        }

        [Route("v1/pages"), HttpGet]
        public async Task<IActionResult> GetRolesPageV1([FromQuery] SimplePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<TRoles, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.RoleRepo.CountAsync(preds);
                var result = await UoW.RoleRepo.GetAsync(preds,
                    x => x.Include(r => r.TUserRoles),
                    x => x.OrderBy(string.Format("{0} {1}", model.OrderBy, model.Order)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<RoleModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetRolesPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<TRoles, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.RoleRepo.CountAsync(preds);
                var result = await UoW.RoleRepo.GetAsync(preds,
                    x => x.Include(r => r.TUserRoles),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<RoleModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{roleID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetRoleUsersV1([FromRoute] Guid roleID)
        {
            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MsgType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            var users = await UoW.RoleRepo.GetUsersListAsync(role.Id);

            return Ok(UoW.Transform.Map<IEnumerable<UserModel>>(users));
        }

        [Route("v1/{roleID:guid}/remove/{userID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RemoveRoleFromUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MsgType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (!await UoW.UserRepo.RemoveFromRoleAsync(user, role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateRoleV1([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MsgType.RoleNotFound.ToString(), $"Role:{model.Id}");
                return NotFound(ModelState);
            }
            else if (role.Immutable)
            {
                ModelState.AddModelError(MsgType.RoleImmutable.ToString(), $"Role:{role.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.RoleRepo.UpdateAsync(UoW.Transform.Map<TRoles>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<RoleModel>(result));
        }
    }
}
