using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;
using AttendanceManagement.ViewModels;

namespace AttendanceManagement.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AssignmentController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Assignment/Create/5
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create(int classId)
        {
            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (classEntity.TeacherId != user?.Id)
            {
                return Forbid();
            }

            var model = new CreateAssignmentViewModel { ClassId = classId };
            ViewBag.ClassName = classEntity.ClassName;
            return View(model);
        }

        // POST: Assignment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create(CreateAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var classEntity = await _context.Classes.FindAsync(model.ClassId);
                if (classEntity == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                if (classEntity.TeacherId != user?.Id)
                {
                    return Forbid();
                }

                string? attachmentUrl = null;
                if (model.Attachment != null)
                {
                    attachmentUrl = await SaveFileAsync(model.Attachment, "assignments");
                }

                var assignment = new Assignment
                {
                    ClassId = model.ClassId,
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    MaxScore = model.MaxScore,
                    AllowLateSubmission = model.AllowLateSubmission,
                    AttachmentUrl = attachmentUrl,
                    CreatedAt = DateTime.UtcNow,
                    IsPublished = true
                };

                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo bài tập thành công!";
                return RedirectToAction("Detail", "Class", new { id = model.ClassId });
            }

            return View(model);
        }

        // GET: Assignment/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null)
            {
                return NotFound();
            }

            var isTeacher = assignment.Class.TeacherId == user.Id;
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassId == assignment.ClassId && e.StudentId == user.Id && e.IsActive);

            if (!isTeacher && !isEnrolled)
            {
                return Forbid();
            }

            var hasSubmitted = await _context.Submissions
                .AnyAsync(s => s.AssignmentId == id && s.StudentId == user.Id);

            var mySubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == user.Id);

            var submissions = new List<SubmissionViewModel>();

            if (isTeacher)
            {
                submissions = await _context.Submissions
                    .Where(s => s.AssignmentId == id)
                    .Include(s => s.Student)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Select(s => new SubmissionViewModel
                    {
                        SubmissionId = s.SubmissionId,
                        StudentId = s.StudentId,
                        StudentName = s.Student.FullName,
                        StudentCode = s.Student.StudentId ?? string.Empty,
                        SubmissionText = s.SubmissionText,
                        AttachmentUrl = s.AttachmentUrl,
                        SubmittedAt = s.SubmittedAt,
                        GradedAt = s.GradedAt,
                        Score = s.Score,
                        Feedback = s.Feedback,
                        Status = s.Status,
                        IsLate = assignment.DueDate.HasValue && s.SubmittedAt > assignment.DueDate.Value
                    })
                    .ToListAsync();
            }

            var totalStudents = await _context.Enrollments
                .CountAsync(e => e.ClassId == assignment.ClassId && e.IsActive);

            var submittedCount = await _context.Submissions
                .CountAsync(s => s.AssignmentId == id);

            var gradedCount = await _context.Submissions
                .CountAsync(s => s.AssignmentId == id && s.Score.HasValue);

            var lateCount = assignment.DueDate.HasValue
                ? await _context.Submissions
                    .CountAsync(s => s.AssignmentId == id && s.SubmittedAt > assignment.DueDate.Value)
                : 0;

            var avgScore = await _context.Submissions
                .Where(s => s.AssignmentId == id && s.Score.HasValue)
                .AverageAsync(s => (double?)s.Score) ?? 0;

            var statistics = new AssignmentStatistics
            {
                TotalStudents = totalStudents,
                SubmittedCount = submittedCount,
                GradedCount = gradedCount,
                LateSubmissionCount = lateCount,
                AverageScore = avgScore,
                SubmissionRate = totalStudents > 0 ? (double)submittedCount / totalStudents * 100 : 0
            };

            var model = new AssignmentDetailViewModel
            {
                Assignment = assignment,
                Class = assignment.Class,
                IsTeacher = isTeacher,
                HasSubmitted = hasSubmitted,
                MySubmission = mySubmission,
                Submissions = submissions,
                Statistics = statistics
            };

            return View(model);
        }

        // GET: Assignment/Submit/5
        public async Task<IActionResult> Submit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null)
            {
                return NotFound();
            }

            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassId == assignment.ClassId && e.StudentId == user.Id && e.IsActive);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var hasSubmitted = await _context.Submissions
                .AnyAsync(s => s.AssignmentId == id && s.StudentId == user.Id);

            if (hasSubmitted)
            {
                TempData["ErrorMessage"] = "Bạn đã nộp bài tập này rồi!";
                return RedirectToAction(nameof(Detail), new { id });
            }

            if (assignment.DueDate.HasValue && DateTime.Now > assignment.DueDate.Value && !assignment.AllowLateSubmission)
            {
                TempData["ErrorMessage"] = "đã hết hạn nộp bài!";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var model = new CreateSubmissionViewModel { AssignmentId = id };
            ViewBag.AssignmentTitle = assignment.Title;
            ViewBag.ClassName = assignment.Class.ClassName;
            ViewBag.DueDate = assignment.DueDate;

            return View(model);
        }

        // POST: Assignment/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(CreateSubmissionViewModel model)
        {
            if (ModelState.IsValid || !string.IsNullOrEmpty(model.SubmissionText) || model.Attachment != null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var assignment = await _context.Assignments.FindAsync(model.AssignmentId);
                if (assignment == null)
                {
                    return NotFound();
                }

                var hasSubmitted = await _context.Submissions
                    .AnyAsync(s => s.AssignmentId == model.AssignmentId && s.StudentId == user.Id);

                if (hasSubmitted)
                {
                    TempData["ErrorMessage"] = "Bạn đã nộp bài tập này rồi!";
                    return RedirectToAction(nameof(Detail), new { id = model.AssignmentId });
                }

                string? attachmentUrl = null;
                if (model.Attachment != null)
                {
                    attachmentUrl = await SaveFileAsync(model.Attachment, "submissions");
                }

                var status = SubmissionStatus.Submitted;
                if (assignment.DueDate.HasValue && DateTime.Now > assignment.DueDate.Value)
                {
                    status = SubmissionStatus.Late;
                }

                var submission = new Submission
                {
                    AssignmentId = model.AssignmentId,
                    StudentId = user.Id,
                    SubmissionText = model.SubmissionText,
                    AttachmentUrl = attachmentUrl,
                    SubmittedAt = DateTime.UtcNow,
                    Status = status
                };

                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Nộp bài thành công!";
                return RedirectToAction(nameof(Detail), new { id = model.AssignmentId });
            }

            TempData["ErrorMessage"] = "Vui lòng nhập nội dung hoăc đính kèm file!";
            return RedirectToAction(nameof(Submit), new { id = model.AssignmentId });
        }

        // POST: Assignment/Grade
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Grade(GradeSubmissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var submission = await _context.Submissions
                    .Include(s => s.Assignment)
                        .ThenInclude(a => a.Class)
                    .FirstOrDefaultAsync(s => s.SubmissionId == model.SubmissionId);

                if (submission == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                if (submission.Assignment.Class.TeacherId != user?.Id)
                {
                    return Forbid();
                }

                if (model.Score > submission.Assignment.MaxScore)
                {
                    TempData["ErrorMessage"] = $"Điểm không được vượt quá {submission.Assignment.MaxScore}!";
                    return RedirectToAction(nameof(Detail), new { id = submission.AssignmentId });
                }

                submission.Score = model.Score;
                submission.Feedback = model.Feedback;
                submission.GradedAt = DateTime.UtcNow;
                submission.Status = SubmissionStatus.Graded;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Chấm điểm thành công!";
                return RedirectToAction(nameof(Detail), new { id = submission.AssignmentId });
            }

            return BadRequest();
        }

        // Helper Methods
        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }
    }
}
