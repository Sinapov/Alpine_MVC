using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Models
{
    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageIndex { get; }
        public int TotalPages { get; }
        public string SortColumn { get; }
        public SortOrder SortOrder { get; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize, string sortColumn, SortOrder sortOrder)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            SortColumn = sortColumn;
            SortOrder = sortOrder;
            Items = items;
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, 
            int pageIndex, 
            int pageSize, 
            string sortColumn, 
            string sortOrder)
        {
            var count = await source.CountAsync();
            var actualSortOrder = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) 
                ? SortOrder.Descending 
                : SortOrder.Ascending;

            var items = await source.Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize, sortColumn, actualSortOrder);
        }
    }
}