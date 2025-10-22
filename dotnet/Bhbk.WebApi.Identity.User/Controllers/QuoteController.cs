using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Identity.User.Controllers
{
    [Route("quotes")]
    [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
    public class QuoteController : BaseController
    {
        [Route("v1"), HttpGet]
        public IActionResult GetV1()
        {
            var random = new Random();
            var skip = random.Next(1, uow.Quotes.Count());
            if (skip == 1)
                skip = 0;
            var quote = uow.Quotes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Quote>()
                .OrderBy("id").Skip(skip).Take(1).ToLambda())
                .SingleOrDefault();
            return Ok(map.Map<QuoteV1>(quote));
        }
    }
}
