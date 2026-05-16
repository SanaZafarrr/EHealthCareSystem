using System.ComponentModel.DataAnnotations;

namespace HealthCareSystem.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        public int AppointmentId { get; set; }

        public int UserId { get; set; }

        public int DoctorId { get; set; }

        public string PatientName { get; set; }

        public string DoctorName { get; set; }

        public string Diagnosis { get; set; }

        public string Medicines { get; set; }

        public string Notes { get; set; }
    }
}
