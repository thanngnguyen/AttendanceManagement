using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;
using AttendanceManagement.ViewModels;

namespace AttendanceManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }

                    _logger.LogInformation("User logged in.");
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    // ✅ Redirect Admin tới Dashboard
                    if (user.Role == UserRole.Admin)
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    
                    // ✅ Redirect Teacher/Student tới Home
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    StudentId = model.StudentId,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role == "Teacher" ? UserRole.Teacher : UserRole.Student,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    _logger.LogInformation("Người dùng đã tạo một tài khoản mới bằng mật khẩu.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Người dùng đã đăng xuất.");
            return RedirectToAction("Index", "Home");
        }

        // ==================== PROFILE MANAGEMENT ====================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Calculate statistics
            int totalClasses = 0;
            int totalAttendances = 0;
            int totalAssignments = 0;
            double attendanceRate = 0;

            if (user.Role == UserRole.Student)
            {
                totalClasses = await _context.Enrollments
                    .CountAsync(e => e.StudentId == user.Id && e.IsActive);

                totalAttendances = await _context.AttendanceRecords
                    .CountAsync(ar => ar.StudentId == user.Id);

                var totalSlots = await _context.AttendanceSlots
                    .Where(s => s.Class.Enrollments.Any(e => e.StudentId == user.Id && e.IsActive))
                    .CountAsync();

                attendanceRate = totalSlots > 0 ? (double)totalAttendances / totalSlots * 100 : 0;

                totalAssignments = await _context.Submissions
                    .CountAsync(s => s.StudentId == user.Id);
            }
            else if (user.Role == UserRole.Teacher)
            {
                totalClasses = await _context.Classes
                    .CountAsync(c => c.TeacherId == user.Id && c.IsActive);

                totalAssignments = await _context.Assignments
                    .CountAsync(a => a.Class.TeacherId == user.Id);
            }

            var model = new ProfileViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                StudentId = user.StudentId,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                Bio = user.Bio,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                TotalClasses = totalClasses,
                TotalAttendances = totalAttendances,
                TotalAssignments = totalAssignments,
                AttendanceRate = attendanceRate
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                StudentId = user.StudentId,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                CurrentProfileImageUrl = user.ProfileImageUrl
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.StudentId = model.StudentId;
            user.PhoneNumber = model.PhoneNumber;
            user.Bio = model.Bio;

            // Handle profile image upload
            if (model.ProfileImage != null)
            {
                var imageUrl = await SaveProfileImageAsync(model.ProfileImage);
                user.ProfileImageUrl = imageUrl;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.CurrentProfileImageUrl = user.ProfileImageUrl;
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("Người dùng đã thay đổi mật khẩu thành công.");
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "PasswordMismatch")
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AccountSettingsViewModel
            {
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                AccessFailedCount = user.AccessFailedCount,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ==================== HELPER METHODS ====================

        private async Task<string> SaveProfileImageAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/profiles/{uniqueFileName}";
        }
    }
}
