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
    [Route("client")]
    public class ClientController : BaseController
    {
        public ClientController() { }

        public ClientController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClientV1([FromBody] ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.ClientRepo.GetAsync(x => x.Name == model.Name);

            if (check.Any())
                return BadRequest(Strings.MsgClientAlreadyExists);

            var result = await UoW.ClientRepo.CreateAsync(UoW.Convert.Map<AppClient>(model));

            return Ok(UoW.Convert.Map<ClientResult>(result));
        }

        [Route("v1/{clientID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClientV1([FromRoute] Guid clientID)
        {
            var client = await UoW.ClientRepo.GetAsync(clientID);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(Strings.MsgClientImmutable);

            client.ActorId = GetUserGUID();

            if (!await UoW.ClientRepo.DeleteAsync(client))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{clientID:guid}"), HttpGet]
        public async Task<IActionResult> GetClientV1([FromRoute] Guid clientID)
        {
            var client = await UoW.ClientRepo.GetAsync(clientID);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            return Ok(UoW.Convert.Map<ClientResult>(client));
        }

        [Route("v1/{clientName}"), HttpGet]
        public async Task<IActionResult> GetClientV1([FromRoute] string clientName)
        {
            var client = (await UoW.ClientRepo.GetAsync(x => x.Name == clientName)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            return Ok(UoW.Convert.Map<ClientResult>(client));
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetClientsV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clients = await UoW.ClientRepo.GetAsync(x => true,
                x => x.OrderBy(model.OrderBy).Skip(model.Skip).Take(model.Take),
                x => x.Include(y => y.AppAudience));

            var result = clients.Select(x => UoW.Convert.Map<ClientResult>(x));

            return Ok(result);
        }

        [Route("v1/{clientID:guid}/audiences"), HttpGet]
        public async Task<IActionResult> GetClientAudiencesV1([FromRoute] Guid clientID)
        {
            var client = await UoW.ClientRepo.GetAsync(clientID);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            var audiences = await UoW.ClientRepo.GetAudiencesAsync(clientID);

            var result = audiences.Select(x => UoW.Convert.Map<AudienceResult>(x)).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateClientV1([FromBody] ClientUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var client = await UoW.ClientRepo.GetAsync(model.Id);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(Strings.MsgClientImmutable);

            var result = await UoW.ClientRepo.UpdateAsync(UoW.Convert.Map<AppClient>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<ClientResult>(result));
        }
    }
}