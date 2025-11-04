using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mingo.ObjectStorageService.Pages
{
    public class UploadModel : PageModel
    {
        public void OnGet()
        {
        }

        [BindProperty(SupportsGet = true)]
        public string Bucket { get; set; } = default!;
    }
}
