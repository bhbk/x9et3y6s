using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("client")]
    public class ClientController : BaseController
    {
        public ClientController() { }

        public ClientController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClientV1([FromBody] ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await IoC.ClientMgmt.FindByNameAsync(model.Name);

            if (check != null)
                return BadRequest(BaseLib.Statics.MsgClientAlreadyExists);

            var client = new ClientFactory<ClientCreate>(model);

            var result = await IoC.ClientMgmt.CreateAsync(client.Devolve());

            return Ok(client.Evolve());
        }

        [Route("v1/{clientID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClientV1([FromRoute] Guid clientID)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            client.ActorId = GetUserGUID();

            if (!await IoC.ClientMgmt.DeleteAsync(client))
                return StatusCode(StatusCodes.Status500InternalServerError);

            return NoContent();
        }

        [Route("v1/{clientID:guid}"), HttpGet]
        public async Task<IActionResult> GetClientV1([FromRoute] Guid clientID)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var result = new ClientFactory<AppAudience>(client);

            return Ok(result.Evolve());
        }

        [Route("v1/{clientName}"), HttpGet]
        public async Task<IActionResult> GetClientV1([FromRoute] string clientName)
        {
            var client = await IoC.ClientMgmt.FindByNameAsync(clientName);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var result = new ClientFactory<AppAudience>(client);

            return Ok(result.Evolve());
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetClientsV1([FromQuery] PagingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clients = IoC.ClientMgmt.Store.Get()
                .OrderBy(model.OrderBy)
                .Skip(Convert.ToInt32((model.PageNumber - 1) * model.PageSize))
                .Take(Convert.ToInt32(model.PageSize));

            var result = clients.Select(x => new ClientFactory<AppClient>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{clientID}/audiences"), HttpGet]
        public async Task<IActionResult> GetClientAudiencesV1([FromRoute] Guid clientID)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var audiences = await IoC.ClientMgmt.GetAudiencesAsync(clientID);

            var result = audiences.Select(x => new AudienceFactory<AppAudience>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateClientV1([FromBody] ClientUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var client = await IoC.ClientMgmt.FindByIdAsync(model.Id);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            var update = new ClientFactory<AppClient>(client);
            update.Update(model);

            var result = await IoC.ClientMgmt.UpdateAsync(update.Devolve());

            return Ok(update.Evolve());
        }
    }
}