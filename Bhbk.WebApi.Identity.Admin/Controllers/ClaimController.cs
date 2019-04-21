using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Paging.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
        public async Task<IActionResult> CreateClaimV1([FromBody] ClaimCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.ClaimRepo.GetAsync(x => x.IssuerId == model.IssuerId
                && x.Type == model.Type)).Any())
            {
                ModelState.AddModelError(MessageType.ClaimAlreadyExists.ToString(), $"Issuer:{model.IssuerId} Claim:{model.Type}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.ClaimRepo.CreateAsync(Mapper.Map<tbl_Claims>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<ClaimModel>(result));
        }

        [Route("v1/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteClaimV1([FromRoute] Guid claimID)
        {
            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Id == claimID)).SingleOrDefault();

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

            if (!await UoW.ClaimRepo.DeleteAsync(claim.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{claimValue}"), HttpGet]
        public async Task<IActionResult> GetClaimV1([FromRoute] string claimValue)
        {
            Guid claimID;
            tbl_Claims claim = null;

            if (Guid.TryParse(claimValue, out claimID))
                claim = (await UoW.ClaimRepo.GetAsync(x => x.Id == claimID)).SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"claimID: { claimValue }");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ClaimModel>(claim));
        }

        [Route("v1/page"), HttpGet]
        public async Task<IActionResult> GetClaimsV1([FromQuery] SimplePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Claims, bool>> predicates;

            if (string.IsNullOrEmpty(model.Filter))
                predicates = x => true;
            else
                predicates = x => x.Type.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.Value.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.ValueType.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.ClaimRepo.CountAsync(predicates);
                var result = await UoW.ClaimRepo.GetAsync(predicates,
                    null,
                    x => x.OrderBy(string.Format("{0} {1}", model.OrderBy, model.Order)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = Mapper.Map<IEnumerable<ClaimModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetClaimsV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Claims, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Type.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.Value.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.ValueType.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.ClaimRepo.CountAsync(preds);
                var result = await UoW.ClaimRepo.GetAsync(preds,
                    null,
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = Mapper.Map<IEnumerable<ClaimModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> UpdateClaimV1([FromBody] ClaimModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

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

            var result = await UoW.ClaimRepo.UpdateAsync(Mapper.Map<tbl_Claims>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<ClaimModel>(result));
        }
    }
}
