using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("enqueue")]
    public class EnqueueController : BaseController
    {
        [Route("v1/email"), HttpPost]
        public IActionResult EnqueueEmailV1([FromBody] EmailV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = null;
            model.FromId = null;

            var email = Mapper.Map<tbl_EmailQueue>(model);

            UoW.EmailQueue.Create(email);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/email/page"), HttpPost]
        public IActionResult GetEmailsV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<EmailV1>
                {
                    Data = Mapper.Map<IEnumerable<EmailV1>>(
                        UoW.Audiences.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_EmailQueue>, IQueryable<tbl_EmailQueue>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ApplyState(state)))),

                    Total = UoW.Audiences.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_EmailQueue>, IQueryable<tbl_EmailQueue>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/text"), HttpPost]
        public IActionResult EnqueueTextV1([FromBody] TextV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = null;
            model.FromId = null;

            var text = Mapper.Map<tbl_TextQueue>(model);

            UoW.TextQueue.Create(text);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/text/page"), HttpPost]
        public IActionResult GetTextsV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<EmailV1>
                {
                    Data = Mapper.Map<IEnumerable<EmailV1>>(
                        UoW.Audiences.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_TextQueue>, IQueryable<tbl_TextQueue>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>().ApplyState(state)))),

                    Total = UoW.Audiences.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_TextQueue>, IQueryable<tbl_TextQueue>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }
    }
}
