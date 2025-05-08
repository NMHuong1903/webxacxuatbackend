using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using WebAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Shared.ModelView;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Option> _optionRepository;
        public ExamController(IGenericRepository<Exam> examRepository, IGenericRepository<Question> questionRepository, IGenericRepository<Option> optionRepository)
        {
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
        }

        [Authorize(Policy = "Teacher")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllExams()
        {
            try
            {
                var exams = await _examRepository.GetAllAsync();
                return Ok(exams);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetExamById([FromBody] Guid id)
        {
            try
            {
                var exams = await _examRepository.GetAllAsync();
                var exam = exams.FirstOrDefault(e => e.Id == id);
                if (exam == null)
                {
                    return NotFound(new { message = "Exam not found" });
                }
                ExamView examView = new ExamView
                {
                    ExamId = exam.Id,
                    Name = exam.Name,
                    Duration = exam.Duration,
                    TotalQuestions = exam.TotalQuestions,
                    CreateBy = exam.CreateBy,
                    CreatedAt = exam.CreatedAt,
                    Questions = new List<QuestionOptionView>()
                };
                foreach (var question in exam.Questions)
                {
                    var options = await _optionRepository.GetAllAsync();
                    var questionView = new QuestionOptionView
                    {
                        QuestionId = question.Id,
                        Content = question.Content,
                        Grade = question.Grade,
                        Unit = question.Unit,
                        ProbabilityOrStatistic = question.ProbabilityOrStatistic,
                        CreateBy = question.CreateBy,
                        Options = options.Where(o => o.QuestionId == question.Id).ToList()
                    };
                    examView.Questions.Add(questionView);
                }
                return Ok(examView);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpPost("add")]
        public async Task<IActionResult> AddExam([FromBody] ExamView examView)
        {
            try
            {
                var exam = new Exam
                {
                    Name = examView.Name,
                    Duration = examView.Duration,
                    TotalQuestions = examView.TotalQuestions,
                    CreateBy = examView.CreateBy,
                    Questions = new List<Question>()
                };
                foreach (var questionView in examView.Questions)
                {
                    var question = new Question
                    {
                        Content = questionView.Content,
                        Grade = questionView.Grade,
                        Unit = questionView.Unit,
                        ProbabilityOrStatistic = questionView.ProbabilityOrStatistic,
                        CreateBy = exam.CreateBy,
                        CreatedAt = exam.CreatedAt,
                    };
                    exam.Questions.Add(question);
                }
                await _examRepository.AddAsync(exam);
                return Ok(new { message = "Exam added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateExam([FromBody] ExamView examView)
        {
            try
            {
                var exams = await _examRepository.GetAllAsync();
                var exam = exams.FirstOrDefault(e => e.Id == examView.ExamId);
                if (exam == null)
                {
                    return NotFound(new { message = "Exam not found" });
                }
                exam.Name = examView.Name;
                exam.Duration = examView.Duration;
                exam.TotalQuestions = examView.TotalQuestions;
                exam.CreateBy = examView.CreateBy;
                exam.CreatedAt = examView.CreatedAt;
                await _examRepository.UpdateAsync(exam);
                return Ok(new { message = "Exam updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteExam([FromBody] ExamView examView)
        {
            try
            {
                var exams = await _examRepository.GetAllAsync();
                var exam = exams.FirstOrDefault(e => e.Id == examView.ExamId);
                if (exam == null)
                {
                    return NotFound(new { message = "Exam not found" });
                }
                await _examRepository.DeleteAsync(exam);
                return Ok(new { message = "Exam deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
