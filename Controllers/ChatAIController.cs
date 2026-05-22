/*using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthCareSystem.Services;

namespace HealthCareSystem.Controllers
{
    [IgnoreAntiforgeryToken]
    public class ChatAIController : Controller
    {
        private readonly OllamaChatService _chatService;

        public ChatAIController(OllamaChatService chatService)
        {
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Patient/ChatAI.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] List<MessageDto> messages)
        {
            if (messages == null || messages.Count == 0)
                return Json(new { success = false, message = "No messages received." });

            var conversationHistory = new List<object>
            {
                new { role = "system", content = "You are a medical assistant. Listen to symptoms, suggest precautions and recommend doctor type. Be brief and clear. Never diagnose definitively. For emergencies say: GO TO ER NOW." }
            };

            foreach (var msg in messages)
            {
                conversationHistory.Add(new
                {
                    role = msg.Role.ToLower(),
                    content = msg.Content
                });
            }

            var reply = await _chatService.GetResponseAsync(conversationHistory);
            return Json(new { success = true, reply = reply });
        }
    }

    public class MessageDto
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
*/