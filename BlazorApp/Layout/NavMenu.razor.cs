using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp.Layout
{
    public partial class NavMenu : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        public string FullName { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadData();
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
            FullName = await JS.InvokeAsync<string>("sessionStorage.getItem", "fullname");
        }

        private async Task Logout()
        {
            await JS.InvokeVoidAsync("sessionStorage.clear");
            NavigationManager.NavigateTo("/");
        }
    }
}
