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

            if ((await UoW.ClientRepo.GetAsync(x => x.IssuerId == model.IssuerId
                && x.Name == model.Name)).Any())
            {
                ModelState.AddModelError(MsgType.ClientAlreadyExists.ToString(), $"Issuer:{model.IssuerId} Client:{model.Name}");
                return BadRequest(ModelState);
            }

            ClientType clientType;

            if (!Enum.TryParse<ClientType>(model.ClientType, out clientType))
            {
                ModelState.AddModelError(MsgType.ClientInvalid.ToString(), $"Issuer:{model.IssuerId} Client:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.ClientRepo.CreateAsync(model);

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<ClientModel>(result));
        }

        [Route("v1/{clientID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteClientV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }
            else if (client.Immutable)
            {
                ModelState.AddModelError(MsgType.ClientImmutable.ToString(), $"Client:{clientID}");
                return BadRequest(ModelState);
            }

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
            TClients client = null;

            if (Guid.TryParse(clientValue, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == clientValue)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{clientValue}");
                return NotFound(ModelState);
            }

            return Ok(UoW.Transform.Map<ClientModel>(client));
        }

        [Route("v1/page"), HttpGet]
        public async Task<IActionResult> GetClientsV1([FromQuery] SimplePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<TClients, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.ClientRepo.CountAsync(preds);
                var result = await UoW.ClientRepo.GetAsync(preds,
                    x => x.Include(r => r.TRoles),
                    x => x.OrderBy(string.Format("{0} {1}", model.OrderBy, model.Order)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<ClientModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetClientsV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<TClients, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.ClientRepo.CountAsync(preds);
                var result = await UoW.ClientRepo.GetAsync(preds,
                    x => x.Include(r => r.TRoles),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<ClientModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{clientID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetClientRolesV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var roles = await UoW.ClientRepo.GetRoleListAsync(clientID);

            return Ok(roles);
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateClientV1([FromBody] ClientModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{model.Id}");
                return NotFound(ModelState);
            }
            else if (client.Immutable
                && client.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MsgType.ClientImmutable.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.ClientRepo.UpdateAsync(UoW.Transform.Map<TClients>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<ClientModel>(result));
        }
    }
}