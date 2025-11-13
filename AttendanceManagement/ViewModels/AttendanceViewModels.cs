using System.ComponentModel.DataAnnotations;
using AttendanceManagement.Models;

namespace AttendanceManagement.ViewModels
{
    public class CreateAttendanceSlotViewModel
    {
        [Required]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Tên phiên điểm danh là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên phiên điểm danh")]
        public string SlotName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        [Display(Name = "Thời gian kết thúc")]
        public DateTime EndTime { get; set; } = DateTime.Now.AddHours(2);

        [Required(ErrorMessage = "Vĩ độ (Latitude) là bắt buộc")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải trong khoảng -90 đến 90")]
        [Display(Name = "Vĩ độ")]
        public double SlotLatitude { get; set; }

        [Required(ErrorMessage = "Kinh độ (Longitude) là bắt buộc")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải trong khoảng -180 đến 180")]
        [Display(Name = "Kinh độ")]
        public double SlotLongitude { get; set; }

        [Required(ErrorMessage = "Khoảng cách cho phép là bắt buộc")]
        [Display(Name = "Khoảng cách cho phép (mét)")]
        [Range(10, 10000, ErrorMessage = "Khoảng cách phải từ 10 đến 10000 mét")]
        public int AllowedDistanceMeters { get; set; } = 100;
    }

    public class AttendanceSlotDetailViewModel
    {
        public AttendanceSlot Slot { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public bool IsTeacher { get; set; }
        public bool HasCheckedIn { get; set; }
        public bool HasRequestedLeave { get; set; }
        public AttendanceRecord? MyAttendance { get; set; }
        public LeaveRequest? MyLeaveRequest { get; set; }
        public List<AttendanceRecordViewModel> AttendanceRecords { get; set; } = new List<AttendanceRecordViewModel>();
        public List<LeaveRequestViewModel> LeaveRequests { get; set; } = new List<LeaveRequestViewModel>();
        public AttendanceStatistics Statistics { get; set; } = new AttendanceStatistics();
    }

    public class AttendanceRecordViewModel
    {
        public int RecordId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistanceFromClass { get; set; }
        public DateTime CheckInTime { get; set; }
        public AttendanceStatus Status { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
        public List<AttendanceFlag> Flags { get; set; } = new List<AttendanceFlag>();
    }

    public class LeaveRequestViewModel
    {
        public int RequestId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? EvidenceUrl { get; set; }
        public DateTime RequestedAt { get; set; }
        public LeaveRequestStatus Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
    }

    public class CheckInViewModel
    {
        [Required]
        public int SlotId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        [StringLength(20)]
        [Display(Name = "Mã sinh viên")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng bật định vị")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Vui lòng bật định vị")]
        public double Longitude { get; set; }
    }

    public class RequestLeaveViewModel
    {
        [Required]
        public int SlotId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        [StringLength(20)]
        [Display(Name = "Mã sinh viên")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lý do xin nghỉ là bắt buộc")]
        [StringLength(1000)]
        [Display(Name = "Lý do xin nghỉ")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Minh chứng")]
        public IFormFile? Evidence { get; set; }
    }

    public class AttendanceStatistics
    {
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public int AbsentCount { get; set; }
        public int LeaveRequestCount { get; set; }
        public int FlaggedCount { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class StudentAttendanceReportViewModel
    {
        public User Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public List<AttendanceSlotSummary> AttendanceHistory { get; set; } = new List<AttendanceSlotSummary>();
        public int TotalSlots { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public int AbsentCount { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class AttendanceSlotSummary
    {
        public AttendanceSlot Slot { get; set; } = null!;
        public AttendanceRecord? Record { get; set; }
        public LeaveRequest? LeaveRequest { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }
}
