using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Models
{
    public class AttendanceRecord
    {
        [Key]
        public int RecordId { get; set; }

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

        // V? trí ?i?m danh
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public double DistanceFromClass { get; set; } // Kho?ng cách t? v? trí l?p h?c (mét)

        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        // Thông tin thi?t b? ?? phát hi?n gian l?n
        [StringLength(500)]
        public string? DeviceInfo { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(200)]
        public string? UserAgent { get; set; }

        // Flag vi ph?m
        public bool IsFlagged { get; set; } = false;

        [StringLength(500)]
        public string? FlagReason { get; set; }

        public virtual ICollection<AttendanceFlag> Flags { get; set; } = new List<AttendanceFlag>();
    }

    public enum AttendanceStatus
    {
        Present = 0,      // Có m?t
        Late = 1,         // ?i mu?n
        Excused = 2,      // Ngh? có phép
        Absent = 3        // V?ng m?t
    }
}
