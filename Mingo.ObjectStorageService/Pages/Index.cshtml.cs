using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService.Core.Entities;
using Mingo.ObjectStorageService.Core.Stores;
using Mingo.ObjectStorageService.Models;

namespace Mingo.ObjectStorageService.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IBucketStore _bucketStore;

        public IndexModel(IBucketStore bucketStore)
        {
            _bucketStore = bucketStore;
        }

        public PaginatedList<BucketEntity> Buckets { get; set; } = new PaginatedList<BucketEntity>([], 0, 1, 20);

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public async Task OnGetAsync()
        {
            IQueryable<BucketEntity> buckets = _bucketStore.All;

            Buckets = await PaginatedList<BucketEntity>.CreateAsync(
                buckets.AsNoTracking(), CurrentPage, PageSize);
        }
    }
}
