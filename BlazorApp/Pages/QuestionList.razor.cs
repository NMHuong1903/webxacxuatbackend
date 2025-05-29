using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using AntDesign;
using AntDesign.TableModels;
using BlazorApp.Components;
using Shared.Models;
using Shared.ResponseModel;
using Shared.SearchModel;
using System.Net.Http.Json;
using Shared.ModelView;

namespace BlazorApp.Pages
{
    public partial class QuestionList : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private INotificationService _notification { get; set; } = default!;
        [Inject] private IMessageService _message { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Parameter] public List<QuestionOptionView> selectedRows { get; set; } = new();
        [Parameter] public EventCallback<List<QuestionOptionView>> SelectedRowsChanged { get; set; }
        [Parameter] public bool isTeacher { get; set; } = true;
        private AddEditQuestion addEditQuestion;
        public List<QuestionOptionView> questionOptionViews { get; set; } = new();
        public List<QuestionOptionView> questionOptionViewPage { get; set; } = new();
        public QuestionSearchModel questionSearchModel;
        public QuestionOptionView row { get; set; }
        public int TotalCount;
        public bool isLoading = false;
        public string token = string.Empty;
        public bool isadd = false;
        public bool isedit = false;
        string selectedProbabilityOrStatistic = string.Empty;
        public List<string> typeQuestion = new List<string>
        {
            "Xác suất",
            "Thống kê",
        };
        string selectedContent = string.Empty;
        int selectedGrade = 0;
        public List<int> grades = new List<int>
        {
            10,
            11,
            12
        };
        int selectedUnit = 0;

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

        async Task OnChange(QueryModel<QuestionOptionView> query)
        {
            questionSearchModel.pageIndex = query.PageIndex;
            await LoadToken();
            await LoadData();
        }

        private void InitForm()
        {
            questionSearchModel = new QuestionSearchModel
            {
                Grade = 0,
                Unit = 0,
                Content = string.Empty,
                ProbabilityOrStatistic = string.Empty,
                pageIndex = 1
            };
        }

        void OnSelectedRowsChanged(IEnumerable<QuestionOptionView> selected)
        {
            selectedRows = selected.ToList();
            SelectedRowsChanged.InvokeAsync(selectedRows);
        }


        private async Task LoadData()
        {
            var response = await Http.PostAsJsonAsync("https://localhost:7247/api/Question/get-page-data-with-filter", questionSearchModel);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<QuestionResponseModel>();
                questionOptionViews = result.questionOptionViews.ToList();
                questionOptionViewPage = questionOptionViews.Take(8).ToList();
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

        private void OnEdit(QuestionOptionView context)
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

        private async Task OnDelete(QuestionOptionView context)
        {
            isLoading = true;
            try
            {
                var response = await Http.DeleteAsync($"https://localhost:7247/api/Question/delete/{context.QuestionId}");
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

        private async Task OnSelectGrade()
        {
            questionSearchModel.Grade = selectedGrade;
            questionSearchModel.pageIndex = 1;
            await LoadToken();
            await LoadData();
        }

        private async Task OnSelectUnit()
        {
            questionSearchModel.Unit = selectedUnit;
            questionSearchModel.pageIndex = 1;
            await LoadToken();
            await LoadData();
        }

        private async Task OnSelectProbabilityOrStatistic()
        {
            questionSearchModel.ProbabilityOrStatistic = selectedProbabilityOrStatistic;
            questionSearchModel.pageIndex = 1;
            await LoadToken();
            await LoadData();
        }

        private async Task OnSelectContent()
        {
            questionSearchModel.Content = selectedContent;
            questionSearchModel.pageIndex = 1;
            await LoadToken();
            await LoadData();
        }
    }
}
