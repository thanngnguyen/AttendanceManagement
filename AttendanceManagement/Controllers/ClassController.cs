using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;
using AttendanceManagement.ViewModels;

namespace AttendanceManagement.Controllers
{
    [Authorize]
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ClassController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Class
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            List<Class> classes;

            if (user.Role == UserRole.Teacher)
            {
                classes = await _context.Classes
                    .Where(c => c.TeacherId == user.Id && c.IsActive)
                    .Include(c => c.Enrollments)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                classes = await _context.Enrollments
                    .Where(e => e.StudentId == user.Id && e.IsActive)
                    .Include(e => e.Class)
                        .ThenInclude(c => c.Teacher)
                    .Select(e => e.Class)
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }

            return View(classes);
        }

        // GET: Class/Create
        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Class/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create(CreateClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var classCode = GenerateClassCode();

                var newClass = new Class
                {
                    ClassName = model.ClassName,
                    ClassCode = classCode,
                    Description = model.Description,
                    Subject = model.Subject,
                    Room = model.Room,
                    TeacherId = user.Id,
                    ClassLatitude = model.ClassLatitude,
                    ClassLongitude = model.ClassLongitude,
                    AllowedDistanceMeters = model.AllowedDistanceMeters,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tạo lớp học thành công! Mã lớp: {classCode}";
                return RedirectToAction(nameof(Detail), new { id = newClass.ClassId });
            }

            return View(model);
        }

        // GET: Class/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var classEntity = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.ClassId == id && c.IsActive);

            if (classEntity == null)
            {
                return NotFound();
            }

            var isTeacher = classEntity.TeacherId == user.Id;
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassId == id && e.StudentId == user.Id && e.IsActive);

            if (!isTeacher && !isEnrolled)
            {
                return Forbid();
            }

            var recentPosts = await _context.Posts
                .Where(p => p.ClassId == id)
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var upcomingAssignments = await _context.Assignments
                .Where(a => a.ClassId == id && a.IsPublished && (!a.DueDate.HasValue || a.DueDate > DateTime.Now))
                .OrderBy(a => a.DueDate)
                .Take(5)
                .ToListAsync();

            var recentSlots = await _context.AttendanceSlots
                .Where(s => s.ClassId == id && s.IsActive)
                .OrderByDescending(s => s.StartTime)
                .Take(5)
                .ToListAsync();

            var model = new ClassDetailViewModel
            {
                Class = classEntity,
                IsTeacher = isTeacher,
                IsEnrolled = isEnrolled,
                TotalStudents = classEntity.Enrollments.Count(e => e.IsActive),
                RecentPosts = recentPosts,
                UpcomingAssignments = upcomingAssignments,
                RecentSlots = recentSlots
            };

            return View(model);
        }

        // GET: Class/Join
        public IActionResult Join()
        {
            return View();
        }

        // POST: Class/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(JoinClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                if (user.Role != UserRole.Student)
                {
                    ModelState.AddModelError("", "Chỉ sinh viên mới có thể tham gia lớp học.");
                    return View(model);
                }

                var classEntity = await _context.Classes
                    .FirstOrDefaultAsync(c => c.ClassCode == model.ClassCode && c.IsActive);

                if (classEntity == null)
                {
                    ModelState.AddModelError("ClassCode", "Mã lớp không tồn tại.");
                    return View(model);
                }

                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.ClassId == classEntity.ClassId && e.StudentId == user.Id);

                if (existingEnrollment != null)
                {
                    if (existingEnrollment.IsActive)
                    {
                        ModelState.AddModelError("", "Bạn đã tham gia lớp học này rồi.");
                        return View(model);
                    }
                    else
                    {
                        existingEnrollment.IsActive = true;
                        existingEnrollment.EnrolledAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    var enrollment = new Enrollment
                    {
                        ClassId = classEntity.ClassId,
                        StudentId = user.Id,
                        EnrolledAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Enrollments.Add(enrollment);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tham gia lớp {classEntity.ClassName} thành công!";
                return RedirectToAction(nameof(Detail), new { id = classEntity.ClassId });
            }

            return View(model);
        }

        // GET: Class/Members/5
        public async Task<IActionResult> Members(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var classEntity = await _context.Classes
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.ClassId == id && c.IsActive);

            if (classEntity == null)
            {
                return NotFound();
            }

            var isTeacher = classEntity.TeacherId == user.Id;
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassId == id && e.StudentId == user.Id && e.IsActive);

            if (!isTeacher && !isEnrolled)
            {
                return Forbid();
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.ClassId == id && e.IsActive)
                .Include(e => e.Student)
                .OrderBy(e => e.Student.FullName)
                .ToListAsync();

            var totalSlots = await _context.AttendanceSlots
                .CountAsync(s => s.ClassId == id && s.IsActive);

            var students = new List<StudentMemberViewModel>();

            foreach (var enrollment in enrollments)
            {
                var attendanceCount = await _context.AttendanceRecords
                    .CountAsync(ar => ar.StudentId == enrollment.StudentId &&
                                     ar.AttendanceSlot.ClassId == id);

                students.Add(new StudentMemberViewModel
                {
                    UserId = enrollment.Student.Id,
                    FullName = enrollment.Student.FullName,
                    Email = enrollment.Student.Email ?? string.Empty,
                    StudentId = enrollment.Student.StudentId,
                    EnrolledAt = enrollment.EnrolledAt,
                    AttendanceCount = attendanceCount,
                    TotalSlots = totalSlots,
                    AttendanceRate = totalSlots > 0 ? (double)attendanceCount / totalSlots * 100 : 0
                });
            }

            var model = new ClassMembersViewModel
            {
                Class = classEntity,
                Teacher = classEntity.Teacher,
                Students = students
            };

            return View(model);
        }

        // POST: Class/CreatePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreatePostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var classEntity = await _context.Classes
                    .FirstOrDefaultAsync(c => c.ClassId == model.ClassId && c.IsActive);

                if (classEntity == null)
                {
                    return NotFound();
                }

                var isTeacher = classEntity.TeacherId == user.Id;
                var isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.ClassId == model.ClassId && e.StudentId == user.Id && e.IsActive);

                if (!isTeacher && !isEnrolled)
                {
                    return Forbid();
                }

                string? attachmentUrl = null;
                if (model.Attachment != null)
                {
                    attachmentUrl = await SaveFileAsync(model.Attachment, "posts");
                }

                var post = new Post
                {
                    ClassId = model.ClassId,
                    AuthorId = user.Id,
                    Content = model.Content,
                    AttachmentUrl = attachmentUrl,
                    Type = model.Type,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng bài thành công!";
                return RedirectToAction(nameof(Detail), new { id = model.ClassId });
            }

            return RedirectToAction(nameof(Detail), new { id = model.ClassId });
        }

        // Helper Methods
        private string GenerateClassCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;

            do
            {
                code = new string(Enumerable.Repeat(chars, 7)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (_context.Classes.Any(c => c.ClassCode == code));

            return code;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }
    }
}
