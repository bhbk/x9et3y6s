using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Models;
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
    [Route("client")]
    public class ClientController : BaseController
    {
        public ClientController() { }

        [Route("v1"), HttpPost] 
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClientV1([FromBody] ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.ClientRepo.GetAsync(x => x.Name == model.Name);

            if (check.Any())
                return BadRequest(Strings.MsgClientAlreadyExists);

            Enums.ClientType clientType;

            if (!Enum.TryParse<Enums.ClientType>(model.ClientType, out clientType))
                return BadRequest(Strings.MsgClientInvalid);

            var result = await UoW.ClientRepo.CreateAsync(UoW.Convert.Map<AppClient>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<ClientResult>(result));
        }

        [Route("v1/{clientID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClientV1([FromRoute] Guid clientID)
        {
            var client = await UoW.ClientRepo.GetAsync(clientID);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(Strings.MsgClientImmutable);

            client.ActorId = GetUserGUID();

            if (!await UoW.ClientRepo.DeleteAsync(client))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientValue}"), HttpGet]
        public async Task<IActionResult> GetClientV1([FromRoute] string clientValue)
        {
            Guid clientID;
            AppClient client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = await UoW.ClientRepo.GetAsync(clientID);
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == clientValue)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            return Ok(UoW.Convert.Map<ClientResult>(client));
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetClientsPageV1([FromBody] TuplePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<AppClient, bool>> expr;

            if (string.IsNullOrEmpty(model.Filter))
                expr = x => true;
            else
                expr = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.ClientRepo.Count(expr);
            var list = await UoW.ClientRepo.GetAsync(expr,
                x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)).Skip(model.Skip).Take(model.Take),
                x => x.Include(y => y.AppRole));

            var result = list.Select(x => UoW.Convert.Map<ClientResult>(x));

            return Ok(new { Count = total, List = result });
        }

        [Route("v1/{clientID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetClientRolesV1([FromRoute] Guid clientID)
        {
            var client = await UoW.ClientRepo.GetAsync(clientID);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            var roles = await UoW.ClientRepo.GetRoleListAsync(clientID);

            var result = roles.Select(x => UoW.Convert.Map<RoleResult>(x)).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateClientV1([FromBody] ClientUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var client = await UoW.ClientRepo.GetAsync(model.Id);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(Strings.MsgClientImmutable);

            var result = await UoW.ClientRepo.UpdateAsync(UoW.Convert.Map<AppClient>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<ClientResult>(result));
        }
    }
}