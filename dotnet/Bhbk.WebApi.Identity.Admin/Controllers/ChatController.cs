using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.LLM.Services;
using Bhbk.Lib.Identity.Primitives.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("chat")]
    [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
    public class ChatController : BaseController
    {
        [Route("conversations"), HttpGet]
        public IActionResult GetConversations([FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var userId = GetIdentityGUID();
            var conversationService = new ConversationService(uow);
            var conversations = conversationService.GetUserConversations(userId, skip, take);

            return Ok(conversations.Select(c => new
            {
                c.Id,
                c.Title,
                c.Started,
                c.Ended,
                c.Created
            }));
        }

        [Route("conversations/{id}"), HttpGet]
        public IActionResult GetConversation(Guid id)
        {
            var userId = GetIdentityGUID();
            var conversation = uow.ChatConversations.Get(x => x.Id == id && x.UserId == userId).FirstOrDefault();

            if (conversation == null)
                return NotFound();

            return Ok(new
            {
                conversation.Id,
                conversation.Title,
                conversation.Started,
                conversation.Ended,
                conversation.Created
            });
        }

        [Route("conversations"), HttpPost]
        public IActionResult CreateConversation([FromBody] CreateConversationRequest request)
        {
            var userId = GetIdentityGUID();
            var conversationService = new ConversationService(uow);
            var conversation = conversationService.CreateConversation(userId, request?.Title);

            return Created($"/chat/conversations/{conversation.Id}", new
            {
                conversation.Id,
                conversation.Title,
                conversation.Started,
                conversation.Created
            });
        }

        [Route("conversations/{id}"), HttpDelete]
        public IActionResult DeleteConversation(Guid id)
        {
            var userId = GetIdentityGUID();
            var conversation = uow.ChatConversations.Get(x => x.Id == id && x.UserId == userId).FirstOrDefault();

            if (conversation == null)
                return NotFound();

            var conversationService = new ConversationService(uow);
            conversationService.DeleteConversation(id);

            return NoContent();
        }

        [Route("conversations/{id}/messages"), HttpGet]
        public IActionResult GetMessages(Guid id)
        {
            var userId = GetIdentityGUID();
            var conversation = uow.ChatConversations.Get(x => x.Id == id && x.UserId == userId).FirstOrDefault();

            if (conversation == null)
                return NotFound();

            var conversationService = new ConversationService(uow);
            var messages = conversationService.GetConversationMessages(id);

            return Ok(messages.Select(m => new
            {
                m.Id,
                m.Role,
                m.Content,
                m.InputTokens,
                m.OutputTokens,
                m.Created
            }));
        }

        [Route("conversations/{conversationId}/files"), HttpGet]
        public IActionResult GetConversationFiles(Guid conversationId)
        {
            var userId = GetIdentityGUID();
            var conversation = uow.ChatConversations.Get(x => x.Id == conversationId && x.UserId == userId).FirstOrDefault();

            if (conversation == null)
                return NotFound();

            var files = uow.ChatFiles.Get(x => x.ConversationId == conversationId);

            return Ok(files.Select(f => new
            {
                f.Id,
                f.FileName,
                f.ContentType,
                f.FileSize,
                f.Summary,
                f.Expires,
                f.Created
            }));
        }

        [Route("files/{id}"), HttpGet]
        public IActionResult DownloadFile(Guid id)
        {
            var userId = GetIdentityGUID();
            var file = uow.ChatFiles.Get(x => x.Id == id).FirstOrDefault();

            if (file == null)
                return NotFound();

            /* Verify the file's conversation belongs to this user. */
            var conversation = uow.ChatConversations.Get(x => x.Id == file.ConversationId && x.UserId == userId).FirstOrDefault();
            if (conversation == null)
                return NotFound();

            if (file.Expires < DateTimeOffset.UtcNow)
                return NotFound();

            return File(file.FileContent, file.ContentType, file.FileName);
        }
        [Route("favorites"), HttpGet]
        public IActionResult GetFavorites()
        {
            var userId = GetIdentityGUID();
            var favorites = uow.ChatFavorites
                .Get(x => x.UserId == userId)
                .OrderBy(x => x.Created)
                .ToList();

            return Ok(favorites.Select(f => new
            {
                f.Id,
                f.Name,
                f.Prompt,
                f.Pinned,
                f.Created
            }));
        }

        [Route("favorites"), HttpPost]
        public IActionResult CreateFavorite([FromBody] CreateFavoriteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name) || string.IsNullOrWhiteSpace(request?.Prompt))
                return BadRequest();

            var userId = GetIdentityGUID();

            var count = uow.ChatFavorites.Get(x => x.UserId == userId).Count();
            if (count >= 50)
                return BadRequest("Maximum of 50 favorites reached.");

            var entry = new tbl_ChatFavorite
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name.Trim(),
                Prompt = request.Prompt.Trim(),
                Pinned = false,
                Created = DateTimeOffset.UtcNow
            };

            uow.ChatFavorites.Post(entry);
            uow.Commit();

            return Created($"/chat/favorites/{entry.Id}", new
            {
                entry.Id,
                entry.Name,
                entry.Prompt,
                entry.Pinned,
                entry.Created
            });
        }

        [Route("favorites/{id}"), HttpPut]
        public IActionResult UpdateFavorite(Guid id, [FromBody] UpdateFavoriteRequest request)
        {
            if (request == null)
                return BadRequest();

            var userId = GetIdentityGUID();
            var favorite = uow.ChatFavorites.Get(x => x.Id == id && x.UserId == userId).FirstOrDefault();

            if (favorite == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Name))
                favorite.Name = request.Name.Trim();

            if (!string.IsNullOrWhiteSpace(request.Prompt))
                favorite.Prompt = request.Prompt.Trim();

            if (request.Pinned.HasValue)
                favorite.Pinned = request.Pinned.Value;

            uow.ChatFavorites.Put(favorite);
            uow.Commit();

            return Ok(new
            {
                favorite.Id,
                favorite.Name,
                favorite.Prompt,
                favorite.Pinned,
                favorite.Created
            });
        }

        [Route("favorites/{id}"), HttpDelete]
        public IActionResult DeleteFavorite(Guid id)
        {
            var userId = GetIdentityGUID();
            var favorite = uow.ChatFavorites.Get(x => x.Id == id && x.UserId == userId).FirstOrDefault();

            if (favorite == null)
                return NotFound();

            uow.ChatFavorites.Delete(favorite);
            uow.Commit();

            return NoContent();
        }

        [Route("prompt-history"), HttpGet]
        public IActionResult GetPromptHistory()
        {
            var userId = GetIdentityGUID();
            var history = uow.ChatPromptHistories
                .Get(x => x.UserId == userId)
                .OrderByDescending(x => x.Created)
                .Take(50)
                .ToList();

            return Ok(history.Select(h => new
            {
                h.Id,
                h.PromptText,
                h.Created
            }));
        }

        [Route("prompt-history"), HttpPost]
        public IActionResult AddPromptHistory([FromBody] AddPromptHistoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.PromptText))
                return BadRequest();

            var userId = GetIdentityGUID();

            var entry = new tbl_ChatPromptHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PromptText = request.PromptText.Trim(),
                Created = DateTimeOffset.UtcNow
            };

            uow.ChatPromptHistories.Post(entry);
            uow.Commit();

            /* cap at 50 rows per user */
            var excess = uow.ChatPromptHistories
                .Get(x => x.UserId == userId)
                .OrderByDescending(x => x.Created)
                .Skip(50)
                .ToList();

            if (excess.Count > 0)
            {
                foreach (var old in excess)
                    uow.ChatPromptHistories.Delete(old);

                uow.Commit();
            }

            return Created($"/chat/prompt-history", new
            {
                entry.Id,
                entry.PromptText,
                entry.Created
            });
        }
    }

    public class CreateConversationRequest
    {
        public string Title { get; set; }
    }

    public class AddPromptHistoryRequest
    {
        public string PromptText { get; set; }
    }

    public class CreateFavoriteRequest
    {
        public string Name { get; set; }
        public string Prompt { get; set; }
    }

    public class UpdateFavoriteRequest
    {
        public string Name { get; set; }
        public string Prompt { get; set; }
        public bool? Pinned { get; set; }
    }
}
