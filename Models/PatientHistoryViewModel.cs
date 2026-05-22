namespace HealthCareSystem.Models
{
    public class PatientHistoryViewModel
    {
        public DateTime AppointmentDate { get; set; }

        public string DoctorName { get; set; }

        public string Department { get; set; }

        public string Status { get; set; }

        public string Disease { get; set; }

        public string Prescription { get; set; }

        public string AIChatStatus { get; set; }
    }
}
