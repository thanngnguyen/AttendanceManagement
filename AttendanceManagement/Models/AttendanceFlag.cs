using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Models
{
    public class AttendanceFlag
    {
        [Key]
        public int FlagId { get; set; }

        [Required]
        public int RecordId { get; set; }

        [ForeignKey("RecordId")]
        public virtual AttendanceRecord AttendanceRecord { get; set; } = null!;

        public FlagType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public DateTime FlaggedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;

        [StringLength(500)]
        public string? Resolution { get; set; }
    }

    public enum FlagType
    {
        OutOfRange = 0,           // Ngoài ph?m vi cho phép
        DuplicateDevice = 1,      // Cùng thi?t b? ?i?m danh nhi?u tài kho?n
        SuspiciousLocation = 2,   // V? trí ?áng ng?
        DuplicateIpAddress = 3,   // Cùng IP ?i?m danh nhi?u tài kho?n
        Other = 99
    }
}
