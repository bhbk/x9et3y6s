using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Alert;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("email")]
    public class EmailController : BaseController
    {
        public EmailController() { }

        [Route("v1"), HttpPost]
        public async Task<IActionResult> SendEmailV1([FromBody] EmailCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Situation == ExecutionType.Live)
            {
                var admin = new AdminClient(Conf, ExecutionType.Live, new HttpClient());

                if (admin == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                var user = await admin.User_GetV1(Jwt.AccessToken.ToString(), model.ToId.ToString());

                if (!user.IsSuccessStatusCode)
                    return BadRequest(Strings.MsgUserInvalid);

                var recipient = JsonConvert.DeserializeObject<UserModel>(await user.Content.ReadAsStringAsync());
            }

            var queue = ((QueueEmailTask)Tasks.Single(x => x.GetType() == typeof(QueueEmailTask)));

            if (!queue.TryEnqueueEmail(model))
                return BadRequest(Strings.MsgSysQueueEmailError);

            return NoContent();
        }
    }
}
