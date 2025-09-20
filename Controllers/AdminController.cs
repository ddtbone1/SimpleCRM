using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SimpleCRM.Models;

namespace SimpleCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Agents()
        {
            return RedirectToAction("Index", "Agents");
        }

        public IActionResult Timesheet()
        {
            return RedirectToAction("Index", "Timesheet");
        }
    }
}
