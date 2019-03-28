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
    [Route("text")]
    public class TextController : BaseController
    {
        public TextController() { }

        [Route("v1"), HttpPost]
        public async Task<IActionResult> SendTextV1([FromBody] TextCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Situation == ExecutionType.Live)
            {
                var admin = new AdminClient(Conf, UoW.Situation, new HttpClient());

                if (admin == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                var user = await admin.User_GetV1(Jwt.AccessToken.ToString(), model.ToId.ToString());

                if (!user.IsSuccessStatusCode)
                    return BadRequest(Strings.MsgUserInvalid);

                var recipient = JsonConvert.DeserializeObject<UserModel>(await user.Content.ReadAsStringAsync());
            }

            var queue = ((QueueTextTask)Tasks.Single(x => x.GetType() == typeof(QueueTextTask)));

            if (!queue.TryEnqueueText(model))
                return BadRequest(Strings.MsgSysQueueSmsError);

            return NoContent();
        }
    }
}
