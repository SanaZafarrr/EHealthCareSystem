using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareSystem.Models
{
    [Table("Doctors")]
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        public string DoctorName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Department { get; set; }

        public string AvailableDays { get; set; }

        public int MaxPatientsPerDay { get; set; } = 10;
    }
}