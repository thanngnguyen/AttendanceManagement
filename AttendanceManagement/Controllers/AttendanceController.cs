using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;
using AttendanceManagement.ViewModels;
using AttendanceManagement.Helpers;

namespace AttendanceManagement.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public AttendanceController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _configuration = configuration;
        }

        // GET: Attendance/CreateSlot/5
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> CreateSlot(int classId)
        {
            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (classEntity.TeacherId != user?.Id)
            {
                return Forbid();
            }

            var model = new CreateAttendanceSlotViewModel
            {
                ClassId = classId,
                SlotLatitude = classEntity.ClassLatitude ?? 0,
                SlotLongitude = classEntity.ClassLongitude ?? 0,
                AllowedDistanceMeters = classEntity.AllowedDistanceMeters
            };

            ViewBag.ClassName = classEntity.ClassName;
            ViewBag.HasClassLocation = classEntity.ClassLatitude.HasValue && classEntity.ClassLongitude.HasValue 
                && classEntity.ClassLatitude.Value != 0 && classEntity.ClassLongitude.Value != 0;
            
            return View(model);
        }

        // POST: Attendance/CreateSlot
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> CreateSlot(CreateAttendanceSlotViewModel model)
        {
            if (ModelState.IsValid)
            {
                var classEntity = await _context.Classes.FindAsync(model.ClassId);
                if (classEntity == null) return NotFound();

                var user = await _userManager.GetUserAsync(User);
                if (classEntity.TeacherId != user?.Id) return Forbid();

                // Làm tròn tọa độ đến 6 chữ số thập phân để đảm bảo tính nhất quán
                double roundedLat = Math.Round(model.SlotLatitude, 6);
                double roundedLng = Math.Round(model.SlotLongitude, 6);

                // Xác thực phạm vi vĩ độ/kinh độ
                if (Math.Abs(roundedLat) > 90 || Math.Abs(roundedLng) > 180)
                {
                    ModelState.AddModelError("", $"Tọa độ không hợp lệ! Latitude phải trong khoảng [-90, 90], Longitude trong khoảng [-180, 180].");
                    ViewBag.ClassName = classEntity.ClassName;
                    return View(model);
                }

                var slot = new AttendanceSlot
                {
                    ClassId = model.ClassId,
                    SlotName = model.SlotName,
                    Description = model.Description,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    SlotLatitude = roundedLat,
                    SlotLongitude = roundedLng,
                    AllowedDistanceMeters = model.AllowedDistanceMeters,
                    IsActive = true,
                    CreatedAt = DateTimeHelper.GetVietnamNow()
                };

                _context.AttendanceSlots.Add(slot);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tạo phiên điểm danh thành công!";
                return RedirectToAction("Detail", "Class", new { id = model.ClassId });
            }

            var classForView = await _context.Classes.FindAsync(model.ClassId);
            ViewBag.ClassName = classForView?.ClassName;
            return View(model);
        }

        // GET: Attendance/SlotDetail/5
        public async Task<IActionResult> SlotDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var slot = await _context.AttendanceSlots
                .Include(s => s.Class)
                    .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            var isTeacher = slot.Class.TeacherId == user.Id;
            var isAdmin = user.Role == UserRole.Admin;
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassId == slot.ClassId && e.StudentId == user.Id && e.IsActive);

            // ✅ Admin hoặc Teacher lớp này hoặc sinh viên đã đăng ký
            if (!isAdmin && !isTeacher && !isEnrolled)
            {
                return Forbid();
            }

            var hasCheckedIn = await _context.AttendanceRecords
                .AnyAsync(ar => ar.SlotId == id && ar.StudentId == user.Id);

            var hasRequestedLeave = await _context.LeaveRequests
                .AnyAsync(lr => lr.SlotId == id && lr.StudentId == user.Id);

            var myAttendance = await _context.AttendanceRecords
                .Include(ar => ar.Flags)
                .FirstOrDefaultAsync(ar => ar.SlotId == id && ar.StudentId == user.Id);

            var myLeaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(lr => lr.SlotId == id && lr.StudentId == user.Id);

            var attendanceRecords = await _context.AttendanceRecords
                .Where(ar => ar.SlotId == id)
                .Include(ar => ar.Student)
                .Include(ar => ar.Flags)
                .OrderBy(ar => ar.CheckInTime)
                .Select(ar => new AttendanceRecordViewModel
                {
                    RecordId = ar.RecordId,
                    StudentName = ar.StudentName,
                    StudentCode = ar.StudentCode,
                    Email = ar.Email,
                    PhoneNumber = ar.PhoneNumber,
                    Latitude = ar.Latitude,
                    Longitude = ar.Longitude,
                    DistanceFromClass = ar.DistanceFromClass,
                    CheckInTime = ar.CheckInTime,
                    Status = ar.Status,
                    IsFlagged = ar.IsFlagged,
                    FlagReason = ar.FlagReason,
                    Flags = ar.Flags.ToList()
                })
                .ToListAsync();

            var leaveRequests = await _context.LeaveRequests
                .Where(lr => lr.SlotId == id)
                .Include(lr => lr.Student)
                .OrderBy(lr => lr.RequestedAt)
                .Select(lr => new LeaveRequestViewModel
                {
                    RequestId = lr.RequestId,
                    StudentName = lr.StudentName,
                    StudentCode = lr.StudentCode,
                    Email = lr.Email,
                    PhoneNumber = lr.PhoneNumber,
                    Reason = lr.Reason,
                    EvidenceUrl = lr.EvidenceUrl,
                    RequestedAt = lr.RequestedAt,
                    Status = lr.Status,
                    ReviewedAt = lr.ReviewedAt,
                    ReviewNote = lr.ReviewNote
                })
                .ToListAsync();

            var totalStudents = await _context.Enrollments
                .CountAsync(e => e.ClassId == slot.ClassId && e.IsActive);

            var statistics = new AttendanceStatistics
            {
                TotalStudents = totalStudents,
                PresentCount = attendanceRecords.Count(ar => ar.Status == AttendanceStatus.Present),
                LateCount = attendanceRecords.Count(ar => ar.Status == AttendanceStatus.Late),
                ExcusedCount = leaveRequests.Count(lr => lr.Status == LeaveRequestStatus.Approved),
                AbsentCount = totalStudents - attendanceRecords.Count - leaveRequests.Count(lr => lr.Status == LeaveRequestStatus.Approved),
                LeaveRequestCount = leaveRequests.Count,
                FlaggedCount = attendanceRecords.Count(ar => ar.IsFlagged),
                AttendanceRate = totalStudents > 0 ? (double)attendanceRecords.Count / totalStudents * 100 : 0
            };

            var model = new AttendanceSlotDetailViewModel
            {
                Slot = slot,
                Class = slot.Class,
                IsTeacher = isTeacher,
                HasCheckedIn = hasCheckedIn,
                HasRequestedLeave = hasRequestedLeave,
                MyAttendance = myAttendance,
                MyLeaveRequest = myLeaveRequest,
                AttendanceRecords = attendanceRecords,
                LeaveRequests = leaveRequests,
                Statistics = statistics
            };

            return View(model);
        }

        // GET: Attendance/CheckIn/5
        public async Task<IActionResult> CheckIn(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var slot = await _context.AttendanceSlots
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            // Check if student is enrolled
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassId == slot.ClassId && e.StudentId == user.Id && e.IsActive);

            if (!isEnrolled)
            {
                TempData["ErrorMessage"] = "Bạn chưa tham gia lớp học này!";
                return RedirectToAction(nameof(SlotDetail), new { id });
            }

            // Check if already checked in
            var hasCheckedIn = await _context.AttendanceRecords
                .AnyAsync(ar => ar.SlotId == id && ar.StudentId == user.Id);

            if (hasCheckedIn)
            {
                TempData["ErrorMessage"] = "Bạn đã điểm danh rồi!";
                return RedirectToAction(nameof(SlotDetail), new { id });
            }

            // Check if slot is active
            if (DateTime.Now < slot.StartTime)
            {
                TempData["ErrorMessage"] = "Phiên điểm danh chưa bắt đầu!";
                return RedirectToAction(nameof(SlotDetail), new { id });
            }

            if (DateTime.Now > slot.EndTime)
            {
                TempData["ErrorMessage"] = "Phiên điểm danh đã kết thúc!";
                return RedirectToAction(nameof(SlotDetail), new { id });
            }

            var model = new CheckInViewModel
            {
                SlotId = id,
                StudentName = user.FullName,
                StudentCode = user.StudentId ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            var testMode = _configuration.GetValue<bool>("AppSettings:TestMode", false);
            var allowedDistance = slot.AllowedDistanceMeters;
            
            // Nếu test mode, t?ng khoảng cách cho phép
            if (testMode)
            {
                allowedDistance = _configuration.GetValue<int>("AppSettings:TestModeDistanceMeters", 5000);
                ViewBag.TestMode = true;
            }

            ViewBag.SlotName = slot.SlotName;
            ViewBag.ClassName = slot.Class.ClassName;
            ViewBag.AllowedDistance = allowedDistance;
            ViewBag.OriginalAllowedDistance = slot.AllowedDistanceMeters;
            ViewBag.SlotLatitude = slot.SlotLatitude;
            ViewBag.SlotLongitude = slot.SlotLongitude;

            return View(model);
        }

        // POST: Attendance/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(CheckInViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var slot = await _context.AttendanceSlots
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync(s => s.SlotId == model.SlotId);

                if (slot == null) return NotFound();

                if (await _context.AttendanceRecords.AnyAsync(ar => ar.SlotId == model.SlotId && ar.StudentId == user.Id))
                    return Json(new { success = false, message = "Bạn đã điểm danh rồi!" });

                // Làm tròn tọa độ đến 6 chữ số thập phân để đảm bảo tính nhất quán
                double roundedLat = Math.Round(model.Latitude, 6);
                double roundedLng = Math.Round(model.Longitude, 6);

                double distance = 0;
                bool isOutOfRange = false;
                bool isDuplicateDevice = false;
                bool isInvalidLocation = false;
                string outOfRangeReason = null;

                bool hasValidSlotLocation = slot.SlotLatitude.HasValue && slot.SlotLongitude.HasValue 
                    && Math.Abs(slot.SlotLatitude.Value) <= 90 && Math.Abs(slot.SlotLongitude.Value) <= 180
                    && slot.SlotLatitude.Value != 0 && slot.SlotLongitude.Value != 0;

                if (!hasValidSlotLocation)
                {
                    isInvalidLocation = true;
                }
                else
                {
                    bool hasValidUserLocation = Math.Abs(roundedLat) <= 90 && Math.Abs(roundedLng) <= 180 
                        && !double.IsNaN(roundedLat) && !double.IsNaN(roundedLng)
                        && roundedLat != 0 && roundedLng != 0;

                    if (hasValidUserLocation)
                    {
                        distance = CalculateDistance(roundedLat, roundedLng, slot.SlotLatitude.Value, slot.SlotLongitude.Value);

                        var testMode = _configuration.GetValue<bool>("AppSettings:TestMode", false);
                        var effectiveAllowedDistance = testMode 
                            ? _configuration.GetValue<int>("AppSettings:TestModeDistanceMeters", 5000)
                            : slot.AllowedDistanceMeters;

                        if (distance > effectiveAllowedDistance)
                        {
                            isOutOfRange = true;
                            outOfRangeReason = $"Ngoài phạm vi cho phép: {distance:F2}m (cho phép: {effectiveAllowedDistance}m)";
                        }
                    }
                    else
                    {
                        isInvalidLocation = true;
                    }
                }

                var status = DateTime.Now > slot.StartTime.AddMinutes(15) 
                    ? AttendanceStatus.Late 
                    : AttendanceStatus.Present;

                var deviceInfo = Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var duplicateDeviceRecords = await _context.AttendanceRecords
                    .Where(ar => ar.SlotId == model.SlotId && ar.StudentId != user.Id)
                    .ToListAsync();

                isDuplicateDevice = duplicateDeviceRecords.Any(ar => 
                    (!string.IsNullOrEmpty(ar.DeviceInfo) && ar.DeviceInfo == deviceInfo) ||
                    (!string.IsNullOrEmpty(ar.IpAddress) && ar.IpAddress == ipAddress));

                bool isFlagged = (isOutOfRange || isDuplicateDevice) && !isInvalidLocation;
                string flagReason = null;

                if (isOutOfRange && isDuplicateDevice)
                    flagReason = $"{outOfRangeReason}; Thiết bị hoặc IP trùng";
                else if (isOutOfRange)
                    flagReason = outOfRangeReason;
                else if (isDuplicateDevice)
                    flagReason = "Thiết bị hoặc IP trùng với sinh viên khác";

                var record = new AttendanceRecord
                {
                    SlotId = model.SlotId,
                    StudentId = user.Id,
                    StudentName = model.StudentName,
                    StudentCode = model.StudentCode,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Latitude = roundedLat,
                    Longitude = roundedLng,
                    DistanceFromClass = distance,
                    CheckInTime = DateTimeHelper.GetVietnamNow(),
                    Status = status,
                    DeviceInfo = deviceInfo,
                    IpAddress = ipAddress,
                    UserAgent = deviceInfo,
                    IsFlagged = isFlagged,
                    FlagReason = flagReason
                };

                _context.AttendanceRecords.Add(record);
                await _context.SaveChangesAsync();

                if (isFlagged)
                {
                    if (isOutOfRange)
                        _context.AttendanceFlags.Add(new AttendanceFlag
                        {
                            RecordId = record.RecordId,
                            Type = FlagType.OutOfRange,
                            Reason = $"Khoảng cách: {distance:F2}m (cho phép: {slot.AllowedDistanceMeters}m)",
                            FlaggedAt = DateTimeHelper.GetVietnamNow()
                        });

                    if (isDuplicateDevice)
                        _context.AttendanceFlags.Add(new AttendanceFlag
                        {
                            RecordId = record.RecordId,
                            Type = FlagType.DuplicateDevice,
                            Reason = "Thiết bị hoặc IP trùng với sinh viên khác",
                            FlaggedAt = DateTimeHelper.GetVietnamNow()
                        });

                    await _context.SaveChangesAsync();
                }

                string message = isFlagged 
                    ? $"Điểm danh thành công! Lưu ý: {flagReason}" 
                    : "Điểm danh thành công!";

                return Json(new { success = true, message = message });
            }

            return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
        }

        // GET: Attendance/RequestLeave/5
        public async Task<IActionResult> RequestLeave(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var slot = await _context.AttendanceSlots
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            // Kiểm tra xem đã được yêu cầu chưa
            var hasRequested = await _context.LeaveRequests
                .AnyAsync(lr => lr.SlotId == id && lr.StudentId == user.Id);

            if (hasRequested)
            {
                TempData["ErrorMessage"] = "Bạn đã gửi đơn xin nghỉ rồi!";
                return RedirectToAction(nameof(SlotDetail), new { id });
            }

            var model = new RequestLeaveViewModel
            {
                SlotId = id,
                StudentName = user.FullName,
                StudentCode = user.StudentId ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            ViewBag.SlotName = slot.SlotName;
            ViewBag.ClassName = slot.Class.ClassName;

            return View(model);
        }

        // POST: Attendance/RequestLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLeave(RequestLeaveViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var hasRequested = await _context.LeaveRequests
                    .AnyAsync(lr => lr.SlotId == model.SlotId && lr.StudentId == user.Id);

                if (hasRequested)
                {
                    TempData["ErrorMessage"] = "Bạn đã gửi đơn xin nghỉ rồi!";
                    return RedirectToAction(nameof(SlotDetail), new { id = model.SlotId });
                }

                string? evidenceUrl = null;
                if (model.Evidence != null)
                {
                    evidenceUrl = await SaveFileAsync(model.Evidence, "evidence");
                }

                var leaveRequest = new LeaveRequest
                {
                    SlotId = model.SlotId,
                    StudentId = user.Id,
                    StudentName = model.StudentName,
                    StudentCode = model.StudentCode,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Reason = model.Reason,
                    EvidenceUrl = evidenceUrl,
                    RequestedAt = DateTimeHelper.GetVietnamNow(),
                    Status = LeaveRequestStatus.Pending
                };

                _context.LeaveRequests.Add(leaveRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Gửi đơn xin nghỉ thành công!";
                return RedirectToAction(nameof(SlotDetail), new { id = model.SlotId });
            }

            return View(model);
        }

        // POST: Attendance/ReviewLeaveRequest
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> ReviewLeaveRequest(int requestId, string status, string? note)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.AttendanceSlot)
                    .ThenInclude(s => s.Class)
                .FirstOrDefaultAsync(lr => lr.RequestId == requestId);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (leaveRequest.AttendanceSlot.Class.TeacherId != user?.Id)
            {
                return Forbid();
            }

            leaveRequest.Status = status == "approved" ? LeaveRequestStatus.Approved : LeaveRequestStatus.Rejected;
            leaveRequest.ReviewedAt = DateTimeHelper.GetVietnamNow();
            leaveRequest.ReviewNote = note;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xử lý đơn xin nghỉ!";
            return RedirectToAction(nameof(SlotDetail), new { id = leaveRequest.SlotId });
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // bán kính trái đất

            double phi1 = lat1 * Math.PI / 180;
            double phi2 = lat2 * Math.PI / 180;
            double deltaLat = (lat2 - lat1) * Math.PI / 180;
            double deltaLon = (lon2 - lon1) * Math.PI / 180;
            
            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            
            double c = 2 * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));
            return R * c;
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
