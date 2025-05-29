using Microsoft.AspNetCore.Components;
using Shared.ModelView;
using Shared.Models;
using Microsoft.JSInterop;

namespace BlazorApp.Components
{
    public partial class RenderQuestion : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public EventCallback<Option> OnClick { get; set; } 
        [Parameter] public QuestionOptionView questionOptionView { get; set; }
        [Parameter] public Option selectedOption { get; set; } = new();
        bool isSubmitted = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JS.InvokeVoidAsync("RenderMath");
        }

        protected override void OnInitialized()
        {
        }

        void HandleClick(Option selected)
        {
            selectedOption = selected;
            OnClick.InvokeAsync(selected);
        }
    }
}
