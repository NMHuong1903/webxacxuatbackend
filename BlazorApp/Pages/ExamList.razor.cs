using AntDesign.TableModels;
using AntDesign;
using BlazorApp.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.ModelView;
using Shared.ResponseModel;
using Shared.SearchModel;
using System.Net.Http.Json;

namespace BlazorApp.Pages
{
    public partial class ExamList : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IMessageService _message { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Parameter] public List<ExamView> selectedRows { get; set; } = new();
        [Parameter] public EventCallback<List<ExamView>> SelectedRowsChanged { get; set; }
        private AddEditExam addEditExam;
        public List<ExamView> examViews { get; set; } = new();
        public List<ExamView> examViewPage { get; set; } = new();
        public ExamSearchModel examSearchModel;
        public ExamView row { get; set; }
        public int TotalCount;
        public bool isLoading = false;
        public string token = string.Empty;
        public bool isadd = false;
        public bool isedit = false;
        string selectedName = string.Empty;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JS.InvokeVoidAsync("RenderMath");
        }
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

        async Task OnChange(QueryModel<ExamView> query)
        {
            examSearchModel.pageIndex = query.PageIndex;
            await LoadToken();
            await LoadData();
        }

        private void InitForm()
        {
            examSearchModel = new ExamSearchModel
            {
                Name = string.Empty,
                pageIndex = 1
            };
        }

        private Task OnSelectedRowsChanged(IEnumerable<ExamView> selected)
        {
            selectedRows = selected.ToList();
            SelectedRowsChanged.InvokeAsync(selectedRows);
            return Task.CompletedTask;
        }


        private async Task LoadData()
        {
            var response = await Http.PostAsJsonAsync("https://localhost:7247/api/Exam/get-page-data-with-filter", examSearchModel);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ExamResponseModel>();
                examViews = result.ExamViews.ToList();
                examViewPage = examViews.Take(8).ToList();
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

        private void OnEdit(ExamView context)
        {
            isedit = true;
            row = context;
        }

        private void OnAdd()
        {
            isadd = true;
        }

        private async Task OnAddEditSuccess()
        {
            await LoadData();

            OnClodeDrawer();

            StateHasChanged();
        }

        private async Task OnDelete(ExamView context)
        {
            isLoading = true;
            try
            {
                var response = await Http.DeleteAsync($"https://localhost:7247/api/Exam/delete/{context.ExamId}");
                if (response.IsSuccessStatusCode)
                {
                    await LoadData();
                    await _notification.Success(new NotificationConfig()
                    {
                        Message = "Xóa thành công",
                        Duration = 2,
                    });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await _notification.Error(new NotificationConfig()
                    {
                        Message = "Xóa không thành công, vui lòng thử lại sau.",
                        Description = error,
                        Duration = 2,
                    });
                }
            }
            catch (Exception ex)
            {
                await _notification.Error(new NotificationConfig()
                {
                    Message = "Xóa không thành công, vui lòng thử lại sau.",
                    Description = ex.Message,
                    Duration = 2,
                });
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task OnDeleteSelected()
        {
            if (selectedRows.Any())
            {
                foreach (var row in selectedRows)
                {
                    await OnDelete(row);
                }
            }
        }

        private void OnClodeDrawer()
        {
            isadd = false;
            isedit = false;
        }

        private async Task OnSelectName()
        {
            examSearchModel.Name = selectedName;
            examSearchModel.pageIndex = 1;
            await LoadToken();
            await LoadData();
        }
    }
}
