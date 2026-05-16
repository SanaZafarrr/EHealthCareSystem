namespace HealthCareSystem.Models
{
    public class ChatMessageRequest
    {
        public string? Message { get; set; }
        public int PatientId { get; set; }
    }

    public class FlaskResponse
    {
        public string? Disease { get; set; }
        public double Confidence { get; set; }
        public string? Guidance { get; set; }
        public string? Description { get; set; }
        public List<string>? Precautions { get; set; }
        public List<string>? SymptomsFound { get; set; }
    }

    public class ChatbotResponse
    {
        public string? Disease { get; set; }
        public double Confidence { get; set; }
        public string? Guidance { get; set; }
        public string? Description { get; set; }
        public List<string>? Precautions { get; set; }
        public string? Specialization { get; set; }
        public string? Urgency { get; set; }
        public List<SuggestedDoctor>? SuggestedDoctors { get; set; }
    }

    public class SuggestedDoctor
    {
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? Department { get; set; }
        public string? AvailableDays { get; set; }
        public string? Email { get; set; }
    }

    public class ChatLogEntry
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string? MessageText { get; set; }
        public string? PredictedDisease { get; set; }
        public string? AIResponse { get; set; }
        public string? SuggestedSpecialization { get; set; }
        public string? Urgency { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SessionId { get; set; }
    }
}