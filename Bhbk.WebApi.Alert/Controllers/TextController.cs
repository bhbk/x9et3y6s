﻿using Bhbk.Lib.Identity.Internal.Primitives.Enums;
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
    public class TextController : BaseController
    {
        public TextController() { }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
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

            if (!queue.TryEnqueueText(model))
            {
                ModelState.AddModelError(MessageType.TextEnqueueError.ToString(), $"MessageID:{model.Id} SenderPhone:{model.FromPhoneNumber}");
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}