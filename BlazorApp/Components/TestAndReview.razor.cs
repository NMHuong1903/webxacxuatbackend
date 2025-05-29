using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Models;
using Shared.ModelView;
using System.Net.Http.Json;

namespace BlazorApp.Components
{
    public partial class TestAndReview : ComponentBase, IDisposable
    {
        [Parameter] public bool isSubmitted { get; set; } = false;

        private ExamView examView = new();
        private List<Option> selectedOptions = new();
        private StudentExam studentExam;
        private string token = string.Empty;
        private Guid examId;
        private bool isLoading = false;
        private TimeSpan TimeLeft;
        private System.Timers.Timer? countdownTimer;
        private bool isTimeUp = false;


        protected override async Task OnInitializedAsync()
        {
            try
            {
                examId = await JS.InvokeAsync<Guid>("sessionStorage.getItem", "examid");
                await LoadToken();
                InitStudentExam();
                await _message.LoadingWhen(async () =>
                {
                    await LoadData();
                }, "Đang tải dữ liệu vui lòng đợi trong giây lát...");
                TimeLeft = TimeSpan.FromMinutes(examView.Duration);
                StartCountdownTimer();
            }
            catch (Exception ex)
            {
                await _notification.Error(new NotificationConfig
                {
                    Message = "Tải dữ liệu không thành công",
                    Description = ex.Message,
                    Duration = 3
                });
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("RenderMath");
            }
        }

        private void StartCountdownTimer()
        {
            countdownTimer = new System.Timers.Timer(1000);
            countdownTimer.Elapsed += async (sender, e) =>
            {
                if (TimeLeft.TotalSeconds > 1)
                {
                    TimeLeft = TimeLeft.Subtract(TimeSpan.FromSeconds(1));
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    countdownTimer?.Stop();
                    isTimeUp = true;

                    await InvokeAsync(async () =>
                    {
                        await _notification.Warning(new NotificationConfig
                        {
                            Message = "Đã hết thời gian làm bài",
                            Duration = 3
                        });

                        if (!isSubmitted)
                        {
                            await SubmitExam();
                        }
                    });
                }
            };
            countdownTimer.AutoReset = true;
            countdownTimer.Start();
        }

        public void Dispose()
        {
            countdownTimer?.Dispose();
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
            token = await JS.InvokeAsync<string>("sessionStorage.getItem", "token");
            if (!string.IsNullOrEmpty(token))
            {
                Http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task LoadData()
        {
            var response = await Http.GetAsync($"https://localhost:7247/api/Exam/get-by-id/{examId}");
            if (response.IsSuccessStatusCode)
            {
                var exam = await response.Content.ReadFromJsonAsync<ExamView>();
                examView = exam ?? new ExamView();
            }
            else
            {
                await _notification.Error(new NotificationConfig
                {
                    Message = "Không thể tải bài thi",
                    Description = response.ReasonPhrase,
                    Duration = 2
                });
            }
        }

        private void OnAnswerSelected(Guid questionId, Guid optionId)
        {
            var question = examView.Questions.FirstOrDefault(q => q.QuestionId == questionId);
            if (question == null) return;

            var option = question.Options.FirstOrDefault(o => o.Id == optionId);
            if (option == null) return;

            selectedOptions.RemoveAll(o => o.QuestionId == questionId);
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

                var response = await Http.PostAsJsonAsync("https://localhost:7247/api/StudentExam/add", studentExam);
                if (response.IsSuccessStatusCode)
                {
                    isSubmitted = true;
                    countdownTimer?.Stop();
                    await _notification.Success(new NotificationConfig
                    {
                        Message = "Nộp bài thành công",
                        Duration = 2
                    });
                }
                else
                {
                    await _notification.Error(new NotificationConfig
                    {
                        Message = "Nộp bài không thành công",
                        Description = response.ReasonPhrase,
                        Duration = 2
                    });
                }
            }
            catch (Exception ex)
            {
                await _notification.Error(new NotificationConfig
                {
                    Message = "Lỗi khi nộp bài",
                    Description = ex.Message,
                    Duration = 2
                });
            }
            finally
            {
                isLoading = false;
            }
        }

        private int CalculateScore()
        {
            int score = 0;
            foreach (var question in examView.Questions)
            {
                var selected = selectedOptions.FirstOrDefault(o => o.QuestionId == question.QuestionId);
                if (selected != null && selected.IsCorrect)
                {
                    score++;
                }
            }
            return score;
        }
    }
}
