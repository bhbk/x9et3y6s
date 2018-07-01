using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

        public ClientController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClient([FromBody] ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var client = await IoC.ClientMgmt.FindByNameAsync(model.Name);

            if (client != null)
                return BadRequest(BaseLib.Statics.MsgClientAlreadyExists);

            var create = new ClientFactory<ClientCreate>(model);

            var result = await IoC.ClientMgmt.CreateAsync(create.Devolve());

            return Ok(create.Evolve());
        }

        [Route("v1/{clientID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClient([FromRoute] Guid clientID)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            client.ActorId = GetUserGUID();

            if (!await IoC.ClientMgmt.DeleteAsync(client))
                return StatusCode(StatusCodes.Status500InternalServerError);

            else
                return NoContent();
        }

        [Route("v1/{clientID}"), HttpGet]
        public async Task<IActionResult> GetClient([FromRoute] Guid clientID)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var result = new ClientFactory<AppAudience>(client);

            return Ok(result.Evolve());
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetClients([FromQuery] PagingModel model)
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
        public async Task<IActionResult> GetClientAudiences([FromRoute] Guid clientID)
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
        public async Task<IActionResult> UpdateClient([FromBody] ClientUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var client = await IoC.ClientMgmt.FindByIdAsync(model.Id);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            else
            {
                var update = new ClientFactory<AppClient>(client);
                update.Update(model);

                var result = await IoC.ClientMgmt.UpdateAsync(update.Devolve());

                return Ok(update.Evolve());
            }
        }
    }
}