using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
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
    [Route("issuer")]
    public class IssuerController : BaseController
    {
        private IssuerProvider _provider;

        public IssuerController(IConfiguration conf, IContextService instance)
        {
            _provider = new IssuerProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> CreateIssuerV1([FromBody] IssuerCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.Issuers.GetAsync(x => x.Name == model.Name)).Any())
            {
                ModelState.AddModelError(MessageType.IssuerAlreadyExists.ToString(), $"Issuer:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.Issuers.CreateAsync(Mapper.Map<tbl_Issuers>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<IssuerModel>(result));
        }

        [Route("v1/{issuerID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> DeleteIssuerV1([FromRoute] Guid issuerID)
        {
            var issuer = (await UoW.Issuers.GetAsync(x => x.Id == issuerID)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{issuerID}");
                return NotFound(ModelState);
            }
            else if (issuer.Immutable)
            {
                ModelState.AddModelError(MessageType.IssuerImmutable.ToString(), $"Issuer:{issuerID}");
                return BadRequest(ModelState);
            }

            issuer.ActorId = GetUserGUID();

            await UoW.Issuers.DeleteAsync(issuer);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{issuerValue}"), HttpGet]
        public async ValueTask<IActionResult> GetIssuerV1([FromRoute] string issuerValue)
        {
            Guid issuerID;
            tbl_Issuers issuer = null;

            if (Guid.TryParse(issuerValue, out issuerID))
                issuer = (await UoW.Issuers.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.Issuers.GetAsync(x => x.Name == issuerValue)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{issuerValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<IssuerModel>(issuer));
        }

        [Route("v1/page"), HttpPost]
        public async ValueTask<IActionResult> GetIssuersV1([FromBody] PageState model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateResult<IssuerModel>
                {
                    Data = Mapper.Map<IEnumerable<IssuerModel>>(
                        await UoW.Issuers.GetAsync(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Issuers>, IQueryable<tbl_Issuers>>>>(
                                model.ToExpression<tbl_Issuers>()))),

                    Total = await UoW.Issuers.CountAsync(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Issuers>, IQueryable<tbl_Issuers>>>>(
                            model.ToPredicateExpression<tbl_Issuers>()))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{issuerID:guid}/clients"), HttpGet]
        public async ValueTask<IActionResult> GetIssuerClientsV1([FromRoute] Guid issuerID)
        {
            var issuer = (await UoW.Issuers.GetAsync(x => x.Id == issuerID)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{issuerID}");
                return NotFound(ModelState);
            }

            var clients = await UoW.Clients.GetAsync(new QueryExpression<tbl_Clients>()
                .Where(x => x.IssuerId == issuerID).ToLambda());

            return Ok(Mapper.Map<ClientModel>(clients));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> UpdateIssuerV1([FromBody] IssuerModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var issuer = (await UoW.Issuers.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.Id}");
                return NotFound(ModelState);
            }
            else if (issuer.Immutable
                && issuer.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.IssuerImmutable.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.Issuers.UpdateAsync(Mapper.Map<tbl_Issuers>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<IssuerModel>(result));
        }
    }
}