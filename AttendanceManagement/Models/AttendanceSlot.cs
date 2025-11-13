using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Models
{
    public class AttendanceSlot
    {
        [Key]
        public int SlotId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string SlotName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // V? trí c?a phiên ?i?m danh (có th? khác v?i v? trí l?p h?c)
        public double? SlotLatitude { get; set; }
        public double? SlotLongitude { get; set; }
        public int AllowedDistanceMeters { get; set; } = 100;

        // Navigation Properties
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    }
}
