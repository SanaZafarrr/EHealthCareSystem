using Microsoft.AspNetCore.Mvc;
using HealthCareSystem.Data;
using HealthCareSystem.Models;
using HealthCareSystem.Services;
using System;
using System.Linq;

namespace HealthCareSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        // ================= DASHBOARD =================
        public IActionResult HelloPatient()
        {
            return View();
        }

        public IActionResult AboutUS()
        {
            return View();
        }

        public IActionResult ViewDoctors()
        {
            return View();
        }

        // ================= CHAT AI =================
       /* public IActionResult ChatAI()
        {
            return View(); // ChatAI.cshtml
        }*/

        // ================= PATIENT HISTORY =================
        public IActionResult PatientHistory()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var history = (from appointment in _context.Appointments

                           join doctor in _context.Doctors
                           on appointment.DoctorId equals doctor.DoctorId

                           join prescription in _context.Prescriptions
                           on appointment.Id equals prescription.AppointmentId
                           into prescriptionGroup

                           from prescription in prescriptionGroup.DefaultIfEmpty()

                           where appointment.UserId == userId

                           orderby appointment.AppointmentDate descending

                           select new PatientHistoryViewModel
                           {
                               AppointmentDate = appointment.AppointmentDate,

                               DoctorName = doctor.DoctorName,

                               Department = doctor.Department,

                               Status = appointment.Status,

                               Disease = prescription != null
                                   ? prescription.Diagnosis
                                   : "Pending Doctor Update",

                               Prescription = prescription != null
                                   ? prescription.Medicines
                                   : "Pending Doctor Update",

                               AIChatStatus = "No AI Chat"
                           }).ToList();

            return View(history);
        }

        // ================= BOOK APPOINTMENT =================
        public IActionResult BookAppointment()
        {
            ViewBag.Doctors = _context.Doctors.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult BookAppointment(int doctorId, DateTime appointmentDate)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string userName = HttpContext.Session.GetString("UserName");

            if (userId == null)
            {
                TempData["Error"] = "Please login first!";
                return RedirectToAction("Login", "Home");
            }

            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);

            if (doctor == null)
            {
                TempData["Error"] = "Doctor not found!";
                return RedirectToAction("BookAppointment");
            }

            DateTime today = DateTime.Today;

            if (appointmentDate.Date < today)
            {
                TempData["Error"] = "You cannot book past dates!";
                return RedirectToAction("BookAppointment");
            }

            if (appointmentDate.Date > today.AddMonths(2))
            {
                TempData["Error"] = "You can only book up to 2 months ahead!";
                return RedirectToAction("BookAppointment");
            }

            string selectedDay = appointmentDate.DayOfWeek.ToString();
            var availableDays = doctor.AvailableDays.Split(',');

            if (!availableDays.Contains(selectedDay))
            {
                TempData["Error"] = "Doctor not available on this day!";
                return RedirectToAction("BookAppointment");
            }

            int totalAppointments = _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate.Date == appointmentDate.Date)
                .Count();

            if (totalAppointments >= doctor.MaxPatientsPerDay)
            {
                TempData["Error"] = "This day is fully booked!";
                return RedirectToAction("BookAppointment");
            }

            Appointment appointment = new Appointment
            {
                DoctorId = doctorId,
                UserId = userId.Value,
                AppointmentDate = appointmentDate,
                Status = "Pending",
                PatientName = userName,
                DoctorName = doctor.DoctorName
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction("BookAppointment");
        }

        // ================= AJAX =================
        public JsonResult GetDoctorDays(int doctorId)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null) return Json(new string[] { });

            return Json(doctor.AvailableDays.Split(','));
        }

        public JsonResult CheckAvailability(int doctorId, DateTime date)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null) return Json("Invalid");

            int count = _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate.Date == date.Date)
                .Count();

            return Json(count >= doctor.MaxPatientsPerDay ? "Full" : "Available");
        }

        // ================= CANCEL APPOINTMENT =================

        // GET
        public IActionResult CancelAppointment(string patientName, string email)
        {
            // Get logged-in user data from session
            string sessionName = HttpContext.Session.GetString("UserName");
            string sessionEmail = HttpContext.Session.GetString("UserEmail");

            // If user not logged in
            if (string.IsNullOrEmpty(sessionName) ||
                string.IsNullOrEmpty(sessionEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // If user entered data in search
            if (!string.IsNullOrEmpty(patientName) &&
                !string.IsNullOrEmpty(email))
            {
                // CHECK:
                // Is entered account same as logged-in account?
                if (patientName.ToLower().Trim() != sessionName.ToLower().Trim()
                    ||
                    email.ToLower().Trim() != sessionEmail.ToLower().Trim())
                {
                    ViewBag.Message =
                        "This is not your account. Please use your own registered account.";

                    return View();
                }

                // Find logged-in user
                var user = _context.Users.FirstOrDefault(u =>
                    u.FirstName.ToLower().Trim() == sessionName.ToLower().Trim()
                    &&
                    u.Email.ToLower().Trim() == sessionEmail.ToLower().Trim());

                if (user != null)
                {
                    // Get appointments
                    var appointments = _context.Appointments
                        .Where(a => a.UserId == user.SystemId)
                        .OrderByDescending(a => a.AppointmentDate)
                        .ToList();

                    ViewBag.PatientName = patientName;
                    ViewBag.Email = email;

                    return View(appointments);
                }

                ViewBag.Message = "User not found!";
            }

            return View();
        }


        // POST (Search Appointment)
        [HttpPost]
        public IActionResult CancelAppointment(string patientName, string email, int? dummy = null)
        {
            if (string.IsNullOrEmpty(patientName) ||
                string.IsNullOrEmpty(email))
            {
                ViewBag.Message =
                    "Please enter both Name and Email!";

                return View();
            }

            return RedirectToAction("CancelAppointment",
                new
                {
                    patientName,
                    email
                });
        }


        // POST (Cancel Appointment)
        [HttpPost]
        public IActionResult ConfirmCancel(int id, string patientName, string email)
        {
            // Get logged-in session data
            string sessionName = HttpContext.Session.GetString("UserName");
            string sessionEmail = HttpContext.Session.GetString("UserEmail");

            // SECURITY CHECK
            if (patientName.ToLower().Trim() != sessionName.ToLower().Trim()
                ||
                email.ToLower().Trim() != sessionEmail.ToLower().Trim())
            {
                TempData["Success"] =
                    "You cannot access another user's appointments.";

                return RedirectToAction("CancelAppointment");
            }

            var appointment = _context.Appointments.Find(id);

            if (appointment != null)
            {
                appointment.Status = "Cancelled";

                _context.SaveChanges();

                TempData["Success"] =
                    "Appointment cancelled successfully!";
            }

            return RedirectToAction("CancelAppointment",
                new
                {
                    patientName,
                    email
                });
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        // ================= AI CHATBOT PAGE =================
        public IActionResult ChatAI()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Home");

            var userType = HttpContext.Session.GetString("UserType");
            if (userType?.ToLower() != "patient")
                return RedirectToAction("Login", "Home");

            return View();
        }

        // ================= AI CHATBOT MESSAGE =================
        [HttpPost]
        public async Task<IActionResult> SendChatMessage(
            [FromBody] ChatMessageRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { error = "Session expired. Please login again." });

            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return Json(new { error = "Please type a message." });

            request.PatientId = userId.Value;

            var chatbot = new ChatbotService(
                HttpContext.RequestServices
                    .GetRequiredService<IHttpClientFactory>(),
                HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
            );

            var result = await chatbot.ProcessMessage(
                request.Message, request.PatientId);

            return Json(result);
        }
    }

}