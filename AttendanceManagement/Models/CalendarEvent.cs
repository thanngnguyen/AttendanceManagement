using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Models
{
    public class CalendarEvent
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public EventType Type { get; set; }

        [StringLength(50)]
        public string? Location { get; set; }
    }

    public enum EventType
    {
        Class = 0,
        Exam = 1,
        Assignment = 2,
        Meeting = 3,
        Other = 99
    }
}
