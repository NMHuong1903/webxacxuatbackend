using Microsoft.AspNetCore.Components;
using AntDesign;
using Microsoft.JSInterop;
using Shared.Models;
using Shared.SearchModel;
using System.Net.Http.Json;
using Shared.ResponseModel;

namespace BlazorApp.Pages
{
    public partial class UserList : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IMessageService _message { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        public List<User> userList { get; set; } = new();
        public IEnumerable<User> selectedRows;
        public UserSearchModel userSearchModel;
        public User row { get; set; }
        public int TotalCount;
        public bool isLoading = false;
        public string token = string.Empty;
        protected override async Task OnInitializedAsync()
        {
            try
            {
                InitForm();
                await _message.LoadingWhen(
                    async () =>
                    {
                        //Phải gọi đầu tiên
                        await LoadToken();

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

        private void InitForm()
        {
            userSearchModel = new UserSearchModel
            {
                Id = Guid.Empty,
                FullName = string.Empty,
                Email = string.Empty,
                UserName = string.Empty,
                Role = string.Empty,
                pageIndex = 1
            };
        }

        private async Task LoadData()
        {
            var response = await Http.PostAsJsonAsync("https://localhost:7247/api/User/get-page-data-with-filter", userSearchModel);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserResponseModel>();
                userList = result.Users.ToList();
                TotalCount = result.TotalCount;
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
    }
}
