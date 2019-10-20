using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
        public async ValueTask<IActionResult> CreateClientV1([FromBody] ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.Clients.GetAsync(x => x.IssuerId == model.IssuerId
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

            var result = await UoW.Clients.CreateAsync(Mapper.Map<tbl_Clients>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<ClientModel>(result));
        }

        [Route("v1/{clientID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> DeleteClientV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.Clients.GetAsync(x => x.Id == clientID)).SingleOrDefault();

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

            await UoW.Clients.DeleteAsync(client);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> DeleteClientRefreshesV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.Clients.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            await UoW.Refreshes.DeleteAsync(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.ClientId == clientID).ToLambda());

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientID:guid}/refresh/{refreshID}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> DeleteClientRefreshV1([FromRoute] Guid clientID, [FromRoute] Guid refreshID)
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.ClientId == clientID && x.Id == refreshID).ToLambda();

            if (!await UoW.Refreshes.ExistsAsync(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshID}");
                return NotFound(ModelState);
            }

            await UoW.Refreshes.DeleteAsync(expr);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientValue}"), HttpGet]
        public async ValueTask<IActionResult> GetClientV1([FromRoute] string clientValue)
        {
            Guid clientID;
            tbl_Clients client = null;

            if (Guid.TryParse(clientValue, out clientID))
                client = (await UoW.Clients.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.Clients.GetAsync(x => x.Name == clientValue)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ClientModel>(client));
        }

        [Route("v1/page"), HttpPost]
        public async ValueTask<IActionResult> GetClientsV1([FromBody] PageState model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateResult<ClientModel>
                {
                    Data = Mapper.Map<IEnumerable<ClientModel>>(
                        await UoW.Clients.GetAsync(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Clients>, IQueryable<tbl_Clients>>>>(
                                model.ToExpression<tbl_Clients>()))),

                    Total = await UoW.Clients.CountAsync(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Clients>, IQueryable<tbl_Clients>>>>(
                            model.ToPredicateExpression<tbl_Clients>()))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{clientID:guid}/refreshes"), HttpGet]
        public async ValueTask<IActionResult> GetClientRefreshesV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.Clients.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var refreshes = await UoW.Refreshes.GetAsync(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.ClientId == client.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RefreshModel>>(refreshes));
        }

        [Route("v1/{clientID:guid}/roles"), HttpGet]
        public async ValueTask<IActionResult> GetClientRolesV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.Clients.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var roles = await UoW.Roles.GetAsync(new QueryExpression<tbl_Roles>()
                .Where(x => x.ClientId == client.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RoleModel>>(roles));
        }

        [Route("v1/{clientID:guid}/urls"), HttpGet]
        public async ValueTask<IActionResult> GetClientUrlsV1([FromRoute] Guid clientID)
        {
            var client = (await UoW.Clients.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var urls = await UoW.Urls.GetAsync(new QueryExpression<tbl_Urls>()
                .Where(x => x.ClientId == client.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<UrlModel>>(urls));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> UpdateClientV1([FromBody] ClientModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = (await UoW.Clients.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

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

            var result = await UoW.Clients.UpdateAsync(Mapper.Map<tbl_Clients>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<ClientModel>(result));
        }
    }
}