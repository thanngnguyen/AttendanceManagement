using System.ComponentModel.DataAnnotations;
using AttendanceManagement.Models;

namespace AttendanceManagement.ViewModels
{
    public class CreateClassViewModel
    {
        [Required(ErrorMessage = "Tên lớp là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tên lớp")]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(50)]
        [Display(Name = "Môn học")]
        public string? Subject { get; set; }

        [StringLength(50)]
        [Display(Name = "Phòng học")]
        public string? Room { get; set; }

        [Display(Name = "Vĩ độ lớp học")]
        public double? ClassLatitude { get; set; }

        [Display(Name = "Kinh độ lớp học")]
        public double? ClassLongitude { get; set; }

        [Display(Name = "Khoảng cách cho phép (mét)")]
        [Range(10, 1000)]
        public int AllowedDistanceMeters { get; set; } = 100;
    }

    public class ClassDetailViewModel
    {
        public Class Class { get; set; } = null!;
        public bool IsTeacher { get; set; }
        public bool IsEnrolled { get; set; }
        public int TotalStudents { get; set; }
        public List<Post> RecentPosts { get; set; } = new List<Post>();
        public List<Assignment> UpcomingAssignments { get; set; } = new List<Assignment>();
        public List<AttendanceSlot> RecentSlots { get; set; } = new List<AttendanceSlot>();
    }

    public class JoinClassViewModel
    {
        [Required(ErrorMessage = "Mã lớp là bắt buộc")]
        [Display(Name = "Mã lớp")]
        public string ClassCode { get; set; } = string.Empty;
    }

    public class ClassMembersViewModel
    {
        public Class Class { get; set; } = null!;
        public User Teacher { get; set; } = null!;
        public List<StudentMemberViewModel> Students { get; set; } = new List<StudentMemberViewModel>();
    }

    public class StudentMemberViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public int AttendanceCount { get; set; }
        public int TotalSlots { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class CreatePostViewModel
    {
        [Required]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [StringLength(2000)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Đính kèm")]
        public IFormFile? Attachment { get; set; }

        [Display(Name = "Loại bài viết")]
        public PostType Type { get; set; } = PostType.Announcement;
    }

    public class PostDetailViewModel
    {
        public Post Post { get; set; } = null!;
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public bool CanEdit { get; set; }
    }
}
