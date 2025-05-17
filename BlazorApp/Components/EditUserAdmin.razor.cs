using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.ModelAddEdit;
using System.Net.Http.Json;
using Shared.Models;

namespace BlazorApp.Components
{
    public partial class EditUserAdmin : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Parameter] public User user { get; set; }
        private string token = string.Empty;
        bool isLoading = false;

        protected override void OnInitialized()
        {
            user = new User();
            user.Email = string.Empty;
            user.FullName = string.Empty;
            user.Password = string.Empty;
            user.Role = string.Empty;
            user.UserName = string.Empty;
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
            isLoading = true;
            try
            {
                await LoadToken();
                var response = await Http.PutAsJsonAsync($"https://localhost:7247/api/User/admin-update", user);
                if (response.IsSuccessStatusCode)
                {
                    await _notification.Success(new NotificationConfig()
                    {
                        Message = "Cập nhật thành công.",
                        Duration = 2,
                    });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await _notification.Error(new NotificationConfig()
                    {
                        Message = "Có lỗi trong quá trình thực hiện, vui lòng thử lại sau.",
                        Description = error,
                        Duration = 2,
                    });
                }
            }
            catch (Exception ex)
            {
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Có lỗi trong quá trình thực hiện, vui lòng thử lại sau.",
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
                Message = "Thông tin không hợp lệ, vui lòng kiểm tra lại.",
                Duration = 2,
            });
        }
    }
}
