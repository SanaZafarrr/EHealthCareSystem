using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using HealthCareSystem.Data;
using HealthCareSystem.Models;

namespace HealthCareSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ==============================
        // DASHBOARD
        // ==============================
        public IActionResult HelloAdmin()
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            ViewBag.AdminName = admin.FirstName + " " + admin.LastName;
            return View();
        }

        // ==============================
        // ADMIN PROFILE (GET)
        // ==============================
        public IActionResult AdminProfile()
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            return View(admin);
        }

        // ==============================
        // ADMIN PROFILE (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminProfile(Admin updatedAdmin)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.SystemId == updatedAdmin.SystemId);

            if (admin == null)
                return RedirectToAction("Login", "Home");

            admin.FirstName = updatedAdmin.FirstName;
            admin.LastName = updatedAdmin.LastName;
            admin.Gender = updatedAdmin.Gender;
            admin.Age = updatedAdmin.Age;
            admin.Email = updatedAdmin.Email;
            admin.Phone = updatedAdmin.Phone;
            admin.City = updatedAdmin.City;
            admin.Address = updatedAdmin.Address;

            if (!string.IsNullOrWhiteSpace(updatedAdmin.Password))
                admin.Password = updatedAdmin.Password;

            _context.SaveChanges();
            ViewBag.Message = "Profile Updated Successfully!";

            return View(admin);
        }

        // ==============================
        // HOSPITAL INFO (GET)
        // ==============================
        public IActionResult HospitalInfo()
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var hospital = _context.Hospitals.FirstOrDefault();

            if (hospital == null)
            {
                hospital = new Hospital
                {
                    HospitalName = "",
                    Address = "",
                    City = ""
                };
            }

            return View(hospital);
        }

        // ==============================
        // HOSPITAL INFO (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HospitalInfo(Hospital updatedHospital)
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var hospital = _context.Hospitals.FirstOrDefault(h => h.HospitalId == updatedHospital.HospitalId);

            if (hospital == null)
            {
                _context.Hospitals.Add(updatedHospital);
            }
            else
            {
                hospital.HospitalName = updatedHospital.HospitalName;
                hospital.Address = updatedHospital.Address;
                hospital.City = updatedHospital.City;
            }

            _context.SaveChanges();
            ViewBag.Message = "Hospital Information Updated Successfully!";

            var updated = _context.Hospitals.FirstOrDefault(h => h.HospitalId == updatedHospital.HospitalId)
                          ?? _context.Hospitals.FirstOrDefault();

            return View(updated);
        }

        // ==============================
        // NEW DOCTOR
        // ==============================
        public IActionResult NewDoctor()
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            return View(new Doctor());
        }

        // ==============================
        // REGISTER DOCTOR (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterDoctor(Doctor doctor)
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var existing = _context.Doctors.FirstOrDefault(d => d.Email == doctor.Email);
            if (existing != null)
            {
                ViewBag.Error = "A doctor with this email already exists.";
                return View("NewDoctor", new Doctor());
            }

            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            ViewBag.Message = "Doctor registered successfully!";
            return View("NewDoctor", new Doctor());
        }

        // ==============================
        // VIEW DOCTORS
        // ==============================
        public IActionResult ViewDoctor()
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var doctors = _context.Doctors.ToList();
            return View(doctors);
        }

        // ==============================
        // EDIT DOCTOR (GET)
        // ==============================
        public IActionResult EditDoctor(int id)
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);
            if (doctor == null)
                return RedirectToAction("ViewDoctor");

            return View(doctor);
        }

        // ==============================
        // EDIT DOCTOR (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDoctor(Doctor updatedDoctor)
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == updatedDoctor.DoctorId);
            if (doctor == null)
                return RedirectToAction("ViewDoctor");

            doctor.DoctorName = updatedDoctor.DoctorName;
            doctor.Email = updatedDoctor.Email;
            doctor.Department = updatedDoctor.Department;
            doctor.AvailableDays = updatedDoctor.AvailableDays;
            doctor.MaxPatientsPerDay = updatedDoctor.MaxPatientsPerDay;

            if (!string.IsNullOrWhiteSpace(updatedDoctor.Password))
                doctor.Password = updatedDoctor.Password;

            _context.SaveChanges();

            TempData["Message"] = "Doctor updated successfully!";
            return RedirectToAction("ViewDoctor");
        }

        // ==============================
        // DELETE DOCTOR
        // ==============================
        public IActionResult DeleteDoctor(int id)
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                _context.SaveChanges();
            }

            TempData["Message"] = "Doctor deleted successfully!";
            return RedirectToAction("ViewDoctor");
        }

        // ==============================
        // VIEW PATIENTS
        // ==============================
        public IActionResult ViewPatient()
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var patients = _context.Users
                .Where(u => u.UserType.ToLower() == "patient")
                .ToList();

            return View(patients);
        }

        // ==============================
        // EDIT PATIENT (GET)
        // ==============================
        public IActionResult EditPatient(int id)
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var patient = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (patient == null)
                return RedirectToAction("ViewPatient");

            return View("PatientEdit", patient);
        }

        // ==============================
        // EDIT PATIENT (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePatient(User updatedUser)
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var patient = _context.Users.FirstOrDefault(u => u.UserId == updatedUser.UserId);
            if (patient == null)
                return RedirectToAction("ViewPatient");

            // Check if new email is taken by someone else
            var emailTaken = _context.Users.FirstOrDefault(u =>
                u.Email == updatedUser.Email && u.UserId != updatedUser.UserId);

            if (emailTaken != null)
            {
                TempData["Error"] = "Another account with this email already exists.";
                return RedirectToAction("EditPatient", new { id = updatedUser.UserId });
            }

            patient.FirstName = updatedUser.FirstName;
            patient.LastName = updatedUser.LastName;
            patient.Email = updatedUser.Email;
            patient.Phone = updatedUser.Phone;
            patient.Gender = updatedUser.Gender;
            patient.Age = updatedUser.Age;
            patient.City = updatedUser.City;
            patient.Address = updatedUser.Address;

            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
                patient.Password = updatedUser.Password;

            _context.SaveChanges();

            TempData["Message"] = "Patient updated successfully!";
            return RedirectToAction("ViewPatient");
        }

        // ==============================
        // DELETE PATIENT
        // ==============================
        public IActionResult DeletePatient(int id)
        {
            var admin = GetLoggedInAdmin();
            if (admin == null)
                return RedirectToAction("Login", "Home");

            var patient = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (patient != null)
            {
                _context.Users.Remove(patient);
                _context.SaveChanges();
            }

            TempData["Message"] = "Patient deleted successfully!";
            return RedirectToAction("ViewPatient");
        }

        // ==============================
        // LOGOUT
        // ==============================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        // ==============================
        // PRIVATE HELPER METHOD
        // ==============================
        private Admin GetLoggedInAdmin()
        {
            var adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
                return null;

            return _context.Admins.FirstOrDefault(a => a.SystemId == adminId);
        }
    }
}
