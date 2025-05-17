using AntDesign.Locales;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using AntDesign;
using Microsoft.JSInterop;
using System.Security.Claims;
using BlazorApp.Components;
using System.Collections.Generic;

namespace BlazorApp.Pages
{
    public partial class Login : ComponentBase
    {
        [Inject] HttpClient Http { get; set; } = default!;
        [Inject] INotificationService _notification { get; set; } = default!;
        [Inject] IJSRuntime JS { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; } = default!;
        Add_EditUser addEditUser;
        public LoginModel loginModel = new LoginModel();
        public bool isLogin = false;
        public bool isAdd = false;

        protected override void OnInitialized()
        {
        }

        private async Task OnFinish()
        {
            try
            {
                isLogin = true;
                var response = await Http.PostAsJsonAsync("https://localhost:7247/api/User/login", loginModel);
                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(token))
                    {
                        await _notification.Error(new NotificationConfig()
                        {
                            Message = "Đăng nhập không thành công. Vui lòng kiểm tra lại tên đăng nhập và mật khẩu.",
                            Duration = 2,
                        });

                        return;
                    }
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                    var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
                    var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                    var fullName = jwtToken.Claims.FirstOrDefault(c => c.Type == "fullname")?.Value;
                    var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
                    await JS.InvokeVoidAsync("sessionStorage.setItem", "token", token);
                    await JS.InvokeVoidAsync("sessionStorage.setItem", "userid", userId);
                    await JS.InvokeVoidAsync("sessionStorage.setItem", "username", userName);
                    await JS.InvokeVoidAsync("sessionStorage.setItem", "password", loginModel.Password);
                    await JS.InvokeVoidAsync("sessionStorage.setItem", "email", email);
                    await JS.InvokeVoidAsync("sessionStorage.setItem", "fullname", fullName);
                    await JS.InvokeVoidAsync("sessionStorage.setItem", "role", role);

                    await _notification.Success(new NotificationConfig()
                    {
                        Message = "Đăng nhập thành công.",
                        Duration = 2,
                    });

                    NavigationManager.NavigateTo("/home");

                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    isLogin = false;
                    await _notification.Error(new NotificationConfig()
                    {
                        Message = "Đăng nhập không thành công. Vui lòng kiểm tra lại tên đăng nhập và mật khẩu.",
                        Description = error,
                        Duration = 2,
                    });
                }
            }
            catch (Exception ex)
            {
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Đăng nhập không thành công. Có lỗi trong quá trình đăng nhập, vui lòng thử lại sau.",
                    Description = ex.Message,
                    Duration = 2,
                });
            }
            finally
            {
                isLogin = false;
            }
        }

        private async Task OnFinishFailed()
        {
            isLogin = false;
            await _notification.Error(new NotificationConfig()
            {
                Message = "Đăng nhập không thành công. Tên đăng nhập và mật khẩu không hợp lệ.",
                Duration = 2,
            });
        }

        private void OnClodeDrawer()
        {
            isAdd = false;
        }
    }
}
