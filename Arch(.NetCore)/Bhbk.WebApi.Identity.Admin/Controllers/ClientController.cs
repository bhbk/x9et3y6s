using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
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
        public IActionResult CreateClientV1([FromBody] ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Clients.Get(x => x.IssuerId == model.IssuerId
                && x.Name == model.Name).Any())
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

            var result = UoW.Clients.Create(Mapper.Map<tbl_Clients>(model));

            UoW.Commit();

            return Ok(Mapper.Map<ClientModel>(result));
        }

        [Route("v1/{clientID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteClientV1([FromRoute] Guid clientID)
        {
            var client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();

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

            UoW.Clients.Delete(client);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{clientID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteClientRefreshesV1([FromRoute] Guid clientID)
        {
            var client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.ClientId == clientID).ToLambda());

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{clientID:guid}/refresh/{refreshID}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteClientRefreshV1([FromRoute] Guid clientID, [FromRoute] Guid refreshID)
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.ClientId == clientID && x.Id == refreshID).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshID}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(expr);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{clientValue}"), HttpGet]
        public IActionResult GetClientV1([FromRoute] string clientValue)
        {
            Guid clientID;
            tbl_Clients client = null;

            if (Guid.TryParse(clientValue, out clientID))
                client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();
            else
                client = UoW.Clients.Get(x => x.Name == clientValue).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ClientModel>(client));
        }

        [Route("v1/{clientID:guid}/set-password"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult SetPasswordV1([FromRoute] Guid clientID, [FromBody] EntityAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            client.ActorId = GetUserGUID();

            if (model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Bad password for client:{client.Id}");
                return BadRequest(ModelState);
            }

            UoW.Clients.SetPassword(client, model.NewPassword);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetClientsV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<ClientModel>
                {
                    Data = Mapper.Map<IEnumerable<ClientModel>>(
                        UoW.Clients.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Clients>, IQueryable<tbl_Clients>>>>(
                                model.ToExpression<tbl_Clients>()),
                            new List<Expression<Func<tbl_Clients, object>>>() { x => x.tbl_Roles })),

                    Total = UoW.Clients.Count(
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
        public IActionResult GetClientRefreshesV1([FromRoute] Guid clientID)
        {
            var client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var refreshes = UoW.Refreshes.Get(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.ClientId == client.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RefreshModel>>(refreshes));
        }

        [Route("v1/{clientID:guid}/roles"), HttpGet]
        public IActionResult GetClientRolesV1([FromRoute] Guid clientID)
        {
            var client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var roles = UoW.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.ClientId == client.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RoleModel>>(roles));
        }

        [Route("v1/{clientID:guid}/urls"), HttpGet]
        public IActionResult GetClientUrlsV1([FromRoute] Guid clientID)
        {
            var client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{clientID}");
                return NotFound(ModelState);
            }

            var urls = UoW.Urls.Get(new QueryExpression<tbl_Urls>()
                .Where(x => x.ClientId == client.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<UrlModel>>(urls));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult UpdateClientV1([FromBody] ClientModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = UoW.Clients.Get(x => x.Id == model.Id).SingleOrDefault();

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

            var result = UoW.Clients.Update(Mapper.Map<tbl_Clients>(model));

            UoW.Commit();

            return Ok(Mapper.Map<ClientModel>(result));
        }
    }
}