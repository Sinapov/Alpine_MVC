using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Models
{
    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public class PaginatedList<T>(List<T> items, int count, int pageIndex, int pageSize, string sortColumn, SortOrder sortOrder)
    {
        public List<T> Items { get; } = items;
        public int PageIndex { get; } = pageIndex;
        public int TotalPages { get; } = (int)Math.Ceiling(count / (double)pageSize);
        public string SortColumn { get; } = sortColumn;
        public SortOrder SortOrder { get; } = sortOrder;

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