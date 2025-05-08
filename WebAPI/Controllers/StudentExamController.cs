using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Repository;
using Shared.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentExamController : ControllerBase
    {
        private readonly IGenericRepository<StudentExam> _studentExamRepository;
        private readonly IGenericRepository<StudentAnswer> _studentAnswerRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Option> _optionRepository;
        private readonly IGenericRepository<Exam> _examRepository;
        public StudentExamController(IGenericRepository<StudentExam> studentExamRepository, IGenericRepository<StudentAnswer> studentAnswerRepository, IGenericRepository<Question> questionRepository, IGenericRepository<Option> optionRepository, IGenericRepository<Exam> examRepository)
        {
            _studentExamRepository = studentExamRepository;
            _studentAnswerRepository = studentAnswerRepository;
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _examRepository = examRepository;
        }

        [Authorize(Policy = "Student")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllStudentExams()
        {
            try
            {
                var studentExams = await _studentExamRepository.GetAllAsync();
                return Ok(studentExams);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Student")]
        [HttpPost("add")]
        public async Task<IActionResult> AddStudentExam([FromBody] StudentExam studentExam)
        {
            try
            {
                await _studentExamRepository.AddAsync(studentExam);
                return Ok(new { message = "Student exam added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Student")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateStudentExam([FromBody] UpdateStudentExamModel updateStudentExamModel)
        {
            try
            {
                updateStudentExamModel.studentExam.CompleteAt = DateTime.Now;
                updateStudentExamModel.studentExam.Score = await CalculateScore(updateStudentExamModel.studentAnswers);
                await _studentExamRepository.UpdateAsync(updateStudentExamModel.studentExam);
                return Ok(new { message = "Student exam updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task<int> CalculateScore(List<StudentAnswer> studentAnswers)
        {
            int score = 0;
            if (studentAnswers == null || studentAnswers.Count == 0)
            {
                throw new Exception("Failed to fetch answers");
            }
            try
            {
                var studentExams = await _studentExamRepository.GetAllAsync();
                var questions = await _questionRepository.GetAllAsync();
                var options = await _optionRepository.GetAllAsync();
                var exams = await _examRepository.GetAllAsync();
                var studentExam = studentExams.FirstOrDefault(se => se.Id == studentAnswers[0].StudentExamId);
                if (studentExam == null)
                {
                    throw new Exception("Student exam not found");
                }
                var exam = exams.FirstOrDefault(e => e.Id == studentExam.ExamId);
                if (exam == null)
                {
                    throw new Exception("Exam not found");
                }
                foreach (var studentAnswer in studentAnswers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == studentAnswer.QuestionId);
                    if (question == null)
                    {
                        throw new Exception("Question not found");
                    }
                    var optionInQuestion = options.Where(o => o.QuestionId == question.Id).ToList();
                    var optionChoiced = optionInQuestion.FirstOrDefault(o => o.Id == studentAnswer.OptionId);
                    if (optionChoiced == null)
                    {
                        throw new Exception("Answers not found");
                    }
                    if (optionChoiced.IsCorrect)
                    {
                        score+=10;
                    }
                    await _studentAnswerRepository.AddAsync(studentAnswer);
                }
                return score;
            }
            catch (Exception ex)
            {
                throw new Exception("Error:" + ex.Message);
            }
        }
    }

    public class UpdateStudentExamModel
    {
        public StudentExam studentExam { get; set; }
        public List<StudentAnswer> studentAnswers { get; set; }
    }
}
