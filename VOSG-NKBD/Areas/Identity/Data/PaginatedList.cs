using Microsoft.EntityFrameworkCore;

namespace VOSG_NKBD.Data
{
    // generic paging helper - <T> means it works for any model, not just Location.
    // adapted from the paging tutorial in the microsoft docs
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            // Ceiling rounds up, so 25 items / 10 per page = 3 pages not 2
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        // the view uses these to grey out the prev/next buttons
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            // Skip past the previous pages, then Take one page worth of rows
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}