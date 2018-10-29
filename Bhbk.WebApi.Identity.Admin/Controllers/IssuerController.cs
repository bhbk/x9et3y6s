using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("issuer")]
    public class IssuerController : BaseController
    {
        public IssuerController() { }

        public IssuerController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateIssuerV1([FromBody] IssuerCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.IssuerRepo.GetAsync(x => x.Name == model.Name);

            if (check.Any())
                return BadRequest(Strings.MsgIssuerAlreadyExists);

            var result = await UoW.IssuerRepo.CreateAsync(UoW.Convert.Map<AppIssuer>(model));

            return Ok(UoW.Convert.Map<IssuerResult>(result));
        }

        [Route("v1/{issuerID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteIssuerV1([FromRoute] Guid issuerID)
        {
            var client = await UoW.IssuerRepo.GetAsync(issuerID);

            if (client == null)
                return NotFound(Strings.MsgIssuerNotExist);

            else if (client.Immutable)
                return BadRequest(Strings.MsgIssuerImmutable);

            client.ActorId = GetUserGUID();

            if (!await UoW.IssuerRepo.DeleteAsync(client))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{issuerID:guid}"), HttpGet]
        public async Task<IActionResult> GetIssuerV1([FromRoute] Guid issuerID)
        {
            var client = await UoW.IssuerRepo.GetAsync(issuerID);

            if (client == null)
                return NotFound(Strings.MsgIssuerNotExist);

            return Ok(UoW.Convert.Map<IssuerResult>(client));
        }

        [Route("v1/{issuerName}"), HttpGet]
        public async Task<IActionResult> GetIssuerV1([FromRoute] string issuerName)
        {
            var client = (await UoW.IssuerRepo.GetAsync(x => x.Name == issuerName)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgIssuerNotExist);

            return Ok(UoW.Convert.Map<IssuerResult>(client));
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetIssuerV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clients = await UoW.IssuerRepo.GetAsync(x => true,
                x => x.OrderBy(model.OrderBy).Skip(model.Skip).Take(model.Take),
                x => x.Include(y => y.AppClient));

            var result = clients.Select(x => UoW.Convert.Map<IssuerResult>(x));

            return Ok(result);
        }

        [Route("v1/{issuerID:guid}/clients"), HttpGet]
        public async Task<IActionResult> GetIssuerClientsV1([FromRoute] Guid issuerID)
        {
            var client = await UoW.IssuerRepo.GetAsync(issuerID);

            if (client == null)
                return NotFound(Strings.MsgIssuerNotExist);

            var clients = await UoW.IssuerRepo.GetClientsAsync(issuerID);

            var result = clients.Select(x => UoW.Convert.Map<ClientResult>(x)).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateIssuerV1([FromBody] IssuerUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var client = await UoW.IssuerRepo.GetAsync(model.Id);

            if (client == null)
                return NotFound(Strings.MsgIssuerNotExist);

            else if (client.Immutable)
                return BadRequest(Strings.MsgIssuerImmutable);

            var result = await UoW.IssuerRepo.UpdateAsync(UoW.Convert.Map<AppIssuer>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<IssuerResult>(result));
        }
    }
}