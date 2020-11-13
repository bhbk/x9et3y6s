using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using SendGrid;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Services
{
    public interface ISendgridService
    {
        ValueTask<Response> TryEmailHandoff(string apiKey, tbl_EmailQueue model);
    }
}
