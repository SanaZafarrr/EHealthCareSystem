using System.ComponentModel.DataAnnotations;

namespace HealthCareSystem.Models
{
    public class User
    {
        [Key]
        public int SystemId { get; set; }   // Auto primary key for EF Core

        public int UserId { get; set; }     // Unique ID shown to the user

        [Required(ErrorMessage = "First Name is required")]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
        [Display(Name = "Age")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "User Type is required")]
        [Display(Name = "User Type")]
        public string UserType { get; set; }  // Admin / Doctor / Patient

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Phone No is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone No")]
        public string Phone { get; set; }   // Matches the form's asp-for="Phone"
    }
}
