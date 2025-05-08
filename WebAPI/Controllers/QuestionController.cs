using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Repository;
using Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Shared.ModelView;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Option> _optionRepository;
        public QuestionController(IGenericRepository<Question> questionRepository, IGenericRepository<Option> optionRepository)
        {
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
        }

        [Authorize(Policy = "Teacher")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllQuestions()
        {
            try
            {
                var questions = await _questionRepository.GetAllAsync();
                return Ok(questions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetQuestionById([FromBody] Guid id)
        {
            try
            {
                var questions = await _questionRepository.GetAllAsync();
                var question = questions.FirstOrDefault(q => q.Id == id);
                if (question == null)
                {
                    return NotFound(new { message = "Question not found" });
                }
                var options = await _optionRepository.GetAllAsync();
                QuestionOptionView questionOptipnView = new QuestionOptionView
                {
                    QuestionId = question.Id,
                    Content = question.Content,
                    Grade = question.Grade,
                    Unit = question.Unit,
                    ProbabilityOrStatistic = question.ProbabilityOrStatistic,
                    CreateBy = question.CreateBy,
                    Options = options.Where(o => o.QuestionId == question.Id).ToList()
                };
                return Ok(questionOptipnView);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpPost("add")]
        public async Task<IActionResult> AddQuestion([FromBody] QuestionOptionView questionOptipnView)
        {
            try
            {
                var question = new Question
                {
                    Content = questionOptipnView.Content,
                    Grade = questionOptipnView.Grade,
                    Unit = questionOptipnView.Unit,
                    ProbabilityOrStatistic = questionOptipnView.ProbabilityOrStatistic,
                    CreateBy = questionOptipnView.CreateBy
                };
                await _questionRepository.AddAsync(question);
                foreach (var option in questionOptipnView.Options)
                {
                    await _optionRepository.AddAsync(option);
                }
                return Ok(new { message = "Question added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuestion([FromBody] QuestionOptionView questionOptipnView)
        {
            try
            {
                var questions = await _questionRepository.GetAllAsync();
                var question = questions.FirstOrDefault(q => q.Id == questionOptipnView.QuestionId);
                if ( question == null)
                {
                    return NotFound(new { message = "Question not found" });
                }
                question.Content = questionOptipnView.Content;
                question.Grade = questionOptipnView.Grade;
                question.Unit = questionOptipnView.Unit;
                question.ProbabilityOrStatistic = questionOptipnView.ProbabilityOrStatistic;             
                await _questionRepository.UpdateAsync(question);
                foreach (var option in questionOptipnView.Options)
                {
                    await _optionRepository.UpdateAsync(option);
                }
                return Ok(new { message = "Question updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteQuestion([FromBody] QuestionOptionView questionOptipnView)
        {
            try
            {
                var questions = await _questionRepository.GetAllAsync();
                var options = await _optionRepository.GetAllAsync();
                var question = questions.FirstOrDefault(q => q.Id == questionOptipnView.QuestionId);
                if (question == null)
                {
                    return NotFound(new { message = "Question not found" });
                }
                await _questionRepository.DeleteAsync(question);
                var questionOptions = options.Where(o => o.QuestionId == question.Id).ToList();
                if (questionOptions.Count == 0)
                {
                    return NotFound(new { message = "Options not found" });
                }
                foreach (var option in questionOptions)
                {
                    await _optionRepository.UpdateAsync(option);
                }
                return Ok(new { message = "Question deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
