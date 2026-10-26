using Microsoft.EntityFrameworkCore;

namespace Rogbau.Api.Common.Pagination
{
    public class PaginatedList<T>
    {
        /// <summary>
        /// Generyczna lista pagingowa
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Elementy na aktualnej stronie
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Liczba elementow na stronie
        /// </summary>
        public int PageSize { get; set; }


        /// <summary>
        /// Calkowita liczba elementow
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Calkowita liczba stron
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Czy istnieje poprzednia strona
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;


        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            //liczymy wszystkie rekordy
            var count = await source.CountAsync(cancellationToken);

            //pobieryemy wymagana strone
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
