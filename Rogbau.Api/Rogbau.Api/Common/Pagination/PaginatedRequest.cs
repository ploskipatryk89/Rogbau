namespace Rogbau.Api.Common.Pagination
{
    public class PaginatedRequest
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;


        /// <summary>
        /// Numer strony
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 1 : (value > 100 ? 100 : value);
        }
    }
}
