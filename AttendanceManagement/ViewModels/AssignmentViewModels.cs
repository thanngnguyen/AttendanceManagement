using System.ComponentModel.DataAnnotations;
using AttendanceManagement.Models;

namespace AttendanceManagement.ViewModels
{
    public class CreateAssignmentViewModel
    {
        [Required]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề bài tập")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hạn nộp")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Điểm tối đa")]
        [Range(1, 1000)]
        public int MaxScore { get; set; } = 100;

        [Display(Name = "Cho phép nộp muộn")]
        public bool AllowLateSubmission { get; set; } = true;

        [Display(Name = "Tệp đính kèm")]
        public IFormFile? Attachment { get; set; }
    }

    public class AssignmentDetailViewModel
    {
        public Assignment Assignment { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public bool IsTeacher { get; set; }
        public bool HasSubmitted { get; set; }
        public Submission? MySubmission { get; set; }
        public List<SubmissionViewModel> Submissions { get; set; } = new List<SubmissionViewModel>();
        public AssignmentStatistics Statistics { get; set; } = new AssignmentStatistics();
    }

    public class SubmissionViewModel
    {
        public int SubmissionId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string? SubmissionText { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? GradedAt { get; set; }
        public double? Score { get; set; }
        public string? Feedback { get; set; }
        public SubmissionStatus Status { get; set; }
        public bool IsLate { get; set; }
    }

    public class CreateSubmissionViewModel
    {
        [Required]
        public int AssignmentId { get; set; }

        [StringLength(2000)]
        [Display(Name = "Nội dung bài làm")]
        public string? SubmissionText { get; set; }

        [Display(Name = "Tệp đính kèm")]
        public IFormFile? Attachment { get; set; }
    }

    public class GradeSubmissionViewModel
    {
        [Required]
        public int SubmissionId { get; set; }

        [Required(ErrorMessage = "Điểm là bắt buộc")]
        [Range(0, 1000)]
        [Display(Name = "Điểm")]
        public double Score { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nhận xét")]
        public string? Feedback { get; set; }
    }

    public class AssignmentStatistics
    {
        public int TotalStudents { get; set; }
        public int SubmittedCount { get; set; }
        public int GradedCount { get; set; }
        public int LateSubmissionCount { get; set; }
        public double AverageScore { get; set; }
        public double SubmissionRate { get; set; }
    }
}
