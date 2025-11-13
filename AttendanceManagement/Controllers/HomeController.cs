using System.Diagnostics;
using AttendanceManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;

namespace AttendanceManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.UserName = user.FullName;
                    ViewBag.UserRole = user.Role.ToString();

                    if (user.Role == UserRole.Teacher)
                    {
                        ViewBag.ClassCount = await _context.Classes
                            .CountAsync(c => c.TeacherId == user.Id && c.IsActive);
                    }
                    else if (user.Role == UserRole.Student)
                    {
                        ViewBag.ClassCount = await _context.Enrollments
                            .CountAsync(e => e.StudentId == user.Id && e.IsActive);
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
