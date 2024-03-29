﻿using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_Tbl;
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
    [Route("motd")]
    public class MOTDController : BaseController
    {
        [Route("v1"), HttpPost]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult CreateV1([FromBody] MOTDTssV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (uow.MOTDs.Get(x => x.Quote == model.quote).Any())
            {
                ModelState.AddModelError(MessageType.MOTDAlreadyExists.ToString(), $"Author:\"{model.author}\" Quote:\"{model.quote}\"");
                return BadRequest(ModelState);
            }

            var motd = map.Map<tbl_MOTD>(model);
            var result = uow.MOTDs.Create(motd);

            uow.Commit();

            return Ok(model);
        }

        [Route("v1/{motdID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid motdID)
        {
            var motd = uow.MOTDs.Get(x => x.Id == motdID)
                .SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{motdID}");
                return NotFound(ModelState);
            }

            uow.MOTDs.Delete(motd);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/{motdValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string motdValue)
        {
            Guid motdID;
            tbl_MOTD motd = null;

            if (Guid.TryParse(motdValue, out motdID))
                motd = uow.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<tbl_MOTD>()
                    .Where(x => x.Id == motdID).ToLambda())
                    .SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{motdValue}");
                return NotFound(ModelState);
            }

            return Ok(map.Map<MOTDTssV1>(motd));
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
                    Data = map.Map<IEnumerable<MOTDTssV1>>(
                        uow.MOTDs.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_MOTD>, IQueryable<tbl_MOTD>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_MOTD>().ApplyState(state)))),

                    Total = uow.MOTDs.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_MOTD>, IQueryable<tbl_MOTD>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_MOTD>().ApplyPredicate(state)))
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
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult UpdateV1([FromBody] MOTDTssV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var motd = uow.MOTDs.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_MOTD>()
                .Where(x => x.Id == model.globalId).ToLambda()).SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{model.globalId}");
                return NotFound(ModelState);
            }

            var result = uow.MOTDs.Update(map.Map<tbl_MOTD>(model));

            uow.Commit();

            return Ok(map.Map<MOTDTssV1>(result));
        }
    }
}
