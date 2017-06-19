using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("client")]
    [Authorize(Roles = "(Built-In) Administrators")]
    public class ClientController : BaseController
    {
        public ClientController() { }

        public ClientController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpPost]
        public async Task<IHttpActionResult> CreateClient(ClientModel.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await UoW.ClientMgmt.FindByNameAsync(model.Name);

            if (client == null)
            {
                var result = await UoW.ClientMgmt.CreateAsync(model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(await UoW.ClientMgmt.FindByNameAsync(model.Name));
            }
            else
                return BadRequest(BaseLib.Statics.MsgClientAlreadyExists);
        }

        [Route("v1/{clientID}"), HttpDelete]
        public async Task<IHttpActionResult> DeleteClient(Guid clientID)
        {
            var client = await UoW.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            else
            {
                var result = await UoW.ClientMgmt.DeleteAsync(clientID);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{clientID}"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetClient(Guid clientID)
        {
            var client = await UoW.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else
                return Ok(client);
        }

        [Route("v1"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetClientList()
        {
            return Ok(await UoW.ClientMgmt.GetListAsync());
        }

        [Route("v1/{clientID}/audiences"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetClientAudiences(Guid clientID)
        {
            var client = await UoW.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else
                return Ok(await UoW.ClientMgmt.GetAudiencesAsync(clientID));
        }

        [Route("v1/{clientID}"), HttpPut]
        public async Task<IHttpActionResult> UpdateClient(Guid clientID, ClientModel.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (clientID != model.Id)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var client = await UoW.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            else
            {
                client.Name = model.Name;
                client.Description = model.Description;
                client.Enabled = model.Enabled;
                client.Immutable = false;

                IdentityResult result = await UoW.ClientMgmt.UpdateAsync(model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(client);
            }
        }
    }
}