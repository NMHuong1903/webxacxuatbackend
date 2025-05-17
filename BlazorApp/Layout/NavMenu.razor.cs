using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorApp.Components;
using Shared.ModelAddEdit;
using Shared.Models;
using System.Collections.Generic;

namespace BlazorApp.Layout
{
    public partial class NavMenu : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        Add_EditUser addEditUser;
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Guid UserId { get; set; } 
        public UserAddEditModel userAddEditModel = new UserAddEditModel();

        public bool isEdit = false;

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
            UserId = await JS.InvokeAsync<Guid>("sessionStorage.getItem", "userid");
            FullName = await JS.InvokeAsync<string>("sessionStorage.getItem", "fullname");
            UserName = await JS.InvokeAsync<string>("sessionStorage.getItem", "username");
            Email = await JS.InvokeAsync<string>("sessionStorage.getItem", "email");
            Password = await JS.InvokeAsync<string>("sessionStorage.getItem", "password");
        }

        private void OnEditUser()
        {
            isEdit = true;
            userAddEditModel.Id = UserId;
            userAddEditModel.FullName = FullName;
            userAddEditModel.UserName = UserName;
            userAddEditModel.Email = Email;
            userAddEditModel.Password = Password;
        }

        private async Task Logout()
        {
            await JS.InvokeVoidAsync("sessionStorage.clear");
            NavigationManager.NavigateTo("/");
        }

        private void OnClodeDrawer()
        {
            isEdit = false;
        }
    }
}
