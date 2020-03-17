using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Domain.Providers.Alert;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("text")]
    public class TextController : BaseController
    {
        private TextProvider _provider;

        public TextController(IConfiguration conf, IContextService instance)
        {
            _provider = new TextProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        public IActionResult SendTextV1([FromBody] TextV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UoW.Users.Get(x => x.Id == model.FromId
                && x.PhoneNumber == model.FromPhoneNumber).Any())
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"SenderID:{model.FromId} SenderPhone:{model.FromPhoneNumber}");
                return NotFound(ModelState);
            }

            var queue = (QueueTextTask)Tasks.Single(x => x.GetType() == typeof(QueueTextTask));

            if (!queue.TryEnqueueText(model))
            {
                ModelState.AddModelError(MessageType.TextEnqueueError.ToString(), $"SenderID:{model.FromId} SenderPhone:{model.FromPhoneNumber}");
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
