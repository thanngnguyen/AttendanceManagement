using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Models
{
    public class LeaveRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int SlotId { get; set; }

        [ForeignKey("SlotId")]
        public virtual AttendanceSlot AttendanceSlot { get; set; } = null!;

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string StudentName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string StudentCode { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? EvidenceUrl { get; set; } // URL file minh ch?ng

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? ReviewNote { get; set; }
    }

    public enum LeaveRequestStatus
    {
        Pending = 0,    // Ch? duy?t
        Approved = 1,   // ?ã duy?t
        Rejected = 2    // T? ch?i
    }
}
