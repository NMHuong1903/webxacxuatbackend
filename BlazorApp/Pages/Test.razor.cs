using AntDesign;
using BlazorApp.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Models;
using Shared.ModelView;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BlazorApp.Pages
{
    public partial class Test : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IMessageService _message { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        public RenderQuestion renderQuestion;
        private List<StudentAnswer> studentAnswers = new List<StudentAnswer>();
        private List<Option> selectedOptions = new List<Option>();
        private Option selectedOption = new Option();
        private StudentExam studentExam;
        private ExamView examView = new();
        private Guid examId;
        public bool isLoading = false;
        public string token = string.Empty;
        public bool isSubmitted = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JS.InvokeVoidAsync("RenderMath");
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                examId = await JS.InvokeAsync<Guid>("sessionStorage.getItem", "examid");
                InitStudentExam();
                await LoadToken();
                await _message.LoadingWhen(
                        async () =>
                        {
                            await LoadData();
                        },
                        "Đang tải dữ liệu vui lòng đợi trong giây lát..."
                    );
            }
            catch (Exception ex)
            {
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Tải dữ liệu không thành công, vui lòng thử lại sau.",
                    Description = ex.Message,
                    Duration = 2,
                });
            }
        }

        private void InitStudentExam()
        {
            studentExam = new StudentExam
            {
                Id = Guid.NewGuid(),
                ExamId = examId,
                StartAt = DateTime.Now
            };
        }

        private async Task LoadToken()
        {
            try
            {

                token = await JS.InvokeAsync<string>("sessionStorage.getItem", "token");

                if (!string.IsNullOrEmpty(token))
                {
                    Http.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (Exception ex)
            {
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Tải dữ liệu không thành công, vui lòng thử lại sau.",
                    Description = ex.Message,
                    Duration = 2,
                });
            }
        }

        private async Task LoadData()
        {
            var response = await Http.GetAsync($"https://localhost:7247/api/Exam/get-by-id/{examId}");
            if (response.IsSuccessStatusCode)
            {
                var exam = await response.Content.ReadFromJsonAsync<ExamView>();
                if (exam != null)
                {
                    examView = exam;
                }
                else
                {
                    await _notification.Error(new NotificationConfig()
                    {
                        Message = "Không tìm thấy bài thi.",
                        Duration = 2,
                    });
                }
            }
            else
            {
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Tải dữ liệu không thành công, vui lòng thử lại sau.",
                    Description = response.ReasonPhrase,
                    Duration = 2,
                });
            }
        }

        public void OnAnswerSelected(Option option)
        {
            selectedOption = option;
            foreach (var ans in selectedOptions)
            {
                if (ans.QuestionId == option.QuestionId)
                {
                    selectedOptions.Remove(ans);
                    break;
                }
            }
            selectedOptions.Add(option);
        }

        public async Task SubmitExam()
        {
            try
            {
                isLoading = true;
                studentExam.CompleteAt = DateTime.Now;
                studentExam.Score = CalculateScore();
                studentExam.StudentId = await JS.InvokeAsync<Guid>("sessionStorage.getItem", "userid");
                var response = await Http.PostAsJsonAsync("https://localhost:7247/api/StudentExam/submit", studentExam);
                if (response.IsSuccessStatusCode)
                {
                    isSubmitted = true;
                    await _notification.Success(new NotificationConfig()
                    {
                        Message = "Nộp bài thành công.",
                        Duration = 2,
                    });
                }
                else
                {
                    await _notification.Error(new NotificationConfig()
                    {
                        Message = "Nộp bài không thành công, vui lòng thử lại sau.",
                        Description = response.ReasonPhrase,
                        Duration = 2,
                    });
                }
            }
            catch (Exception ex)
            {
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Lỗi khi nộp bài.",
                    Description = ex.Message,
                    Duration = 2,
                });
            }
            finally
            {
                isLoading = false;
            }
        }

        public int CalculateScore()
        {
            int score = 0;
            foreach (var question in examView.Questions)
            {
                var selectedOption = selectedOptions.FirstOrDefault(o => o.QuestionId == question.QuestionId);
                if (selectedOption != null && selectedOption.IsCorrect)
                {
                    score ++;
                }
            }
            return score;
        }
    }
}
