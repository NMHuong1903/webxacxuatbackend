using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using AntDesign;
using Shared.Models;
using Shared.ModelView;
using System.Net.Http.Json;
using Shared.ResponseModel;

namespace BlazorApp.Components
{
    public partial class AddEditQuestion : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public QuestionOptionView questionOptionView { get; set; } 
        public Dictionary<string, object> uploadProps { get; set; }
        public bool isLoading = false;
        [Parameter] public bool isAdd { get; set; } = false;
        [Parameter] public bool isEdit { get; set; } = false;
        public string token = string.Empty;
        public string selectedType = string.Empty;
        public List<string> typeQuestion = new List<string>
        {
            "Xác suất",
            "Thống kê",
        };
        public Option trueOption { get; set; } 
        public Option falseOption1 { get; set; }
        public Option falseOption2 { get; set; }
        public Option falseOption3 { get; set; }

        protected override async void OnInitialized()
        {
            InitForm();
            await LoadToken();
        }

        private void InitForm()
        {
            questionOptionView ??= new QuestionOptionView
            {
                QuestionId = Guid.NewGuid(),
                Options = new List<Option>()
            };

            trueOption = new Option
            {
                Id = Guid.NewGuid(),
                IsCorrect = true,
                QuestionId = questionOptionView.QuestionId
            };
            falseOption1 = new Option
            {
                Id = Guid.NewGuid(),
                IsCorrect = false,
                QuestionId = questionOptionView.QuestionId
            };
            falseOption2 = new Option
            {
                Id = Guid.NewGuid(),
                IsCorrect = false,
                QuestionId = questionOptionView.QuestionId
            };
            falseOption3 = new Option
            {
                Id = Guid.NewGuid(),
                IsCorrect = false,
                QuestionId = questionOptionView.QuestionId
            };
            uploadProps = new Dictionary<string, object>
            {
                { "questionId", questionOptionView.QuestionId }
            };
            selectedType = string.Empty;
        }

        private async Task LoadToken()
        {
            try
            {
                token = await JS.InvokeAsync<string>("sessionStorage.getItem", "token");

                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("đính kèm token");
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

        private void OnSingleCompleted(UploadInfo fileinfo)
        {
            if (fileinfo.File.State == UploadState.Success)
            {
                var result = fileinfo.File.GetResponse<UrlResponse>();
                questionOptionView.ImageUrl = result.url;
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
                    questionOptionView.CreateBy = UserId;
                    questionOptionView.Options.Add(trueOption);
                    questionOptionView.Options.Add(falseOption1);
                    questionOptionView.Options.Add(falseOption2);
                    questionOptionView.Options.Add(falseOption3);
                    var response = await Http.PostAsJsonAsync("https://localhost:7247/api/Question/add", questionOptionView);
                    if (response.IsSuccessStatusCode)
                    {
                        isLoading = false;
                        await _notification.Success(new NotificationConfig()
                        {
                            Message = "Thêm câu hỏi thành công",
                            Duration = 2,
                        });
                        InitForm();
                        StateHasChanged();
                    }
                    else
                    {
                        isLoading = false;
                        var error = await response.Content.ReadAsStringAsync();
                        await _notification.Error(new NotificationConfig()
                        {
                            Message = "Thêm câu hỏi không thành công, vui lòng thử lại sau.",
                            Description = error,
                            Duration = 2,
                        });
                    }
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    isLoading = false;
                    await _notification.Error(new NotificationConfig()
                    {
                        Message = "Thêm câu hỏi không thành công, vui lòng thử lại sau.",
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
                    var response = await Http.PutAsJsonAsync("https://localhost:7247/api/Question/update", questionOptionView);
                    if (response.IsSuccessStatusCode)
                    {
                        isLoading = false;
                        await _notification.Success(new NotificationConfig()
                        {
                            Message = "Cập nhật câu hỏi thành công",
                            Duration = 2,
                        });
                    }
                    else
                    {
                        isLoading = false;
                        var error = await response.Content.ReadAsStringAsync();
                        await _notification.Error(new NotificationConfig()
                        {
                            Message = "Cập nhật câu hỏi không thành công, vui lòng thử lại sau.",
                            Description = error,
                            Duration = 2,
                        });
                    }
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    isLoading = false;
                    await _notification.Error(new NotificationConfig()
                    {
                        Message = "Cập nhật câu hỏi không thành công, vui lòng thử lại sau.",
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

        private void OnSelectedType()
        { 
            if(!string.IsNullOrEmpty(selectedType))
            {
                questionOptionView.ProbabilityOrStatistic = selectedType;
            }
        }
    }
}
