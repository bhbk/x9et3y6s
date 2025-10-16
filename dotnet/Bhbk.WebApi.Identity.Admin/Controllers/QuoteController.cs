using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("quotes")]
    public class QuoteController : BaseController
    {
        [Route("v1"), HttpPost]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
        public IActionResult CreateV1([FromBody] QuoteV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (uow.Quotes.Get(x => x.Quote == model.quote).Any())
            {
                ModelState.AddModelError(MessageType.QuoteAlreadyExists.ToString(), $"Author:\"{model.author}\" Quote:\"{model.quote}\"");
                return BadRequest(ModelState);
            }
            var quote = map.Map<tbl_Quote>(model);
            var result = uow.Quotes.Post(quote);
            uow.Commit();
            return Ok(model);
        }
        [Route("v1/{quoteID:guid}"), HttpDelete]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
        public IActionResult DeleteV1([FromRoute] Guid quoteID)
        {
            var quote = uow.Quotes.Get(x => x.Id == quoteID).SingleOrDefault();
            if (quote == null)
            {
                ModelState.AddModelError(MessageType.QuoteNotFound.ToString(), $"Quote:{quoteID}");
                return NotFound(ModelState);
            }
            uow.Quotes.Delete(quote);
            uow.Commit();
            return NoContent();
        }
        [Route("v1/{quoteValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string quoteValue)
        {
            Guid quoteID;
            tbl_Quote quote = null;
            if (Guid.TryParse(quoteValue, out quoteID))
                quote = uow.Quotes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Quote>()
                    .Where(x => x.Id == quoteID).ToLambda()).SingleOrDefault();
            if (quote == null)
            {
                ModelState.AddModelError(MessageType.QuoteNotFound.ToString(), $"Quote:{quoteValue}");
                return NotFound(ModelState);
            }
            return Ok(map.Map<QuoteV1>(quote));
        }
        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] PagerState state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = new PagerStateResult<QuoteV1>
                {
                    Data = map.Map<IEnumerable<QuoteV1>>(
                        uow.Quotes.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_Quote>, IQueryable<tbl_Quote>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_Quote>().ApplyState(state)))),
                    Total = uow.Quotes.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_Quote>, IQueryable<tbl_Quote>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_Quote>().ApplyPredicate(state)))
                };
                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }
        [Route("v1"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
        public IActionResult UpdateV1([FromBody] QuoteV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var quote = uow.Quotes.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_Quote>()
                .Where(x => x.Id == model.globalId).ToLambda()).SingleOrDefault();
            if (quote == null)
            {
                ModelState.AddModelError(MessageType.QuoteNotFound.ToString(), $"Quote:{model.globalId}");
                return NotFound(ModelState);
            }
            var result = uow.Quotes.Put(map.Map<tbl_Quote>(model));
            uow.Commit();
            return Ok(map.Map<QuoteV1>(result));
        }
    }
}
