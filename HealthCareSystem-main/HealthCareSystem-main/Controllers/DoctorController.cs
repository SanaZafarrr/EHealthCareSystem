
using HealthCareSystem.Data;
using HealthCareSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace HealthCareSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly AppDbContext _context;

        public DoctorController(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // ================= HELLO DOCTOR =======================
        // ======================================================

        public IActionResult HelloDoctor()
        {
            if (HttpContext.Session.GetString("UserType") != "Doctor")
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.DoctorName =
                HttpContext.Session.GetString("DoctorName");

            return View();
        }

        // ======================================================
        // ================= DOCTOR PROFILE =====================
        // ======================================================

        public IActionResult DoctorProfile()
        {
            int? doctorId =
                HttpContext.Session.GetInt32("DoctorId");

            if (doctorId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var doctor = _context.Doctors
                .FirstOrDefault(d => d.DoctorId == doctorId);

            return View(doctor);
        }

        // ======================================================
        // ================= MANAGE APPOINTMENTS ================
        // ======================================================

        public IActionResult ManageAppointments()
        {
            string doctorName =
                HttpContext.Session.GetString("DoctorName");

            if (doctorName == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var appointments = _context.Appointments
                .Where(a => a.DoctorName == doctorName)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(appointments);
        }

        // ======================================================
        // ================= CONFIRM APPOINTMENT ================
        // ======================================================

        [HttpPost]
        public IActionResult ConfirmAppointment(int id)
        {
            var appointment = _context.Appointments
                .FirstOrDefault(a => a.Id == id);

            if (appointment != null)
            {
                // ONLY PENDING APPOINTMENTS CAN BE CONFIRMED
                if (appointment.Status == "Pending")
                {
                    appointment.Status = "Confirmed";

                    _context.SaveChanges();

                    TempData["Success"] =
                        "Appointment confirmed successfully!";
                }
                else
                {
                    TempData["Delete"] =
                        "Only pending appointments can be confirmed.";
                }
            }

            return RedirectToAction("ManageAppointments");
        }

        // ======================================================
        // ================= DELETE APPOINTMENT =================
        // ======================================================

        [HttpPost]
        public IActionResult DeleteAppointment(int id)
        {
            var appointment = _context.Appointments
                .FirstOrDefault(a => a.Id == id);

            if (appointment != null)
            {
                // ONLY PENDING APPOINTMENTS CAN BE CANCELLED
                if (appointment.Status == "Pending")
                {
                    appointment.Status = "Cancelled";

                    _context.SaveChanges();

                    TempData["Delete"] =
                        "Appointment cancelled successfully!";
                }
                else
                {
                    TempData["Delete"] =
                        "Confirmed appointments cannot be cancelled.";
                }
            }

            return RedirectToAction("ManageAppointments");
        }
        // ======================================================
        // ================= ADD PRESCRIPTION PAGE ==============
        // ======================================================

        public IActionResult AddPrescription()
        {
            int? doctorId =
                HttpContext.Session.GetInt32("DoctorId");

            if (doctorId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            string doctorName =
                HttpContext.Session.GetString("DoctorName");

            var confirmedAppointments = _context.Appointments
                .Where(a =>
                    a.DoctorName == doctorName
                    && a.Status == "Confirmed")
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(confirmedAppointments);
        }

        // ======================================================
        // ================= CREATE / EDIT PAGE =================
        // ======================================================

        public IActionResult CreatePrescription(int id)
        {
            // id = AppointmentId

            var appointment = _context.Appointments
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // ==================================================
            // CHECK IF PRESCRIPTION ALREADY EXISTS
            // ==================================================

            var existingPrescription = _context.Prescriptions
                .FirstOrDefault(p =>
                    p.AppointmentId == id);

            // ==================================================
            // EDIT MODE
            // ==================================================

            if (existingPrescription != null)
            {
                return View(existingPrescription);
            }

            // ==================================================
            // ADD MODE
            // ==================================================

            Prescription model = new Prescription()
            {
                AppointmentId = appointment.Id,

                UserId = appointment.UserId,

                DoctorId =
                    HttpContext.Session.GetInt32("DoctorId") ?? 0,

                PatientName = appointment.PatientName,

                DoctorName =
                    HttpContext.Session.GetString("DoctorName")
            };

            return View(model);
        }

        // ======================================================
        // ================= SAVE / UPDATE ======================
        // ======================================================

        [HttpPost]
        public IActionResult CreatePrescription(Prescription model)
        {
            var existingPrescription = _context.Prescriptions
                .FirstOrDefault(p =>
                    p.AppointmentId == model.AppointmentId);

            // ==================================================
            // UPDATE EXISTING PRESCRIPTION
            // ==================================================

            if (existingPrescription != null)
            {
                existingPrescription.Diagnosis =
                    model.Diagnosis;

                existingPrescription.Medicines =
                    model.Medicines;

                existingPrescription.Notes =
                    model.Notes;

                _context.SaveChanges();

                TempData["Success"] =
                    "Prescription updated successfully!";
            }

            // ==================================================
            // ADD NEW PRESCRIPTION
            // ==================================================

            else
            {
                var appointment = _context.Appointments
                    .FirstOrDefault(a =>
                        a.Id == model.AppointmentId);

                if (appointment == null)
                {
                    return NotFound();
                }

                model.PatientName =
                    appointment.PatientName;

                model.DoctorName =
                    HttpContext.Session.GetString("DoctorName");

                model.DoctorId =
                    HttpContext.Session.GetInt32("DoctorId") ?? 0;

                model.UserId =
                    appointment.UserId;

                _context.Prescriptions.Add(model);

                _context.SaveChanges();

                TempData["Success"] =
                    "Prescription added successfully!";
            }

            return RedirectToAction("AddPrescription");
        }
        //  PatientRecords
        public IActionResult PatientRecords()
        {
            string doctorName = HttpContext.Session.GetString("DoctorName");

            if (doctorName == null)
                return RedirectToAction("Login", "Home");

            var patients = _context.Appointments
                .Where(a => a.DoctorName == doctorName)
                .Select(a => new PatientRecordViewModel
                {
                    UserId = a.UserId,
                    PatientName = a.PatientName
                })
                .GroupBy(p => p.UserId)
                .Select(g => g.First())
                .ToList();

            return View(patients);
        }
        //   PatientReport 
        public IActionResult PatientReport(int id)
        {
            int doctorId = HttpContext.Session.GetInt32("DoctorId") ?? 0;

            if (doctorId == 0)
                return RedirectToAction("Login", "Home");

            // ✅ ONLY THIS DOCTOR + THIS PATIENT
            var appointments = _context.Appointments
                .Where(a =>
                    a.UserId == id &&
                    a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            var prescriptions = _context.Prescriptions
                .Where(p =>
                    p.UserId == id &&
                    p.DoctorId == doctorId)
                .OrderByDescending(p => p.PrescriptionId)
                .ToList();

            ViewBag.Appointments = appointments;
            ViewBag.Prescriptions = prescriptions;

            ViewBag.PatientName =
                appointments.FirstOrDefault()?.PatientName;

            return View();
        }
    }
}



