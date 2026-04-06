

namespace MovieMvcProject.Application.Commons
{
    public class PagedResult<T>
    {
        public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

      
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult(IReadOnlyCollection<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

      
        public PagedResult() { }

       
        public static PagedResult<T> Empty(int pageNumber, int pageSize) =>
            new PagedResult<T>(Array.Empty<T>(), 0, pageNumber, pageSize);
    }
}