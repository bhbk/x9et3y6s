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
    [Route("role")]
    public class RoleController : BaseController
    {
        public RoleController() { }

        [Route("v1/{roleID:guid}/add/{userID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> AddRoleToUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (!await UoW.UserRepo.AddRoleAsync(user, role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> CreateRoleV1([FromBody] RoleCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.RoleRepo.GetAsync(x => x.ClientId == model.ClientId
                && x.Name == model.Name)).Any())
            {
                ModelState.AddModelError(MessageType.RoleAlreadyExists.ToString(), $"Client:{model.ClientId} Role:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var create = await UoW.RoleRepo.CreateAsync(UoW.Mapper.Map<tbl_Roles>(model));

            await UoW.CommitAsync();

            var result = (await UoW.RoleRepo.GetAsync(x => x.Name == model.Name)).SingleOrDefault();

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(UoW.Mapper.Map<RoleModel>(result));
        }

        [Route("v1/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteRoleV1([FromRoute] Guid roleID)
        {
            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"roleID: { roleID }");
                return NotFound(ModelState);
            }
            else if (role.Immutable)
            {
                ModelState.AddModelError(MessageType.RoleImmutable.ToString(), $"Role:{role.Id}");
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
            tbl_Roles role = null;

            if (Guid.TryParse(roleValue, out roleID))
                role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();
            else
                role = (await UoW.RoleRepo.GetAsync(x => x.Name == roleValue)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleValue}");
                return NotFound(ModelState);
            }

            return Ok(UoW.Mapper.Map<RoleModel>(role));
        }

        [Route("v1/page"), HttpGet]
        public async Task<IActionResult> GetRolesV1([FromQuery] SimplePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<tbl_Roles, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.Description.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.RoleRepo.CountAsync(preds);
                var result = await UoW.RoleRepo.GetAsync(preds,
                    x => x.Include(r => r.tbl_UserRoles),
                    x => x.OrderBy(string.Format("{0} {1}", model.OrderBy, model.Order)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Mapper.Map<IEnumerable<RoleModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetRolesV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Roles, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.Description.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.RoleRepo.CountAsync(preds);
                var result = await UoW.RoleRepo.GetAsync(preds,
                    x => x.Include(r => r.tbl_UserRoles),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Mapper.Map<IEnumerable<RoleModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{roleID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetRoleUsersV1([FromRoute] Guid roleID)
        {
            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            var users = await UoW.RoleRepo.GetUsersListAsync(role.Id);

            return Ok(UoW.Mapper.Map<IEnumerable<UserModel>>(users));
        }

        [Route("v1/{roleID:guid}/remove/{userID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> RemoveRoleFromUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (!await UoW.UserRepo.RemoveFromRoleAsync(user, role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> UpdateRoleV1([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{model.Id}");
                return NotFound(ModelState);
            }
            else if (role.Immutable
                && role.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.RoleImmutable.ToString(), $"Role:{role.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.RoleRepo.UpdateAsync(UoW.Mapper.Map<tbl_Roles>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Mapper.Map<RoleModel>(result));
        }
    }
}
