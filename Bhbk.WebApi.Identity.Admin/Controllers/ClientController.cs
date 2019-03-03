using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
        [Authorize(Policy = "AdministratorPolicy")]
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

            var result = await UoW.ClientRepo.CreateAsync(model);

            await UoW.CommitAsync();

            return Ok(result);
        }

        [Route("v1/{clientID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteClientV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(Strings.MsgClientImmutable);

            client.ActorId = GetUserGUID();

            if (!await UoW.ClientRepo.DeleteAsync(client.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientValue}"), HttpGet]
        public async Task<IActionResult> GetClientV1([FromRoute] string clientValue)
        {
            Guid clientID;
            ClientModel client;

            if (Guid.TryParse(clientValue, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == clientValue)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            return Ok(client);
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetClientsPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<ClientModel, bool>> preds;
            Expression<Func<ClientModel, object>> ords = x => string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2);

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.ClientRepo.Count(preds);
            var result = await UoW.ClientRepo.GetAsync(preds, ords, null, model.Skip, model.Take);

            return Ok(new { Count = total, List = result });
        }

        [Route("v1/{clientID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetClientRolesV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            var roles = await UoW.ClientRepo.GetRoleListAsync(clientID);

            return Ok(roles);
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateClientV1([FromBody] ClientModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(Strings.MsgClientImmutable);

            var result = await UoW.ClientRepo.UpdateAsync(model);

            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}