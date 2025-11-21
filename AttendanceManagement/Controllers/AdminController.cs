using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;
using AttendanceManagement.ViewModels;

namespace AttendanceManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===== DASHBOARD =====
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalTeachers = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher);
            var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
            var totalClasses = await _context.Classes.CountAsync();
            var totalAttendanceRecords = await _context.AttendanceRecords.CountAsync();
            var totalFlaggedRecords = await _context.AttendanceRecords.CountAsync(ar => ar.IsFlagged);
            var totalLeaveRequests = await _context.LeaveRequests.CountAsync();

            // Calculate average attendance rate
            var totalEnrollments = await _context.Enrollments.CountAsync(e => e.IsActive);
            var averageAttendanceRate = totalEnrollments > 0 
                ? (double)totalAttendanceRecords / totalEnrollments * 100 
                : 0;

            // Recent users
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new UserListItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    StudentId = u.StudentId,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    IsActive = u.LockoutEnabled && u.LockoutEnd == null
                })
                .ToListAsync();

            // Top classes
            var topClasses = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.AttendanceSlots)
                .Take(5)
                .Select(c => new ClassStatistics
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    ClassCode = c.ClassCode,
                    TeacherName = c.Teacher.FullName,
                    EnrolledStudents = c.Enrollments.Count(e => e.IsActive),
                    TotalSlots = c.AttendanceSlots.Count,
                    AttendanceRecords = 0,
                    AverageAttendanceRate = 0,
                    FlaggedRecords = 0,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            // Recent flags
            var recentFlags = await _context.AttendanceFlags
                .Include(af => af.AttendanceRecord)
                    .ThenInclude(ar => ar.AttendanceSlot)
                        .ThenInclude(s => s.Class)
                .OrderByDescending(af => af.FlaggedAt)
                .Take(5)
                .Select(af => new AttendanceFlagSummary
                {
                    FlagId = af.FlagId,
                    RecordId = af.RecordId,
                    StudentName = af.AttendanceRecord.StudentName,
                    StudentCode = af.AttendanceRecord.StudentCode,
                    SlotName = af.AttendanceRecord.AttendanceSlot.SlotName,
                    ClassName = af.AttendanceRecord.AttendanceSlot.Class.ClassName,
                    Type = af.Type,
                    Reason = af.Reason,
                    FlaggedAt = af.FlaggedAt,
                    IsResolved = af.IsResolved
                })
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalTeachers = totalTeachers,
                TotalStudents = totalStudents,
                TotalClasses = totalClasses,
                TotalAttendanceRecords = totalAttendanceRecords,
                TotalFlaggedRecords = totalFlaggedRecords,
                TotalLeaveRequests = totalLeaveRequests,
                AverageAttendanceRate = averageAttendanceRate,
                RecentUsers = recentUsers,
                TopClasses = topClasses,
                RecentFlags = recentFlags
            };

            return View(model);
        }

        // ===== USER MANAGEMENT =====
        public async Task<IActionResult> Users(UserRole? role = null, int page = 1)
        {
            var pageSize = 10;
            var query = _context.Users.AsQueryable();

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    StudentId = u.StudentId,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    IsActive = !u.LockoutEnabled || u.LockoutEnd == null,
                    EnrolledClassesCount = u.Enrollments.Count(e => e.IsActive),
                    AttendanceRecordsCount = u.AttendanceRecords.Count
                })
                .ToListAsync();

            ViewBag.CurrentRole = role;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalCount = totalCount;

            return View(users);
        }

        public async Task<IActionResult> UserDetail(string id)
        {
            var user = await _context.Users
                .Include(u => u.Enrollments)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.Teacher)
                .Include(u => u.AttendanceRecords)
                    .ThenInclude(ar => ar.AttendanceSlot)
                .Include(u => u.LeaveRequests)
                    .ThenInclude(lr => lr.AttendanceSlot)
                .Include(u => u.TeachingClasses)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var model = new UserDetailViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                StudentId = user.StudentId,
                Bio = user.Bio,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                ProfileImageUrl = user.ProfileImageUrl,
                TeachingClasses = user.TeachingClasses.Select(tc => new ClassInfo
                {
                    ClassId = tc.ClassId,
                    ClassName = tc.ClassName,
                    ClassCode = tc.ClassCode,
                    StudentCount = tc.Enrollments.Count(e => e.IsActive),
                    SlotCount = tc.AttendanceSlots.Count,
                    IsActive = tc.IsActive
                }).ToList(),
                EnrolledClasses = user.Enrollments.Select(e => new EnrollmentInfo
                {
                    EnrollmentId = e.EnrollmentId,
                    ClassId = e.ClassId,
                    ClassName = e.Class.ClassName,
                    ClassCode = e.Class.ClassCode,
                    TeacherName = e.Class.Teacher?.FullName ?? "N/A",
                    EnrolledAt = e.EnrolledAt,
                    IsActive = e.IsActive
                }).ToList(),
                AttendanceHistory = user.AttendanceRecords.Select(ar => new AttendanceInfo
                {
                    RecordId = ar.RecordId,
                    SlotId = ar.SlotId,
                    SlotName = ar.AttendanceSlot.SlotName,
                    CheckInTime = ar.CheckInTime,
                    Status = ar.Status,
                    DistanceFromClass = ar.DistanceFromClass,
                    IsFlagged = ar.IsFlagged,
                    FlagReason = ar.FlagReason
                }).ToList(),
                LeaveRequests = user.LeaveRequests.Select(lr => new LeaveRequestInfo
                {
                    RequestId = lr.RequestId,
                    SlotId = lr.SlotId,
                    SlotName = lr.AttendanceSlot.SlotName,
                    Reason = lr.Reason,
                    RequestedAt = lr.RequestedAt,
                    Status = lr.Status,
                    ReviewedAt = lr.ReviewedAt,
                    ReviewNote = lr.ReviewNote
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                StudentId = user.StudentId,
                Bio = user.Bio,
                Role = user.Role
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.StudentId = model.StudentId;
            user.Bio = model.Bio;
            user.Role = model.Role;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Update roles in Identity
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role.ToString());

                TempData["SuccessMessage"] = "C?p nh?t thông tin ng??i dùng thành công!";
                return RedirectToAction("UserDetail", new { id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ===== CLASS MANAGEMENT =====
        public async Task<IActionResult> Classes(int page = 1)
        {
            var pageSize = 10;
            var totalCount = await _context.Classes.CountAsync();

            var classes = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.AttendanceSlots)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClassManagementViewModel
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    ClassCode = c.ClassCode,
                    Subject = c.Subject ?? string.Empty,
                    Room = c.Room ?? string.Empty,
                    Description = c.Description,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher.FullName,
                    StudentCount = c.Enrollments.Count(e => e.IsActive),
                    SlotCount = c.AttendanceSlots.Count,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalCount = totalCount;

            return View(classes);
        }

        public async Task<IActionResult> ClassDetail(int id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.AttendanceSlots)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classEntity == null)
                return NotFound();

            var model = new ClassManagementViewModel
            {
                ClassId = classEntity.ClassId,
                ClassName = classEntity.ClassName,
                ClassCode = classEntity.ClassCode,
                Subject = classEntity.Subject ?? string.Empty,
                Room = classEntity.Room ?? string.Empty,
                Description = classEntity.Description,
                TeacherId = classEntity.TeacherId,
                TeacherName = classEntity.Teacher.FullName,
                StudentCount = classEntity.Enrollments.Count(e => e.IsActive),
                SlotCount = classEntity.AttendanceSlots.Count,
                IsActive = classEntity.IsActive,
                CreatedAt = classEntity.CreatedAt
            };

            ViewBag.Students = classEntity.Enrollments.Where(e => e.IsActive).Select(e => e.Student).ToList();
            ViewBag.Slots = classEntity.AttendanceSlots.OrderByDescending(s => s.StartTime).ToList();

            return View(model);
        }

        // ===== ATTENDANCE MANAGEMENT =====
        public async Task<IActionResult> Attendance(AttendanceStatus? status = null, int page = 1)
        {
            var pageSize = 20;
            var query = _context.AttendanceRecords.AsQueryable();

            // ✅ Filter theo status nếu có
            if (status.HasValue)
                query = query.Where(ar => ar.Status == status.Value);

            var totalCount = await query.CountAsync();

            var records = await query
                .Include(ar => ar.AttendanceSlot)
                    .ThenInclude(s => s.Class)
                .OrderByDescending(ar => ar.CheckInTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ar => new AttendanceInfo
                {
                    RecordId = ar.RecordId,
                    SlotId = ar.SlotId,
                    StudentName = ar.StudentName,
                    StudentCode = ar.StudentCode,
                    SlotName = ar.AttendanceSlot.SlotName,
                    CheckInTime = ar.CheckInTime,
                    Status = ar.Status,
                    DistanceFromClass = ar.DistanceFromClass,
                    IsFlagged = ar.IsFlagged,
                    FlagReason = ar.FlagReason
                })
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CurrentStatus = status;

            var stats = new AttendanceStatisticsViewModel
            {
                TotalRecords = totalCount,
                PresentCount = await _context.AttendanceRecords.CountAsync(ar => ar.Status == AttendanceStatus.Present),
                LateCount = await _context.AttendanceRecords.CountAsync(ar => ar.Status == AttendanceStatus.Late),
                AbsentCount = await _context.AttendanceRecords.CountAsync(ar => ar.Status == AttendanceStatus.Absent),
                FlaggedCount = await _context.AttendanceRecords.CountAsync(ar => ar.IsFlagged)
            };

            ViewBag.Statistics = stats;
            return View(records);
        }

        // ===== FLAGS MANAGEMENT =====
        public async Task<IActionResult> Flags(int page = 1)
        {
            var pageSize = 10;
            var totalCount = await _context.AttendanceFlags.CountAsync();

            var flags = await _context.AttendanceFlags
                .Include(af => af.AttendanceRecord)
                    .ThenInclude(ar => ar.AttendanceSlot)
                        .ThenInclude(s => s.Class)
                .OrderByDescending(af => af.FlaggedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(af => new AttendanceFlagSummary
                {
                    FlagId = af.FlagId,
                    RecordId = af.RecordId,
                    StudentName = af.AttendanceRecord.StudentName,
                    StudentCode = af.AttendanceRecord.StudentCode,
                    SlotName = af.AttendanceRecord.AttendanceSlot.SlotName,
                    ClassName = af.AttendanceRecord.AttendanceSlot.Class.ClassName,
                    Type = af.Type,
                    Reason = af.Reason,
                    FlaggedAt = af.FlaggedAt,
                    IsResolved = af.IsResolved
                })
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalCount = totalCount;

            return View(flags);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveFlag(int flagId)
        {
            var flag = await _context.AttendanceFlags.FindAsync(flagId);
            if (flag == null)
                return NotFound();

            flag.IsResolved = !flag.IsResolved;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = flag.IsResolved ? "Đánh dấu vi phạm đã được giải quyết!" : "Bỏ đánh dấu vi phạm đã giải quyết!";
            return RedirectToAction("Flags");
        }

        // ===== LEAVE REQUESTS MANAGEMENT =====
        public async Task<IActionResult> LeaveRequests(LeaveRequestStatus? status = null, int page = 1)
        {
            var pageSize = 10;
            var query = _context.LeaveRequests
                .Include(lr => lr.AttendanceSlot)
                    .ThenInclude(s => s.Class)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(lr => lr.Status == status.Value);

            var totalCount = await query.CountAsync();
            var requests = await query
                .OrderByDescending(lr => lr.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(lr => new LeaveRequestManagementViewModel
                {
                    RequestId = lr.RequestId,
                    StudentName = lr.StudentName,
                    StudentCode = lr.StudentCode,
                    Email = lr.Email,
                    ClassName = lr.AttendanceSlot.Class.ClassName,
                    SlotName = lr.AttendanceSlot.SlotName,
                    Reason = lr.Reason,
                    EvidenceUrl = lr.EvidenceUrl,
                    RequestedAt = lr.RequestedAt,
                    Status = lr.Status,
                    ReviewedAt = lr.ReviewedAt,
                    ReviewNote = lr.ReviewNote,
                    SlotId = lr.SlotId
                })
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalCount = totalCount;

            return View(requests);
        }

        // ===== REPORTS =====
        public async Task<IActionResult> Reports()
        {
            // User statistics
            var totalUsers = await _context.Users.CountAsync();
            var teacherCount = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher);
            var studentCount = await _context.Users.CountAsync(u => u.Role == UserRole.Student);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TeacherCount = teacherCount;
            ViewBag.StudentCount = studentCount;

            // Class statistics
            ViewBag.TotalClasses = await _context.Classes.CountAsync();
            ViewBag.ActiveClasses = await _context.Classes.CountAsync(c => c.IsActive);

            // Attendance statistics
            ViewBag.TotalAttendanceRecords = await _context.AttendanceRecords.CountAsync();
            ViewBag.FlaggedRecords = await _context.AttendanceRecords.CountAsync(ar => ar.IsFlagged);

            // Leave requests
            ViewBag.PendingLeaveRequests = await _context.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Pending);
            ViewBag.ApprovedLeaveRequests = await _context.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Approved);

            return View();
        }
    }
}
