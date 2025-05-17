using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Repository;
using Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Shared.ModelView;
using Shared.SearchModel;
using Shared.ResponseModel;

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
        [HttpPost("get-page-data-with-filter")]
        public async Task<IActionResult> GetPageDataQuestions(QuestionSearchModel questionSearchModel)
        {
            try
            {
                List<QuestionOptionView> questionOptionViews = new List<QuestionOptionView>();
                var options = await _optionRepository.GetAllAsync();
                var questions = await _questionRepository.GetAllAsync();
                if (questionSearchModel.Grade == 10 || questionSearchModel.Grade == 11 || questionSearchModel.Grade == 12)
                {
                    questions = questions.Where(q => q.Grade == questionSearchModel.Grade).ToList();
                }
                if (questionSearchModel.Unit > 0)
                {
                    questions = questions.Where(q => q.Unit == questionSearchModel.Unit).ToList();
                }
                if (!string.IsNullOrEmpty(questionSearchModel.ProbabilityOrStatistic))
                {
                    questions = questions.Where(q => q.ProbabilityOrStatistic == questionSearchModel.ProbabilityOrStatistic).ToList();
                }
                if (!string.IsNullOrEmpty(questionSearchModel.Content))
                {
                    questions = questions.Where(q => q.Content.Contains(questionSearchModel.Content)).ToList();
                }
                var totalCount = questions.Count();
                var pageSize = 8;
                questions = questions.Skip((questionSearchModel.pageIndex - 1) * pageSize).Take(pageSize).ToList();
                foreach (var question in questions)
                {
                    QuestionOptionView questionOptionView = new QuestionOptionView
                    {
                        QuestionId = question.Id,
                        Content = question.Content,
                        Grade = question.Grade,
                        Unit = question.Unit,
                        Answer = question.Answer,
                        ImageUrl = question.ImageUrl,
                        ProbabilityOrStatistic = question.ProbabilityOrStatistic,
                        CreateBy = question.CreateBy,
                        Options = options.Where(o => o.QuestionId == question.Id).ToList()
                    };
                    questionOptionViews.Add(questionOptionView);
                }
                return Ok(new QuestionResponseModel
                {
                    questionOptionViews = questionOptionViews,
                    TotalCount = totalCount
                });
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
                    Answer = question.Answer,
                    ImageUrl = question.ImageUrl,
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
                    Answer = questionOptipnView.Answer,
                    ImageUrl = questionOptipnView.ImageUrl,
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

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] Guid questionId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{questionId}{ext}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "questions");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/images/questions/{fileName}";
            return Ok(new { url });
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
                question.Answer = questionOptipnView.Answer;
                question.ImageUrl = questionOptipnView.ImageUrl;
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
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
        {
            try
            {
                var questions = await _questionRepository.GetAllAsync();
                var options = await _optionRepository.GetAllAsync();
                var question = questions.FirstOrDefault(q => q.Id == questionId);
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
