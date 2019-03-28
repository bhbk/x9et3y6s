using Bhbk.Lib.Identity.DomainModels.Alert;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Text;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("exception")]
    public class ExceptionController : BaseController
    {
        public ExceptionController() { }

        [Route("v1"), HttpPost]
        public IActionResult ExceptionFromClient([FromBody] ExceptionCreate error)
        {
            StringBuilder msg = new StringBuilder();

            msg.AppendLine("Error Details ");
            msg.AppendLine("Venue: " + error.Venue);
            msg.AppendLine("Cause: " + error.Cause);
            msg.AppendLine("Message: " + error.Message);
            msg.AppendLine("Type: " + error.Type);
            msg.AppendLine("Url: " + error.Url);
            msg.AppendLine("StackTrace: ");

            foreach (var line in error.StackTrace)
                msg.AppendLine(line.Source);

            //Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(msg.ToString()));

            return Ok();
        }
    }
}
