using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using AntDesign;
using Shared.Models;
using Shared.ModelView;
using System.Net.Http.Json;

namespace BlazorApp.Components
{
    public partial class AddEditQuestion : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public QuestionOptionView questionOptionView { get; set; } = new();
        public bool isLoading = false;
        public bool isAdd = false;
        public bool isEdit = false;
        public string token = string.Empty;

        protected override async void OnInitialized()
        {
            await LoadToken();
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

        private async Task OnFinish()
        {
            if (isAdd)
            {
                isLoading = true;
                try
                {
                    var response = await Http.PostAsJsonAsync("https://localhost:7247/api/Question/add", questionOptionView);
                    if (response.IsSuccessStatusCode)
                    {
                        isLoading = false;
                        await _notification.Success(new NotificationConfig()
                        {
                            Message = "Thêm câu hỏi thành công",
                            Duration = 2,
                        });
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
    }
}
