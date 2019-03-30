using Bhbk.Lib.Core.DomainModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
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
    [Route("issuer")]
    public class IssuerController : BaseController
    {
        public IssuerController() { }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateIssuerV1([FromBody] IssuerCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.IssuerRepo.GetAsync(x => x.Name == model.Name);

            if (check.Any())
            {
                ModelState.AddModelError(MsgType.IssuerAlreadyExists.ToString(), Strings.MsgIssuerAlreadyExists);
                return BadRequest(ModelState);
            }

            var result = await UoW.IssuerRepo.CreateAsync(model);

            return Ok(UoW.Transform.Map<IssuerModel>(result));
        }

        [Route("v1/{issuerID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteIssuerV1([FromRoute] Guid issuerID)
        {
            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MsgType.IssuerNotFound.ToString(), $"issuerID: { issuerID }");
                return NotFound(ModelState);
            }

            else if (issuer.Immutable)
                return BadRequest(Strings.MsgIssuerImmutable);

            issuer.ActorId = GetUserGUID();

            if (!await UoW.IssuerRepo.DeleteAsync(issuer.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{issuerValue}"), HttpGet]
        public async Task<IActionResult> GetIssuerV1([FromRoute] string issuerValue)
        {
            Guid issuerID;
            TIssuers issuer = null;

            if (Guid.TryParse(issuerValue, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == issuerValue)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            return Ok(UoW.Transform.Map<IssuerModel>(issuer));
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetIssuersPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<TIssuers, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.IssuerRepo.CountAsync(preds);
                var result = await UoW.IssuerRepo.GetAsync(preds,
                    x => x.Include(c => c.TClients),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<IssuerModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.PagerException.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{issuerID:guid}/clients"), HttpGet]
        public async Task<IActionResult> GetIssuerClientsV1([FromRoute] Guid issuerID)
        {
            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            var clients = await UoW.IssuerRepo.GetClientsAsync(issuerID);

            return Ok(clients);
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateIssuerV1([FromBody] IssuerModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            else if (issuer.Immutable)
                return BadRequest(Strings.MsgIssuerImmutable);

            var result = await UoW.IssuerRepo.UpdateAsync(UoW.Transform.Map<TIssuers>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<IssuerModel>(result));
        }
    }
}