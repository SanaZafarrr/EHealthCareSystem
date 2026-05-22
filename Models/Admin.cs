using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareSystem.Models
{
    [Table("Admin")] // maps to dbo.Admins table
    public class Admin
    {
        [Key]
        public int SystemId { get; set; } // primary key, identity

        [Required]
        public int AdminId { get; set; } // like UserId in Users table

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string UserType { get; set; } // should be "Admin"

        [Required]
        public string Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Phone { get; set; }
    }
}