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
            Console.WriteLine($"=== CREATING SLOT - RECEIVED DATA ===");
            Console.WriteLine($"ClassId: {model.ClassId}");
            Console.WriteLine($"SlotName: {model.SlotName}");
            Console.WriteLine($"SlotLatitude: {model.SlotLatitude}");
            Console.WriteLine($"SlotLongitude: {model.SlotLongitude}");
            Console.WriteLine($"AllowedDistanceMeters: {model.AllowedDistanceMeters}");
            
            if (ModelState.IsValid)
            {
                var classEntity = await _context.Classes.FindAsync(model.ClassId);
                if (classEntity == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                if (classEntity.TeacherId != user?.Id)
                {
                    return Forbid();
                }

                // Validate latitude/longitude range
                if (Math.Abs(model.SlotLatitude) > 90 || Math.Abs(model.SlotLongitude) > 180)
                {
                    ModelState.AddModelError("", $"Tọa độ không hợp lệ! Latitude phải trong khoảng [-90, 90], Longitude trong khoảng [-180, 180]. Bạn đã nhập: Lat={model.SlotLatitude}, Lng={model.SlotLongitude}");
                    ViewBag.ClassName = classEntity.ClassName;
                    return View(model);
                }

                Console.WriteLine($"=== CREATING SLOT IN DATABASE ===");
                Console.WriteLine($"Slot Latitude: {model.SlotLatitude}");
                Console.WriteLine($"Slot Longitude: {model.SlotLongitude}");
                Console.WriteLine($"Allowed Distance: {model.AllowedDistanceMeters}m");

                var slot = new AttendanceSlot
                {
                    ClassId = model.ClassId,
                    SlotName = model.SlotName,
                    Description = model.Description,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    SlotLatitude = model.SlotLatitude,
                    SlotLongitude = model.SlotLongitude,
                    AllowedDistanceMeters = model.AllowedDistanceMeters,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AttendanceSlots.Add(slot);
                await _context.SaveChangesAsync();

                Console.WriteLine($"? Slot created successfully:");
                Console.WriteLine($"   ID: {slot.SlotId}");
                Console.WriteLine($"   Latitude: {slot.SlotLatitude}");
                Console.WriteLine($"   Longitude: {slot.SlotLongitude}");
                Console.WriteLine($"   Distance: {slot.AllowedDistanceMeters}m");

                // Verify saved data
                var savedSlot = await _context.AttendanceSlots.FindAsync(slot.SlotId);
                if (savedSlot != null)
                {
                    Console.WriteLine($"=== VERIFICATION FROM DATABASE ===");
                    Console.WriteLine($"   Saved Latitude: {savedSlot.SlotLatitude}");
                    Console.WriteLine($"   Saved Longitude: {savedSlot.SlotLongitude}");
                }

                TempData["SuccessMessage"] = $"Tạo phiên điểm danh thành công! (Lat={model.SlotLatitude:F6}, Lng={model.SlotLongitude:F6})";
                return RedirectToAction("Detail", "Class", new { id = model.ClassId });
            }

            // Log validation errors
            Console.WriteLine("=== MODEL STATE ERRORS ===");
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
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
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassId == slot.ClassId && e.StudentId == user.Id && e.IsActive);

            if (!isTeacher && !isEnrolled)
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

                if (slot == null)
                {
                    return NotFound();
                }

                // Check if already checked in
                var hasCheckedIn = await _context.AttendanceRecords
                    .AnyAsync(ar => ar.SlotId == model.SlotId && ar.StudentId == user.Id);

                if (hasCheckedIn)
                {
                    return Json(new { success = false, message = "Bạn đã điểm danh rồi!" });
                }

                // Calculate distance from class location
                double distance = 0;
                bool isFlagged = false;
                string? flagReason = null;

                // Log thông tin ?? debug
                Console.WriteLine($"=== CHECK-IN DEBUG INFO ===");
                Console.WriteLine($"Slot ID: {slot.SlotId}");
                Console.WriteLine($"Slot Location: Lat={slot.SlotLatitude}, Lng={slot.SlotLongitude}");
                Console.WriteLine($"User Location: Lat={model.Latitude}, Lng={model.Longitude}");
                Console.WriteLine($"Allowed Distance: {slot.AllowedDistanceMeters}m");

                // Kiểm tra slot có tọa độ hợp lệ không
                bool hasValidSlotLocation = slot.SlotLatitude.HasValue && slot.SlotLongitude.HasValue 
                    && slot.SlotLatitude.Value != 0 && slot.SlotLongitude.Value != 0
                    && Math.Abs(slot.SlotLatitude.Value) <= 90 && Math.Abs(slot.SlotLongitude.Value) <= 180;

                if (hasValidSlotLocation)
                {
                    // Kiểm tra user có tọa độ hợp lệ không
                    bool hasValidUserLocation = model.Latitude != 0 && model.Longitude != 0
                        && Math.Abs(model.Latitude) <= 90 && Math.Abs(model.Longitude) <= 180;

                    if (hasValidUserLocation)
                    {
                        distance = CalculateDistance(
                            model.Latitude, model.Longitude,
                            slot.SlotLatitude.Value, slot.SlotLongitude.Value
                        );

                        Console.WriteLine($"Calculated Distance: {distance:F2}m");

                        // Kiểm tra TestMode
                        var testMode = _configuration.GetValue<bool>("AppSettings:TestMode", false);
                        var effectiveAllowedDistance = slot.AllowedDistanceMeters;
                        
                        if (testMode)
                        {
                            effectiveAllowedDistance = _configuration.GetValue<int>("AppSettings:TestModeDistanceMeters", 5000);
                            Console.WriteLine($"TEST MODE: Allowed distance increased to {effectiveAllowedDistance}m");
                        }

                        if (distance > effectiveAllowedDistance)
                        {
                            isFlagged = true;
                            flagReason = $"Ngoài phạm vi cho phép: {distance:F2}m (cho phép: {effectiveAllowedDistance}m)";
                            Console.WriteLine($"? FLAGGED: {flagReason}");
                        }
                        else
                        {
                            Console.WriteLine($"? OK: Trong phạm vi ({distance:F2}m <= {effectiveAllowedDistance}m)");
                        }
                    }
                    else
                    {
                        // User location invalid
                        isFlagged = true;
                        flagReason = "Không lấy đượcc vị trí hợp lệ từ thiết bị";
                        Console.WriteLine($"? WARNING: User location invalid - Lat={model.Latitude}, Lng={model.Longitude}");
                    }
                }
                else
                {
                    // Slot không có tọa độ - cho phép điểm danh không cần check vị trí
                    Console.WriteLine("? WARNING: Slot không có tọa độ GPS - bỏ qua kiểm tra khoảng cách");
                    distance = 0; // Set distance = 0 nếu không check
                }

                // Determine attendance status
                var status = AttendanceStatus.Present;
                var lateThresholdMinutes = 15;
                
                if (DateTime.Now > slot.StartTime.AddMinutes(lateThresholdMinutes))
                {
                    status = AttendanceStatus.Late;
                    Console.WriteLine($"Status: Late (checked in {(DateTime.Now - slot.StartTime).TotalMinutes:F0} minutes after start)");
                }
                else
                {
                    Console.WriteLine($"Status: Present");
                }

                // Get device info
                var deviceInfo = Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                Console.WriteLine($"Device: {deviceInfo?.Substring(0, Math.Min(50, deviceInfo?.Length ?? 0))}...");
                Console.WriteLine($"IP: {ipAddress}");

                // Check for duplicate device/IP
                var duplicateDevice = await _context.AttendanceRecords
                    .AnyAsync(ar => ar.SlotId == model.SlotId &&
                                   ar.StudentId != user.Id &&
                                   (ar.DeviceInfo == deviceInfo || ar.IpAddress == ipAddress));

                if (duplicateDevice)
                {
                    isFlagged = true;
                    if (string.IsNullOrEmpty(flagReason))
                        flagReason = "Thiết bị hoặc IP trùng với sinh viên khác";
                    else
                        flagReason += "; Thiết bị hoặc IP trùng với sinh viên khác";
                    
                    Console.WriteLine($"? FLAGGED: Duplicate device/IP detected");
                }

                var record = new AttendanceRecord
                {
                    SlotId = model.SlotId,
                    StudentId = user.Id,
                    StudentName = model.StudentName,
                    StudentCode = model.StudentCode,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    DistanceFromClass = distance,
                    CheckInTime = DateTime.UtcNow,
                    Status = status,
                    DeviceInfo = deviceInfo,
                    IpAddress = ipAddress,
                    UserAgent = deviceInfo,
                    IsFlagged = isFlagged,
                    FlagReason = flagReason
                };

                _context.AttendanceRecords.Add(record);
                await _context.SaveChangesAsync();

                Console.WriteLine($"? Attendance Record Created: ID={record.RecordId}, Distance={distance:F2}m, IsFlagged={isFlagged}");

                // Add flags if needed
                if (isFlagged)
                {
                    if (hasValidSlotLocation && distance > slot.AllowedDistanceMeters)
                    {
                        _context.AttendanceFlags.Add(new AttendanceFlag
                        {
                            RecordId = record.RecordId,
                            Type = FlagType.OutOfRange,
                            Reason = $"Khoảng cách: {distance:F2}m (cho phép: {slot.AllowedDistanceMeters}m)",
                            FlaggedAt = DateTime.UtcNow
                        });
                    }

                    if (duplicateDevice)
                    {
                        _context.AttendanceFlags.Add(new AttendanceFlag
                        {
                            RecordId = record.RecordId,
                            Type = FlagType.DuplicateDevice,
                            Reason = "Thiết bị hoặc IP trùng với sinh viên khác",
                            FlaggedAt = DateTime.UtcNow
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                var message = isFlagged 
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

            // Check if already requested
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

                // Check if already requested
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
                    RequestedAt = DateTime.UtcNow,
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
            leaveRequest.ReviewedAt = DateTime.UtcNow;
            leaveRequest.ReviewNote = note;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xử lý đơn xin nghỉ!";
            return RedirectToAction(nameof(SlotDetail), new { id = leaveRequest.SlotId });
        }

        // Helper Methods
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth's radius in meters
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
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
