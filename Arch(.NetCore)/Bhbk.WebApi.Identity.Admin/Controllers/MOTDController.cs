using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("motd")]
    public class MOTDController : BaseController
    {
        private MOTDProvider _provider;

        public MOTDController(IConfiguration conf, IContextService instance)
        {
            _provider = new MOTDProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult CreateV1([FromBody] MOTDTssV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.MOTDs.Get(x => x.Quote == model.quote).Any())
            {
                ModelState.AddModelError(MessageType.MOTDAlreadyExists.ToString(), $"Author:\"{model.author}\" Quote:\"{model.quote}\"");
                return BadRequest(ModelState);
            }

            var motd = Mapper.Map<tbl_MOTDs>(model);
            var result = UoW.MOTDs.Create(motd);

            UoW.Commit();

            return Ok(model);
        }

        [Route("v1/{motdID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid motdID)
        {
            var motd = UoW.MOTDs.Get(x => x.Id == motdID)
                .SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{motdID}");
                return NotFound(ModelState);
            }

            UoW.MOTDs.Delete(motd);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{motdValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string motdValue)
        {
            Guid motdID;
            tbl_MOTDs motd = null;

            if (Guid.TryParse(motdValue, out motdID))
                motd = UoW.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<tbl_MOTDs>()
                    .Where(x => x.Id == motdID).ToLambda())
                    .SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{motdValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<MOTDTssV1>(motd));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<MOTDTssV1>
                {
                    Data = Mapper.Map<IEnumerable<MOTDTssV1>>(
                        UoW.MOTDs.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_MOTDs>, IQueryable<tbl_MOTDs>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_MOTDs>().ApplyState(state)))),

                    Total = UoW.MOTDs.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_MOTDs>, IQueryable<tbl_MOTDs>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_MOTDs>().ApplyPredicate(state)))
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
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult UpdateV1([FromBody] MOTDTssV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var motd = UoW.MOTDs.Get(x => x.Id == model.globalId).SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{model.globalId}");
                return NotFound(ModelState);
            }

            var result = UoW.MOTDs.Update(Mapper.Map<tbl_MOTDs>(model));

            UoW.Commit();

            return Ok(Mapper.Map<MOTDTssV1>(result));
        }
    }
}
