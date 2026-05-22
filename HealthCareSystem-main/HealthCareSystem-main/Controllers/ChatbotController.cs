using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace HealthCareSystem.Controllers
{
    public class ChatbotController : Controller
    {
        // =========================================
        // HTTP CLIENT
        // =========================================
        private static readonly HttpClient client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        // =========================================
        // INDEX PAGE
        // =========================================
        public IActionResult Index()
        {
            return View();
        }

        // =========================================
        // CHAT PAGE
        // =========================================
        public IActionResult ChatAI()
        {
            return View();
        }

        // =========================================
        // ASK CHATBOT
        // =========================================
        [HttpPost]
        public async Task<IActionResult> AskBot(string message)
        {
            try
            {
                // =========================================
                // VALIDATE INPUT
                // =========================================
                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new
                    {
                        reply = "Please enter a message."
                    });
                }

                // =========================================
                // CREATE JSON PAYLOAD
                // =========================================
                var payload = new
                {
                    message = message.Trim()
                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response;

                // =========================================
                // CALL FLASK API
                // =========================================
                try
                {
                    response = await client.PostAsync(
                        "http://127.0.0.1:5000/chat",
                        content
                    );
                }
                catch
                {
                    return Json(new
                    {
                        reply = "❌ Chatbot service is not reachable. Make sure Flask API is running."
                    });
                }

                // =========================================
                // CHECK RESPONSE STATUS
                // =========================================
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        reply = $"❌ Chatbot API Error: {response.StatusCode}"
                    });
                }

                // =========================================
                // READ API RESPONSE
                // =========================================
                var result = await response.Content.ReadAsStringAsync();

                string reply;

                // =========================================
                // SAFE JSON PARSING
                // =========================================
                try
                {
                    JsonNode obj = JsonNode.Parse(result);

                    reply =
                        obj?["reply"] != null
                            ? obj["reply"].ToString()
                        : obj?["response"] != null
                            ? obj["response"].ToString()
                        : result;
                }
                catch
                {
                    // If API returns plain text
                    reply = result;
                }

                // =========================================
                // RETURN FINAL RESPONSE
                // =========================================
                return Json(new
                {
                    reply = reply
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    reply = "❌ Server Error: " + ex.Message
                });
            }
        }
    }
}