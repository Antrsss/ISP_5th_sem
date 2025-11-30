using Microsoft.AspNetCore.Components;
using WEB_353502_ZGIRSKAYA.BlazorWasm.Models;

namespace WEB_353502_ZGIRSKAYA.BlazorWasm.Components
{
    public partial class CocktailDetails
    {
        [Parameter]
        public CocktailDetailsDto? SelectedCocktail { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        private async Task ClearSelection()
        {
            SelectedCocktail = null;
            await OnClose.InvokeAsync();
        }
    }
}