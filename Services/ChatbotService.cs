using System.Text;
using System.Text.Json;
using HealthCareSystem.Models;
using Microsoft.Data.SqlClient;

namespace HealthCareSystem.Services
{
    public class ChatbotService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private const string FLASK_URL = "http://localhost:5001/chat";

        public ChatbotService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<ChatbotResponse> ProcessMessage(string message, int patientId)
        {
            var response = new ChatbotResponse
            {
                Disease = "General Health Query",
                Confidence = 0,
                Guidance = "",
                Description = "",
                Precautions = new List<string>(),
                Specialization = "",
                Urgency = "routine",
                SuggestedDoctors = new List<SuggestedDoctor>()
            };

            try
            {
                // =============================================
                // STEP 1 — Call Flask API (Platform 4)
                // Flask runs your trained model (Platform 2)
                // and calls Gemini API (Platform 3)
                // =============================================
                var http = _httpFactory.CreateClient();
                var payload = new { message = message };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var flaskResult = await http.PostAsync(FLASK_URL, content);
                var resultJson = await flaskResult.Content.ReadAsStringAsync();

                var aiResult = JsonSerializer.Deserialize<FlaskResponse>(resultJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (aiResult == null)
                    return response;

                response.Disease = aiResult.Disease ?? "Unknown";
                response.Confidence = aiResult.Confidence;
                response.Guidance = aiResult.Guidance ?? "";
                response.Description = aiResult.Description ?? "";
                response.Precautions = aiResult.Precautions ?? new List<string>();

                // =============================================
                // STEP 2 — Query SQL Server (Platform 5)
                // Match disease to department
                // Find doctors from Doctors table
                // Save chat to ChatLog table
                // =============================================
                string? connStr = _config.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    // Get specialization and urgency for predicted disease
                    string specQuery = @"SELECT Specialization, Urgency 
                                         FROM DiseaseSpecialization 
                                         WHERE DiseaseName = @disease";
                    using (var cmd = new SqlCommand(specQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@disease", aiResult.Disease ?? "");
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                response.Specialization = reader["Specialization"]?.ToString() ?? "";
                                response.Urgency = reader["Urgency"]?.ToString() ?? "routine";
                            }
                        }
                    }

                    // Get matching doctors from your Doctors table
                    if (!string.IsNullOrEmpty(response.Specialization))
                    {
                        string docQuery = @"SELECT TOP 3 DoctorId, DoctorName, 
                                             Department, AvailableDays, Email
                                             FROM Doctors
                                             WHERE Department = @dept";
                        using (var cmd = new SqlCommand(docQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@dept", response.Specialization);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    response.SuggestedDoctors!.Add(new SuggestedDoctor
                                    {
                                        DoctorId = Convert.ToInt32(reader["DoctorId"]),
                                        DoctorName = reader["DoctorName"]?.ToString() ?? "",
                                        Department = reader["Department"]?.ToString() ?? "",
                                        AvailableDays = reader["AvailableDays"]?.ToString() ?? "",
                                        Email = reader["Email"]?.ToString() ?? ""
                                    });
                                }
                            }
                        }
                    }

                    // Save conversation to ChatLog table
                    string logQuery = @"INSERT INTO ChatLog
                                        (PatientId, MessageText, PredictedDisease,
                                         AIResponse, SuggestedSpecialization,
                                         Urgency, SessionId)
                                        VALUES (@pid, @msg, @disease,
                                                @resp, @spec, @urgency, @session)";
                    using (var cmd = new SqlCommand(logQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@pid", patientId);
                        cmd.Parameters.AddWithValue("@msg", message);
                        cmd.Parameters.AddWithValue("@disease", response.Disease ?? "");
                        cmd.Parameters.AddWithValue("@resp", response.Guidance ?? "");
                        cmd.Parameters.AddWithValue("@spec", response.Specialization ?? "");
                        cmd.Parameters.AddWithValue("@urgency", response.Urgency ?? "routine");
                        cmd.Parameters.AddWithValue("@session", Guid.NewGuid().ToString());
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                response.Guidance = "Sorry, I could not process your request. " +
                                    "Please make sure the AI service is running. " +
                                    "Error: " + ex.Message;
            }

            return response;
        }

        internal async Task GetResponseAsync(List<object> conversationHistory)
        {
            throw new NotImplementedException();
        }
    }
}