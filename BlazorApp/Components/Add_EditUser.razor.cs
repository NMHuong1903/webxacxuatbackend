using Microsoft.AspNetCore.Components;
using Shared.ModelAddEdit;
using Shared.Models;
using System.Net.Http.Json;
using AntDesign;

namespace BlazorApp.Components
{
    public partial class Add_EditUser : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Parameter] public bool IsEdit { get; set; } = false;
        [Parameter] public bool IsAdd { get; set; } = false;

        [Parameter] public UserAddEditModel? userInDb { get; set; } = null;
        public UserAddEditModel userAddEditModel = new UserAddEditModel();
        bool isLoading = false;

        protected override void OnInitialized()
        {
            if (IsEdit && userInDb != null)
            {
                userAddEditModel.FullName = userInDb.FullName;
                userAddEditModel.Email = userInDb.Email;
                userAddEditModel.UserName = userInDb.UserName;
                userAddEditModel.Password = userInDb.Password;
                userAddEditModel.Id = userInDb.Id;
            }
            else if (IsAdd)
            {
                userAddEditModel = new UserAddEditModel();
            }
        }

        private async Task OnFinish()
        {
            if (IsEdit)
            {
                isLoading = true;
                await Http.PutAsJsonAsync($"https://localhost:7247/api/User", userAddEditModel);
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
                catch(Exception ex)
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
