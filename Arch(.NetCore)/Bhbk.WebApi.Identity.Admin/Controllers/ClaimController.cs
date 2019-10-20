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
    [Route("claim")]
    public class ClaimController : BaseController
    {
        private ClaimProvider _provider;

        public ClaimController(IConfiguration conf, IContextService instance)
        {
            _provider = new ClaimProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> CreateClaimV1([FromBody] ClaimCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.Claims.GetAsync(new QueryExpression<tbl_Claims>()
                .Where(x => x.IssuerId == model.IssuerId && x.Type == model.Type).ToLambda()))
                .Any())
            {
                ModelState.AddModelError(MessageType.ClaimAlreadyExists.ToString(), $"Issuer:{model.IssuerId} Claim:{model.Type}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.Claims.CreateAsync(Mapper.Map<tbl_Claims>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<ClaimModel>(result));
        }

        [Route("v1/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> DeleteClaimV1([FromRoute] Guid claimID)
        {
            var claim = (await UoW.Claims.GetAsync(new QueryExpression<tbl_Claims>()
                .Where(x => x.Id == claimID).ToLambda()))
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (claim.Immutable)
            {
                ModelState.AddModelError(MessageType.ClaimImmutable.ToString(), $"Claim:{claimID}");
                return BadRequest(ModelState);
            }

            claim.ActorId = GetUserGUID();

            await UoW.Claims.DeleteAsync(claim);
            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{claimValue}"), HttpGet]
        public async ValueTask<IActionResult> GetClaimV1([FromRoute] string claimValue)
        {
            Guid claimID;
            tbl_Claims claim = null;

            if (Guid.TryParse(claimValue, out claimID))
                claim = (await UoW.Claims.GetAsync(new QueryExpression<tbl_Claims>()
                    .Where(x => x.Id == claimID).ToLambda()))
                    .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"claimID: { claimValue }");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ClaimModel>(claim));
        }

        [Route("v1/page"), HttpPost]
        public async ValueTask<IActionResult> GetClaimsV1([FromBody] PageState model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateResult<ClaimModel>
                {
                    Data = Mapper.Map<IEnumerable<ClaimModel>>(
                        await UoW.Claims.GetAsync(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Claims>, IQueryable<tbl_Claims>>>>(
                                model.ToExpression<tbl_Claims>()))),

                    Total = await UoW.Claims.CountAsync(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Claims>, IQueryable<tbl_Claims>>>>(
                            model.ToPredicateExpression<tbl_Claims>()))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async ValueTask<IActionResult> UpdateClaimV1([FromBody] ClaimModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claim = (await UoW.Claims.GetAsync(new QueryExpression<tbl_Claims>()
                .Where(x => x.Id == model.Id).ToLambda()))
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{model.Id}");
                return NotFound(ModelState);
            }
            else if (claim.Immutable
                && claim.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.ClaimImmutable.ToString(), $"Claim:{claim.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.Claims.UpdateAsync(Mapper.Map<tbl_Claims>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<ClaimModel>(result));
        }
    }
}
