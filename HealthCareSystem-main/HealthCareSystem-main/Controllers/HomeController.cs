


using HealthCareSystem.Data;
using HealthCareSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace HealthCareSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // ================= HOME =================
        public IActionResult Index()
        {
            return View();
        }

        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            var existingUser = _context.Users
                .FirstOrDefault(u => u.Email == user.Email);

            if (existingUser != null)
            {
                ViewBag.Error = "Email already exists!";
                return View(user);
            }

            if (user.UserType.ToLower() == "admin")
            {
                ViewBag.Error = "Admin cannot register from here!";
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "You are successfully registered!";
            return RedirectToAction("Login");
        }

        // ================= LOGIN =================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password, string UserType)
        {
            if (string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(UserType))
            {
                ViewBag.Error = "All fields are required!";
                return View();
            }

            // ================= ADMIN LOGIN (UNCHANGED) =================
            if (UserType.ToLower() == "admin")
            {
                var admin = _context.Admins.FirstOrDefault(a =>
                    a.Email == Email &&
                    a.Password == Password);

                if (admin == null)
                {
                    ViewBag.Error = "Invalid Admin Credentials!";
                    return View();
                }

                HttpContext.Session.SetInt32("UserId", admin.SystemId);
                HttpContext.Session.SetString("UserName", admin.FirstName);
                HttpContext.Session.SetString("UserType", "Admin");

                return RedirectToAction("HelloAdmin", "Admin");
            }

            // ================= DOCTOR LOGIN (FIXED + CORRECT PLACE) =================
            if (UserType.ToLower() == "doctor")
            {
                var doctor = _context.Doctors.FirstOrDefault(d =>
                    d.Email == Email &&
                    d.Password == Password);

                if (doctor == null)
                {
                    ViewBag.Error = "Invalid Doctor Credentials!";
                    return View();
                }

                HttpContext.Session.SetInt32("DoctorId", doctor.DoctorId);
                HttpContext.Session.SetString("DoctorName", doctor.DoctorName);
                HttpContext.Session.SetString("DoctorEmail", doctor.Email);
                HttpContext.Session.SetString("UserType", "Doctor");

                return RedirectToAction("HelloDoctor", "Doctor");
            }

            // ================= USER / PATIENT LOGIN (UNCHANGED) =================
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == Email &&
                u.Password == Password &&
                u.UserType.ToLower() == UserType.ToLower());

            if (user == null)
            {
                ViewBag.Error = "Invalid Credentials!";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.SystemId);
            HttpContext.Session.SetString("UserName", user.FirstName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserType", user.UserType);

            if (user.UserType.ToLower() == "patient")
                return RedirectToAction("HelloPatient", "Patient");

            return RedirectToAction("Index");
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ================= OTHER PAGES =================
        [HttpGet]
        public IActionResult Services()
        {
            return View();
        }

    }
}