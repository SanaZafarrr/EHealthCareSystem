using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareSystem.Models
{
    [Table("Hospitals")]  // ← must match exactly the table name in database
    public class Hospital
    {
        [Key]
        public int HospitalId { get; set; }

        [Required]
        public string HospitalName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }
    }
}