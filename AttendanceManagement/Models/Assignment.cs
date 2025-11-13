using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Models
{
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public int MaxScore { get; set; } = 100;

        public bool AllowLateSubmission { get; set; } = true;

        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        public bool IsPublished { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
