﻿using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Paging.Models;
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
    [Route("role")]
    public class RoleController : BaseController
    {
        private RoleProvider _provider;

        public RoleController(IConfiguration conf, IContextService instance)
        {
            _provider = new RoleProvider(conf, instance);
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

            var create = await UoW.RoleRepo.CreateAsync(Mapper.Map<tbl_Roles>(model));

            await UoW.CommitAsync();

            var result = (await UoW.RoleRepo.GetAsync(x => x.Name == model.Name)).SingleOrDefault();

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(Mapper.Map<RoleModel>(result));
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

            return Ok(Mapper.Map<RoleModel>(role));
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetRolesV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Roles, bool>> predicates;

            if (string.IsNullOrEmpty(model.Filter))
                predicates = x => true;
            else
                predicates = x => x.Name.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.Description.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.RoleRepo.CountAsync(predicates);
                var result = await UoW.RoleRepo.GetAsync(predicates,
                    x => x.Include(r => r.tbl_UserRoles).ThenInclude(r => r.User),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = Mapper.Map<IEnumerable<RoleModel>>(result) });
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

            return Ok(Mapper.Map<IEnumerable<UserModel>>(users));
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

            var result = await UoW.RoleRepo.UpdateAsync(Mapper.Map<tbl_Roles>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<RoleModel>(result));
        }
    }
}
