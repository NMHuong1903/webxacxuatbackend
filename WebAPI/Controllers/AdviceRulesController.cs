using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using WebAPI.Repository;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdviceRulesController : ControllerBase
    {
        private readonly IGenericRepository<AdviceRules> _adviceRuleRepository;
        private readonly IGenericRepository<StudentExam> _studentExamRepository;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<User> _userRepository;
        public AdviceRulesController(IGenericRepository<AdviceRules> adviceRuleRepository,
            IGenericRepository<StudentExam> studentExamRepository,
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Question> questionRepository,
            IGenericRepository<User> userRepository)
        {
            _adviceRuleRepository = adviceRuleRepository;
            _studentExamRepository = studentExamRepository;
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _userRepository = userRepository;
        }

        [Authorize(Policy = "Student")]
        [HttpGet("get-dashboard")]
        public async Task<IActionResult> GetDashboard([FromBody] Guid userId)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                if (users == null || !users.Any())
                    return NotFound(new { message = "No users found" });
                var user = users.FirstOrDefault(x => x.Id == userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });
                else
                {
                    if (user.Role == "Admin")
                    {
                        List<ChartData> chartData = new List<ChartData>();
                        ChartData teacherData = new ChartData
                        {
                            Label = "Teachers",
                            Value = users.Count(x => x.Role == "Teacher")
                        };
                        chartData.Add(teacherData);
                        ChartData studentData = new ChartData
                        {
                            Label = "Students",
                            Value = users.Count(x => x.Role == "Student")
                        };
                        chartData.Add(studentData);
                        ChartData adminData = new ChartData
                        {
                            Label = "Admin",
                            Value = users.Count(x => x.Role == "Admin")
                        };
                        return Ok(chartData);
                    }
                    if (user.Role == "Teacher")
                    {
                        var exams = await _examRepository.GetAllAsync();
                        List<ChartData> chartDataExam = new List<ChartData>();
                        ChartData examDataOwner = new ChartData
                        {
                            Label = "My exam",
                            Value = exams.Count(x => x.CreateBy == userId)
                        };
                        chartDataExam.Add(examDataOwner);
                        ChartData examData = new ChartData
                        {
                            Label = "Other exam",
                            Value = exams.Count()
                        };
                        chartDataExam.Add(examData);
                        List<ChartData> chartDataQuestion = new List<ChartData>();
                        var questions = await _questionRepository.GetAllAsync();
                        ChartData questionDataOwner = new ChartData
                        {
                            Label = "My question",
                            Value = questions.Count(x => x.CreateBy == userId)
                        };
                        chartDataQuestion.Add(questionDataOwner);
                        ChartData questionData = new ChartData
                        {
                            Label = "Other question",
                            Value = questions.Count()
                        };
                        chartDataQuestion.Add(questionData);
                        return Ok(new { chartDataExam, chartDataQuestion });
                    }
                    if (user.Role == "Student")
                    {
                        var studentExams = await _studentExamRepository.GetAllAsync();
                        var exams = await _examRepository.GetAllAsync();
                        List<ChartData> chartDataScore = new List<ChartData>();
                        var listStudentExam = studentExams.Where(x => x.StudentId == userId).ToList();
                        foreach (var studentExam in listStudentExam)
                        {
                            var exam = exams.FirstOrDefault(x => x.Id == studentExam.ExamId);
                            if (exam != null)
                            {
                                ChartData chartData = new ChartData
                                {
                                    Label = exam.Name,
                                    Value = studentExam.Score
                                };
                                chartDataScore.Add(chartData);
                            }
                        }
                        return Ok(chartDataScore);
                    }
                }
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
