using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService.Core.Entities;
using Mingo.ObjectStorageService.Core.Stores;
using Mingo.ObjectStorageService.Models;

namespace Mingo.ObjectStorageService.Pages
{
    public class BucketDetailsModel : PageModel
    {
        private readonly IObjectStore _objectStore;

        public BucketDetailsModel(IObjectStore objectStore)
        {
            _objectStore = objectStore;
        }

        public PaginatedList<ObjectEntity>? ObjectEntities { get; set; }

        public async Task OnGetAsync()
        {
            var query = _objectStore.All.AsNoTracking()
            .Where(x => x.BucketName == this.Bucket)
            .OrderByDescending(x => x.LastModified);
            ObjectEntities = await PaginatedList<ObjectEntity>.CreateAsync(
                query,
                CurrentPage,
                PageSize);
        }

        [BindProperty(SupportsGet = true)]
        public string Bucket { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
