using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace MyImageBoard.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }

        public void OnGet()
        {
            ErrorCode = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error?.GetType().Name ?? "Unknown";
            Message = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error?.Message ?? "An unexpected error occurred.";
        }
    }
}