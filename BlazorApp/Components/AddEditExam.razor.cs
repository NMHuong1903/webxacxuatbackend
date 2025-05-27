using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Models;
using Shared.ModelView;
using Shared.ResponseModel;
using System.Net.Http.Json;
using BlazorApp.Pages;

namespace BlazorApp.Components
{
    public partial class AddEditExam : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public ExamView examView { get; set; }
        [Parameter] public EventCallback OnSuccess { get; set; }
        public bool isLoading = false;
        [Parameter] public bool isAdd { get; set; } = false;
        [Parameter] public bool isEdit { get; set; } = false;
        public string token = string.Empty;
        public QuestionList questionList;

        protected override async void OnInitialized()
        {
            if (examView == null)
            {
                InitForm();
            }
            await LoadToken();
        }

        private void InitForm()
        {
            examView = new ExamView();
            examView.Questions = new List<QuestionOptionView>();
            examView.ExamId = Guid.NewGuid();
        }

        private void OnQuestionsChanged(List<QuestionOptionView> questionOptionViews)
        {
            examView.Questions = questionOptionViews;
            examView.TotalQuestions = questionOptionViews.Count;
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

        private async Task OnFinish()
        {
            if (isAdd)
            {
                isLoading = true;
                try
                {
                    var UserId = await JS.InvokeAsync<Guid>("sessionStorage.getItem", "userid");
                    examView.CreateBy = UserId;
                    var response = await Http.PostAsJsonAsync("https://localhost:7247/api/Exam/add", examView);
                    if (response.IsSuccessStatusCode)
                    {
                        isLoading = false;
                        await _notification.Success(new NotificationConfig()
                        {
                            Message = "Thêm bài kiểm tra thành công",
                            Duration = 2,
                        });
                        await OnSuccess.InvokeAsync();
                        InitForm();
                    }
                    else
                    {
                        isLoading = false;
                        var error = await response.Content.ReadAsStringAsync();
                        await _notification.Error(new NotificationConfig()
                        {
                            Message = "Thêm bài kiểm tra không thành công, vui lòng thử lại sau.",
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
                        Message = "Thêm bài kiểm tra không thành công, vui lòng thử lại sau.",
                        Description = ex.Message,
                        Duration = 2,
                    });
                }
                finally
                {
                    isLoading = false;
                }
            }
            else if (isEdit)
            {
                isLoading = true;
                try
                {
                    var response = await Http.PutAsJsonAsync("https://localhost:7247/api/Question/update", examView);
                    if (response.IsSuccessStatusCode)
                    {
                        isLoading = false;
                        await _notification.Success(new NotificationConfig()
                        {
                            Message = "Cập nhật bài kiểm tra thành công",
                            Duration = 2,
                        });
                        await OnSuccess.InvokeAsync();
                    }
                    else
                    {
                        isLoading = false;
                        var error = await response.Content.ReadAsStringAsync();
                        await _notification.Error(new NotificationConfig()
                        {
                            Message = "Cập nhật bài kiểm tra không thành công, vui lòng thử lại sau.",
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
                        Message = "Cập nhật bài kiểm tra không thành công, vui lòng thử lại sau.",
                        Description = ex.Message,
                        Duration = 2,
                    });
                }
                finally
                {
                    isLoading = false;
                }
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
