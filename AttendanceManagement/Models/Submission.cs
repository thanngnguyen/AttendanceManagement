using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Models
{
    public class Submission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; } = null!;

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; } = null!;

        [StringLength(2000)]
        public string? SubmissionText { get; set; }

        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? GradedAt { get; set; }

        public double? Score { get; set; }

        [StringLength(1000)]
        public string? Feedback { get; set; }

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    }

    public enum SubmissionStatus
    {
        Submitted = 0,
        Graded = 1,
        Returned = 2,
        Late = 3
    }
}
