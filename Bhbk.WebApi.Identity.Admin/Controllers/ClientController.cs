using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
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
    [Route("client")]
    public class ClientController : BaseController
    {
        private ClientProvider _provider;

        public ClientController(IConfiguration conf, IContextService instance)
        {
            _provider = new ClientProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> CreateClientV1([FromBody] ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.ClientRepo.GetAsync(x => x.IssuerId == model.IssuerId
                && x.Name == model.Name)).Any())
            {
                ModelState.AddModelError(MessageType.ClientAlreadyExists.ToString(), $"Issuer:{model.IssuerId} Client:{model.Name}");
                return BadRequest(ModelState);
            }

            ClientType clientType;

            if (!Enum.TryParse<ClientType>(model.ClientType, true, out clientType))
            {
                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Issuer:{model.IssuerId} Client:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.ClientRepo.CreateAsync(Mapper.Map<tbl_Clients>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<ClientModel>(result));
        }

        [Route("v1/{clientID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteClientV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }
            else if (client.Immutable)
            {
                ModelState.AddModelError(MessageType.ClientImmutable.ToString(), $"Client:{clientID}");
                return BadRequest(ModelState);
            }

            client.ActorId = GetUserGUID();

            if (!await UoW.ClientRepo.DeleteAsync(client.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteClientRefreshesV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            foreach (var refreshes in await UoW.RefreshRepo.GetAsync(x => x.ClientId == clientID))
            {
                if (!await UoW.RefreshRepo.DeleteAsync(refreshes.Id))
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientID:guid}/refresh/{refreshID}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteClientRefreshV1([FromRoute] Guid clientID, [FromRoute] Guid refreshID)
        {
            var refresh = (await UoW.RefreshRepo.GetAsync(x => x.ClientId == clientID
                && x.Id == refreshID)).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{clientID}");
                return NotFound(ModelState);
            }

            if (!await UoW.RefreshRepo.DeleteAsync(refresh.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientValue}"), HttpGet]
        public async Task<IActionResult> GetClientV1([FromRoute] string clientValue)
        {
            Guid clientID;
            tbl_Clients client = null;

            if (Guid.TryParse(clientValue, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == clientValue)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ClientModel>(client));
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetClientsV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Clients, bool>> predicates;

            if (string.IsNullOrEmpty(model.Filter))
                predicates = x => true;
            else
                predicates = x => x.Name.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.Description.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.ClientRepo.CountAsync(predicates);
                var result = await UoW.ClientRepo.GetAsync(predicates,
                    x => x.Include(r => r.tbl_Roles),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = Mapper.Map<IEnumerable<ClientModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{clientID:guid}/refreshes"), HttpGet]
        public async Task<IActionResult> GetClientRefreshesV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var refreshes = await UoW.RefreshRepo.GetAsync(x => x.ClientId == clientID);

            var result = refreshes.Select(x => Mapper.Map<RefreshModel>(x));

            return Ok(result);
        }

        [Route("v1/{clientID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetClientRolesV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var roles = await UoW.ClientRepo.GetRolesAsync(clientID);

            return Ok(roles);
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> UpdateClientV1([FromBody] ClientModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = (await UoW.ClientRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{model.Id}");
                return NotFound(ModelState);
            }
            else if (client.Immutable
                && client.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.ClientImmutable.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.ClientRepo.UpdateAsync(Mapper.Map<tbl_Clients>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<ClientModel>(result));
        }
    }
}