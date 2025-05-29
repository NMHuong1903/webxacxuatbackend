using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OneOf.Types;
using Shared.ModelView;
using Shared.ResponseModel;
using Shared.SearchModel;
using System.Net.Http.Json;

namespace BlazorApp.Pages
{
    public partial class Practice : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;
        private ExamView examView { get; set; } = new();
        private List<QuestionOptionView> questionOptionViews { get; set; } = new();
        public bool isLoading = false;
        private string type { get; set; } = string.Empty;
        private int grade { get; set; } = 0;
        private int unit { get; set; } = 0;
        private string token { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadToken();
            await LoadData();
            InitForm();
        }

        private async Task LoadData()
        {
            try
            {
                type = await JS.InvokeAsync<string>("sessionStorage.getItem", "typeforpractice");
                var gradeString = await JS.InvokeAsync<string>("sessionStorage.getItem", "gradeforpractice");
                grade = int.TryParse(gradeString, out var g) ? g : 0;
                var unitString = await JS.InvokeAsync<string>("sessionStorage.getItem", "unitforpractice");
                unit = int.TryParse(unitString, out var u) ? u : 0;
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

        private void InitForm()
        {
            examView = new ExamView
            {
                ExamId = Guid.NewGuid(),
                Name = "Luyện tập " + type.ToLower() + " lớp " + grade + " bài " + unit,
                Questions = new List<QuestionOptionView>()
            };
            questionOptionViews = new List<QuestionOptionView>();
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

        private async Task LoadQuestion()
        {
            var questionSearchModel = new QuestionSearchModel
            {
                ProbabilityOrStatistic = type,
                Grade = grade,
                Unit = unit,
                TotalCount = examView.TotalQuestions,
            };
            var response = await Http.PostAsJsonAsync("https://localhost:7247/api/Question/get-page-data-for-practice", questionSearchModel);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<QuestionResponseModel>();
                questionOptionViews = result.questionOptionViews.ToList();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Tải dữ liệu không thành công, vui lòng thử lại sau.",
                    Description = error,
                    Duration = 2,
                });
            }
        }

        private async Task OnFinish()
        {
            isLoading = true;
            try
            {
                await LoadQuestion();
                var UserId = await JS.InvokeAsync<Guid>("sessionStorage.getItem", "userid");
                examView.CreateBy = UserId;
                examView.Questions = questionOptionViews;
                examView.CreatedAt = DateTime.Now;
                var response = await Http.PostAsJsonAsync("https://localhost:7247/api/Exam/add", examView);
                if (response.IsSuccessStatusCode)
                {
                    isLoading = false;
                    await _notification.Success(new NotificationConfig()
                    {
                        Message = "Tạo bài luyện tập thành công",
                        Duration = 2,
                    });
                    await JS.InvokeVoidAsync("sessionStorage.setItem", "examid", examView.ExamId.ToString());
                    NavigationManager.NavigateTo("/test");
                }
                else
                {
                    isLoading = false;
                    var error = await response.Content.ReadAsStringAsync();
                    await _notification.Error(new NotificationConfig()
                    {
                        Message = "Tạo bài luyện tập không thành công, vui lòng thử lại sau.",
                        Description = error,
                        Duration = 2,
                    });
                }
            }
            catch (Exception ex)
            {
                isLoading = false;
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Tạo bài luyện tập không thành công, vui lòng thử lại sau.",
                    Description = ex.Message,
                    Duration = 2,
                });
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task OnFinishFailed()
        {
            isLoading = false;
            await _notification.Error(new NotificationConfig()
            {
                Message = "Thông tin không hợp lệ, vui lòng thử lại sau.",
                Duration = 2,
            });
        }
    }
}
