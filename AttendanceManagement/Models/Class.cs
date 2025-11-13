using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(20)]
        public string ClassCode { get; set; } = string.Empty; // Mã l?p ?? sinh viên tham gia

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Subject { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        [StringLength(500)]
        public string? BannerImageUrl { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;

        [ForeignKey("TeacherId")]
        public virtual User Teacher { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // V? trí l?p h?c (?? ki?m tra ?i?m danh)
        public double? ClassLatitude { get; set; }
        public double? ClassLongitude { get; set; }
        public int AllowedDistanceMeters { get; set; } = 100; // Kho?ng cách cho phép (m?c ??nh 100m)

        // Navigation Properties
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<AttendanceSlot> AttendanceSlots { get; set; } = new List<AttendanceSlot>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public virtual ICollection<ClassMaterial> Materials { get; set; } = new List<ClassMaterial>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();
    }
}
