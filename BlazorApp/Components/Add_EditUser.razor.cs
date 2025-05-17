using Microsoft.AspNetCore.Components;
using Shared.ModelAddEdit;
using Shared.Models;
using System.Net.Http.Json;
using AntDesign;
using Microsoft.JSInterop;

namespace BlazorApp.Components
{
    public partial class Add_EditUser : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Parameter] public bool IsEdit { get; set; } = false;
        [Parameter] public bool IsAdd { get; set; } = false;
        //[Parameter] public UserAddEditModel userInDb { get; set; } = new UserAddEditModel();
        [Parameter] public UserAddEditModel userAddEditModel { get; set; } = new UserAddEditModel();
        private string token = string.Empty;
        bool isLoading = false;

        protected override void OnInitialized()
        {

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
            if (IsEdit)
            {
                isLoading = true;
                try
                {
                    await LoadToken();
                    var response = await Http.PutAsJsonAsync($"https://localhost:7247/api/User/update", userAddEditModel);
                    if (response.IsSuccessStatusCode)
                    {
                        await JS.InvokeVoidAsync("sessionStorage.setItem", "password", userAddEditModel.Password);
                        await JS.InvokeVoidAsync("sessionStorage.setItem", "email", userAddEditModel.Email);
                        await JS.InvokeVoidAsync("sessionStorage.setItem", "fullname", userAddEditModel.FullName);
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
            else if (IsAdd)
            {
                isLoading = true;
                try
                {
                    var response = await Http.PostAsJsonAsync("https://localhost:7247/api/User/add", userAddEditModel);
                    if (response.IsSuccessStatusCode)
                    {
                        await _notification.Success(new NotificationConfig()
                        {
                            Message = "Thêm mới thành công.",
                            Duration = 2,
                        });
                        userAddEditModel = new UserAddEditModel();
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
