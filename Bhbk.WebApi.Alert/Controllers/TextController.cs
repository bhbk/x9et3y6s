using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("text")]
    //[Authorize(Policy = "UsersPolicy")]
    public class TextController : BaseController
    {
        public TextController() { }

        [Route("v1"), HttpPost]
        public async Task<IActionResult> SendTextV1([FromBody] TextCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!(await UoW.UserRepo.GetAsync(x => x.Id == model.FromId
                && x.PhoneNumber == model.FromPhoneNumber)).Any())
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"SenderID:{model.FromId} SenderPhone:{model.FromPhoneNumber}");
                return NotFound(ModelState);
            }

            var queue = (QueueTextTask)Tasks.Single(x => x.GetType() == typeof(QueueTextTask));

            var msg = UoW.Mapper.Map<tbl_QueueTexts>(model);

            if (!queue.TryEnqueueText(msg))
            {
                ModelState.AddModelError(MessageType.TextEnqueueError.ToString(), $"MessageID:{msg.Id} SenderPhone:{model.FromPhoneNumber}");
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
