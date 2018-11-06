using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.EntityModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
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
                return BadRequest(Strings.MsgIssuerAlreadyExists);

            var result = await UoW.IssuerRepo.CreateAsync(UoW.Convert.Map<AppIssuer>(model));

            return Ok(UoW.Convert.Map<IssuerModel>(result));
        }

        [Route("v1/{issuerID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteIssuerV1([FromRoute] Guid issuerID)
        {
            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            else if (issuer.Immutable)
                return BadRequest(Strings.MsgIssuerImmutable);

            issuer.ActorId = GetUserGUID();

            if (!await UoW.IssuerRepo.DeleteAsync(issuer))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{issuerValue}"), HttpGet]
        public async Task<IActionResult> GetIssuerV1([FromRoute] string issuerValue)
        {
            Guid issuerID;
            AppIssuer issuer;

            if (Guid.TryParse(issuerValue, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == issuerValue)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            return Ok(UoW.Convert.Map<IssuerModel>(issuer));
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetIssuersPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<AppIssuer, bool>> expr;

            if (string.IsNullOrEmpty(model.Filter))
                expr = x => true;
            else
                expr = x => x.Name.ToLower().Contains(model.Filter.ToLower())
                || x.Description.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.IssuerRepo.Count(expr);
            var list = await UoW.IssuerRepo.GetAsync(expr,
                x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)).Skip(model.Skip).Take(model.Take),
                x => x.Include(y => y.AppClient));

            var result = list.Select(x => UoW.Convert.Map<IssuerModel>(x));

            return Ok(new { Count = total, List = result });
        }

        [Route("v1/{issuerID:guid}/clients"), HttpGet]
        public async Task<IActionResult> GetIssuerClientsV1([FromRoute] Guid issuerID)
        {
            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            var clients = await UoW.IssuerRepo.GetClientsAsync(issuerID);

            var result = clients.Select(x => UoW.Convert.Map<ClientModel>(x)).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateIssuerV1([FromBody] IssuerUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            else if (issuer.Immutable)
                return BadRequest(Strings.MsgIssuerImmutable);

            var result = await UoW.IssuerRepo.UpdateAsync(UoW.Convert.Map<AppIssuer>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<IssuerModel>(result));
        }
    }
}