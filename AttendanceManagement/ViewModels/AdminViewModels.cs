using System.ComponentModel.DataAnnotations;
using AttendanceManagement.Models;

namespace AttendanceManagement.ViewModels
{
    // ===== ADMIN DASHBOARD =====
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalAttendanceRecords { get; set; }
        public int TotalFlaggedRecords { get; set; }
        public int TotalLeaveRequests { get; set; }
        public double AverageAttendanceRate { get; set; }

        public List<UserListItem> RecentUsers { get; set; } = new();
        public List<ClassStatistics> TopClasses { get; set; } = new();
        public List<AttendanceFlagSummary> RecentFlags { get; set; } = new();
    }

    // ===== USER MANAGEMENT =====
    public class UserListItem
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public int EnrolledClassesCount { get; set; }
        public int AttendanceRecordsCount { get; set; }
    }

    public class UserDetailViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? Bio { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? ProfileImageUrl { get; set; }

        // For Teachers
        public List<ClassInfo> TeachingClasses { get; set; } = new();

        // For Students
        public List<EnrollmentInfo> EnrolledClasses { get; set; } = new();
        public List<AttendanceInfo> AttendanceHistory { get; set; } = new();
        public List<LeaveRequestInfo> LeaveRequests { get; set; } = new();
    }

    public class EditUserViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        public string? StudentId { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public UserRole Role { get; set; }
    }

    // ===== CLASS MANAGEMENT =====
    public class ClassStatistics
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int EnrolledStudents { get; set; }
        public int TotalSlots { get; set; }
        public int AttendanceRecords { get; set; }
        public double AverageAttendanceRate { get; set; }
        public int FlaggedRecords { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ClassManagementViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int SlotCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ===== ATTENDANCE MANAGEMENT =====
    public class AttendanceInfo
    {
        public int RecordId { get; set; }
        public int SlotId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
        public AttendanceStatus Status { get; set; }
        public double DistanceFromClass { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
    }

    public class AttendanceFlagSummary
    {
        public int FlagId { get; set; }
        public int RecordId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public FlagType Type { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime FlaggedAt { get; set; }
        public bool IsResolved { get; set; }
    }

    public class AttendanceStatisticsViewModel
    {
        public int TotalRecords { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int FlaggedCount { get; set; }
        public double AverageAttendanceRate { get; set; }
        public List<AttendanceFlagSummary> FlaggedRecords { get; set; } = new();
    }

    // ===== LEAVE REQUEST MANAGEMENT =====
    public class LeaveRequestInfo
    {
        public int RequestId { get; set; }
        public int SlotId { get; set; }
        public string SlotName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public LeaveRequestStatus Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
    }

    public class LeaveRequestManagementViewModel
    {
        public int RequestId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? EvidenceUrl { get; set; }
        public DateTime RequestedAt { get; set; }
        public LeaveRequestStatus Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
        public int SlotId { get; set; }
    }

    // ===== ENROLLMENT MANAGEMENT =====
    public class EnrollmentInfo
    {
        public int EnrollmentId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public bool IsActive { get; set; }
    }

    // ===== CLASS INFO =====
    public class ClassInfo
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int SlotCount { get; set; }
        public bool IsActive { get; set; }
    }
}
