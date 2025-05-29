using AntDesign;
using BlazorApp.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Models;
using Shared.ModelView;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BlazorApp.Pages
{
    public partial class Test : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        public TestAndReview testAndReview { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JS.InvokeVoidAsync("RenderMath");
        }

        protected override async Task OnInitializedAsync()
        {
        }
    }
}
