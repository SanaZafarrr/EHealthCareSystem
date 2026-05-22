using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HealthCareSystem.Data;
using HealthCareSystem.Models;

using System;
using System.Linq;
using System.Text;

using System.Net.Http;
using System.Net.Http.Headers;

using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace HealthCareSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppDbContext _context;

        private static readonly HttpClient client = new HttpClient();

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

        // ================= PATIENT HISTORY =================

        public IActionResult PatientHistory()
        {
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Home"
                );
            }

            var history =
                (
                    from appointment in _context.Appointments

                    join doctor in _context.Doctors
                    on appointment.DoctorId equals doctor.DoctorId

                    join prescription in _context.Prescriptions
                    on appointment.Id equals prescription.AppointmentId
                    into prescriptionGroup

                    from prescription in
                        prescriptionGroup.DefaultIfEmpty()

                    where appointment.UserId == userId

                    orderby appointment.AppointmentDate descending

                    select new PatientHistoryViewModel
                    {
                        AppointmentDate =
                            appointment.AppointmentDate,

                        DoctorName =
                            doctor.DoctorName,

                        Department =
                            doctor.Department,

                        Status =
                            appointment.Status,

                        // ================= DIAGNOSIS =================
                        Disease =
                            appointment.Status == "Cancelled"
                                ? "Appointment Cancelled"

                                : prescription != null
                                    ? prescription.Diagnosis

                                    : "Pending Doctor Update",

                        // ================= MEDICINES =================
                        Prescription =
                            appointment.Status == "Cancelled"
                                ? "Appointment Cancelled"

                                : prescription != null
                                    ? prescription.Medicines

                                    : "Pending Doctor Update",
                    }

                ).ToList();

            return View(history);
        }
        // ================= BOOK APPOINTMENT =================

        public IActionResult BookAppointment()
        {
            ViewBag.Doctors =
                _context.Doctors.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult BookAppointment(
            int doctorId,
            DateTime appointmentDate
        )
        {
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            string userName =
                HttpContext.Session.GetString("UserName");

            if (userId == null)
            {
                TempData["Error"] =
                    "Please login first!";

                return RedirectToAction(
                    "Login",
                    "Home"
                );
            }

            var doctor =
                _context.Doctors.FirstOrDefault(
                    d => d.DoctorId == doctorId
                );

            if (doctor == null)
            {
                TempData["Error"] =
                    "Doctor not found!";

                return RedirectToAction(
                    "BookAppointment"
                );
            }

            DateTime today = DateTime.Today;

            // ==========================
            // PAST DATE CHECK
            // ==========================
            if (appointmentDate.Date < today)
            {
                TempData["Error"] =
                    "You cannot book past dates!";

                return RedirectToAction(
                    "BookAppointment"
                );
            }

            // ==========================
            // 2 MONTH LIMIT CHECK
            // ==========================
            if (appointmentDate.Date >
                today.AddMonths(2))
            {
                TempData["Error"] =
                    "You can only book up to 2 months ahead!";

                return RedirectToAction(
                    "BookAppointment"
                );
            }

            // ==========================
            // FIXED DAY CHECK (IMPORTANT FIX)
            // ==========================

            string selectedDay =
                appointmentDate.DayOfWeek
                    .ToString()
                    .Trim()
                    .ToLower();

            var availableDays =
                doctor.AvailableDays
                    .Split(',')
                    .Select(d => d.Trim().ToLower())
                    .ToList();

            if (!availableDays.Contains(selectedDay))
            {
                TempData["Error"] =
                    "Doctor not available on this day!";

                return RedirectToAction(
                    "BookAppointment"
                );
            }

            // ==========================
            // DAILY LIMIT CHECK
            // ==========================
            int totalAppointments =
                _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId
                        &&
                        a.AppointmentDate.Date ==
                        appointmentDate.Date
                    )
                    .Count();

            if (totalAppointments >=
                doctor.MaxPatientsPerDay)
            {
                TempData["Error"] =
                    "This day is fully booked!";

                return RedirectToAction(
                    "BookAppointment"
                );
            }

            // ==========================
            // CREATE APPOINTMENT
            // ==========================
            Appointment appointment =
                new Appointment
                {
                    DoctorId = doctorId,

                    UserId = userId.Value,

                    AppointmentDate =
                        appointmentDate,

                    Status = "Pending",

                    PatientName = userName,

                    DoctorName =
                        doctor.DoctorName
                };

            _context.Appointments.Add(appointment);

            _context.SaveChanges();

            TempData["Success"] =
                "Appointment booked successfully!";

            return RedirectToAction(
                "BookAppointment"
            );
        }

        // ================= AJAX =================

        public JsonResult GetDoctorDays(int doctorId)
        {
            var doctor = _context.Doctors
                .FirstOrDefault(d => d.DoctorId == doctorId);

            if (doctor == null || string.IsNullOrWhiteSpace(doctor.AvailableDays))
            {
                return Json(new string[] { });
            }

            var days = doctor.AvailableDays
                .Split(',')
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToList();

            return Json(days);
        }
        public JsonResult CheckAvailability(
            int doctorId,
            DateTime date
        )
        {
            var doctor =
                _context.Doctors.FirstOrDefault(
                    d => d.DoctorId == doctorId
                );

            if (doctor == null)
            {
                return Json("Invalid");
            }

            int count =
                _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId
                        &&
                        a.AppointmentDate.Date ==
                        date.Date
                    )
                    .Count();

            return Json(
                count >= doctor.MaxPatientsPerDay
                    ? "Full"
                    : "Available"
            );
        }

        // ================= CANCEL APPOINTMENT =================

        // GET
        public IActionResult CancelAppointment(
            string patientName,
            string email
        )
        {
            string sessionName =
                HttpContext.Session.GetString("UserName");

            string sessionEmail =
                HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(sessionName)
                ||
                string.IsNullOrEmpty(sessionEmail))
            {
                return RedirectToAction(
                    "Login",
                    "Home"
                );
            }

            if (!string.IsNullOrEmpty(patientName)
                &&
                !string.IsNullOrEmpty(email))
            {
                if (
                    patientName.ToLower().Trim()
                    !=
                    sessionName.ToLower().Trim()

                    ||

                    email.ToLower().Trim()
                    !=
                    sessionEmail.ToLower().Trim()
                )
                {
                    ViewBag.Message =
                        "This is not your account. Please use your own registered account.";

                    return View();
                }

                var user =
                    _context.Users.FirstOrDefault(
                        u =>
                            u.FirstName.ToLower().Trim()
                            ==
                            sessionName.ToLower().Trim()

                            &&

                            u.Email.ToLower().Trim()
                            ==
                            sessionEmail.ToLower().Trim()
                    );

                if (user != null)
                {
                    var appointments =
                        _context.Appointments
                            .Where(a =>
                                a.UserId == user.SystemId
                            )
                            .OrderByDescending(a =>
                                a.AppointmentDate
                            )
                            .ToList();

                    ViewBag.PatientName =
                        patientName;

                    ViewBag.Email =
                        email;

                    return View(appointments);
                }

                ViewBag.Message =
                    "User not found!";
            }

            return View();
        }

        // POST SEARCH
        [HttpPost]
        public IActionResult CancelAppointment(
            string patientName,
            string email,
            int? dummy = null
        )
        {
            if (string.IsNullOrEmpty(patientName)
                ||
                string.IsNullOrEmpty(email))
            {
                ViewBag.Message =
                    "Please enter both Name and Email!";

                return View();
            }

            return RedirectToAction(
                "CancelAppointment",
                new
                {
                    patientName,
                    email
                }
            );
        }

        // POST CANCEL
        [HttpPost]
        public IActionResult ConfirmCancel(
            int id,
            string patientName,
            string email
        )
        {
            string sessionName =
                HttpContext.Session.GetString("UserName");

            string sessionEmail =
                HttpContext.Session.GetString("UserEmail");

            if (
                patientName.ToLower().Trim()
                !=
                sessionName.ToLower().Trim()

                ||

                email.ToLower().Trim()
                !=
                sessionEmail.ToLower().Trim()
            )
            {
                TempData["Success"] =
                    "You cannot access another user's appointments.";

                return RedirectToAction(
                    "CancelAppointment"
                );
            }

            var appointment =
                _context.Appointments.Find(id);

            if (appointment != null)
            {
                // ONLY PENDING APPOINTMENTS CAN BE CANCELLED
                if (appointment.Status == "Pending")
                {
                    appointment.Status = "Cancelled";

                    _context.SaveChanges();

                    TempData["Success"] =
                        "Appointment cancelled successfully!";
                }
                else if (appointment.Status == "Confirmed")
                {
                    TempData["Success"] =
                        "Confirmed appointments cannot be cancelled.";
                }
                else
                {
                    TempData["Success"] =
                        "This appointment is already cancelled.";
                }
            }

            return RedirectToAction(
                "CancelAppointment",
                new
                {
                    patientName,
                    email
                }
            );
        }

        // ================= AI MEDICAL CHATBOT =================

        [HttpGet]
        public IActionResult ChatAI()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AskBot(string message)
        {
            try
            {
                // ============================================
                // VALIDATION
                // ============================================

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new
                    {
                        reply = "Please enter a message",
                        isEmergency = false
                    });
                }

                // ============================================
                // CREATE JSON PAYLOAD
                // ============================================

                var payload = new
                {
                    message = message.Trim()
                };

                var json = JsonSerializer.Serialize(payload);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                // ============================================
                // CALL FLASK API
                // ============================================

                HttpResponseMessage response;

                try
                {
                    response = await client.PostAsync(
                        "http://127.0.0.1:5000/chat",
                        content
                    );
                }
                catch (HttpRequestException)
                {
                    return Json(new
                    {
                        reply = "Flask backend not running. Start Python server on port 5000.",
                        isEmergency = false
                    });
                }
                catch (TaskCanceledException)
                {
                    return Json(new
                    {
                        reply = " AI response timeout. TinyLlama is still processing.",
                        isEmergency = false
                    });
                }

                // ============================================
                // HTTP ERROR CHECK
                // ============================================

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        reply = $" Flask API Error: HTTP {(int)response.StatusCode}",
                        isEmergency = false
                    });
                }

                // ============================================
                // READ RESPONSE
                // ============================================

                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("FLASK RESPONSE:");
                Console.WriteLine(result);

                string reply = "";
                bool isEmergency = false;

                // ============================================
                // JSON PARSE
                // ============================================

                try
                {
                    var obj = JsonNode.Parse(result);

                    if (obj != null)
                    {
                        reply =
                            obj["reply"]?.ToString()
                            ?? obj["response"]?.ToString()
                            ?? "";

                        isEmergency =
                            obj["isEmergency"]?.GetValue<bool>()
                            ?? obj["is_emergency"]?.GetValue<bool>()
                            ?? false;
                    }
                }
                catch (Exception jsonEx)
                {
                    Console.WriteLine("JSON PARSE ERROR:");
                    Console.WriteLine(jsonEx.Message);

                    reply = result;
                }

                // ============================================
                // EMPTY RESPONSE FIX
                // ============================================

                if (string.IsNullOrWhiteSpace(reply))
                {
                    reply = " AI could not generate a response. Please try again.";
                }

                // ============================================
                // RETURN FINAL JSON
                // ============================================

                return Json(new
                {
                    reply,
                    isEmergency
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("SYSTEM ERROR:");
                Console.WriteLine(ex);

                return Json(new
                {
                    reply = " System error occurred while processing request.",
                    isEmergency = false
                });
            }
        }

        // ================= LOGOUT =================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login",
                "Home"
            );
        }
    }
}