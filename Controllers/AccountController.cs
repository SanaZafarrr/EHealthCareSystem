 using Microsoft.AspNetCore.Mvc;

namespace HealthCareSystem.Controllers
{
   public class AccountController : Controller
   {
       // Show Logout Confirmation Page
       public IActionResult Logout()
       {
           return View("~/Views/Admin/Logout.cshtml");
       }


       // Confirm Logout
       public IActionResult ConfirmLogout()
       {
           // Clear session
           HttpContext.Session.Clear();

           // Redirect to LOGIN page in HOME controller
           return RedirectToAction("Login", "Home");
       }
   }
}  




