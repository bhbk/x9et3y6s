using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interface;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("client")]
    public class ClientController : BaseController
    {
        public ClientController() { }

        public ClientController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> CreateClient(ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await UoW.ClientMgmt.FindByNameAsync(model.Name);

            if (client == null)
            {
                var create = UoW.ClientMgmt.Store.Mf.Create.DoIt(model);
                var result = await UoW.ClientMgmt.CreateAsync(create);

                return Ok(result);
            }
            else
                return BadRequest(BaseLib.Statics.MsgClientAlreadyExists);
        }

        [Route("v1/{clientID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteClient(Guid clientID)
        {
            var client = await UoW.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            if (!await UoW.ClientMgmt.DeleteAsync(clientID))
                return InternalServerError();

            else
                return Ok();
        }

        [Route("v1/{clientID}"), HttpGet]
        public async Task<IHttpActionResult> GetClient(Guid clientID)
        {
            var client = await UoW.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else
                return Ok(client);
        }

        [Route("v1"), HttpGet]
        public async Task<IHttpActionResult> GetClients()
        {
            return Ok(await UoW.ClientMgmt.GetListAsync());
        }

        [Route("v1/{clientID}/audiences"), HttpGet]
        public async Task<IHttpActionResult> GetClientAudiences(Guid clientID)
        {
            var client = await UoW.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else
                return Ok(await UoW.ClientMgmt.GetAudiencesAsync(clientID));
        }

        [Route("v1/{clientID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateClient(Guid clientID, ClientUpdate model)
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
                var update = UoW.ClientMgmt.Store.Mf.Update.DoIt(model);
                var result = await UoW.ClientMgmt.UpdateAsync(update);

                return Ok(result);
            }
        }
    }
}