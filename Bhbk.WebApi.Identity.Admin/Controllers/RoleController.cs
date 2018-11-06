using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.EntityModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
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
                return NotFound(Strings.MsgRoleNotExist);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.UserRepo.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateRoleV1([FromBody] RoleCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = (await UoW.RoleRepo.GetAsync(x => x.Name == model.Name)).SingleOrDefault();

            if (check != null)
                return BadRequest(Strings.MsgRoleAlreadyExists);

            var create = await UoW.RoleRepo.CreateAsync(UoW.Convert.Map<AppRole>(model));

            await UoW.CommitAsync();

            var result = (await UoW.RoleRepo.GetAsync(x => x.Name == model.Name)).SingleOrDefault();

            if(result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(UoW.Convert.Map<RoleModel>(result));
        }

        [Route("v1/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteRoleV1([FromRoute] Guid roleID)
        {
            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(Strings.MsgRoleImmutable);

            role.ActorId = GetUserGUID();

            if (!await UoW.RoleRepo.DeleteAsync(role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{roleValue}"), HttpGet]
        public async Task<IActionResult> GetRoleV1([FromRoute] string roleValue)
        {
            Guid roleID;
            AppRole role;

            if (Guid.TryParse(roleValue, out roleID))
                role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();
            else
                role = (await UoW.RoleRepo.GetAsync(x => x.Name == roleValue)).SingleOrDefault();

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            return Ok(UoW.Convert.Map<RoleModel>(role));
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetRolesPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<AppRole, bool>> expr;

            if (string.IsNullOrEmpty(model.Filter))
                expr = x => true;
            else
                expr = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.RoleRepo.Count(expr);
            var list = await UoW.RoleRepo.GetAsync(expr,
                x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)).Skip(model.Skip).Take(model.Take),
                x => x.Include(y => y.AppUserRole));

            var result = list.Select(x => UoW.Convert.Map<RoleModel>(x));

            return Ok(new { Count = total, List = result });
        }

        [Route("v1/{roleID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetRoleUsersV1([FromRoute] Guid roleID)
        {
            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            var users = await UoW.RoleRepo.GetUsersListAsync(role);

            var result = users.Select(x => UoW.Convert.Map<UserModel>(x));

            return Ok(result);
        }

        [Route("v1/{roleID:guid}/remove/{userID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RemoveRoleFromUserV1([FromRoute] Guid roleID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.UserRepo.RemoveFromRoleAsync(user, role.Name);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateRoleV1([FromBody] RoleUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (role == null)
                return NotFound(Strings.MsgRoleNotExist);

            else if (role.Immutable)
                return BadRequest(Strings.MsgRoleImmutable);

            var result = await UoW.RoleRepo.UpdateAsync(UoW.Convert.Map<AppRole>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<RoleModel>(result));
        }
    }
}
