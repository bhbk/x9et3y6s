using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
        public ClaimController() { }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateClaimV1([FromBody] ClaimCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.ClaimRepo.GetAsync(x => x.IssuerId == model.IssuerId
                && x.Type == model.Type);

            if (check.Any())
                return BadRequest(Strings.MsgClaimAlreadyExists);

            var result = await UoW.ClaimRepo.CreateAsync(model);

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<ClaimModel>(result));
        }

        [Route("v1/{clientID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteClaimV1([FromRoute] Guid clientID)
        {
            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();

            if (claim == null)
                return NotFound(Strings.MsgClaimNotExist);

            else if (claim.Immutable)
                return BadRequest(Strings.MsgClaimImmutable);

            claim.ActorId = GetUserGUID();

            if (!await UoW.ClaimRepo.DeleteAsync(claim.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientValue}"), HttpGet]
        public async Task<IActionResult> GetClaimV1([FromRoute] string clientValue)
        {
            Guid claimID;
            AppClaim claim = null;

            if (Guid.TryParse(clientValue, out claimID))
                claim = (await UoW.ClaimRepo.GetAsync(x => x.Id == claimID)).SingleOrDefault();

            if (claim == null)
                return NotFound(Strings.MsgClaimNotExist);

            return Ok(UoW.Transform.Map<ClaimModel>(claim));
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetClientsPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<AppClaim, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Type.ToLower().Contains(model.Filter.ToLower())
                || x.Value.ToLower().Contains(model.Filter.ToLower())
                || x.ValueType.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.ClaimRepo.CountAsync(preds);
            var result = await UoW.ClaimRepo.GetAsync(preds,
                null,
                x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                model.Skip,
                model.Take);

            return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<ClaimModel>>(result) });
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateClaimV1([FromBody] ClaimModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (claim == null)
                return NotFound(Strings.MsgClaimNotExist);

            else if (claim.Immutable)
                return BadRequest(Strings.MsgClaimImmutable);

            var result = await UoW.ClaimRepo.UpdateAsync(UoW.Transform.Map<AppClaim>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<ClaimModel>(result));
        }
    }
}
