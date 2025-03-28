using Redis.OM.Modeling;

namespace WebApi.Models
{
    public abstract class PagedRes<T>
    {
        [RedisIdField]
        [Indexed]
        public string Query { get; set; }
        public PagedData<T> Data { get; set; }
    }

    public class PagedData<T>
    {
        public long PageNumber { get; set; }
        public long PageSize { get; set; }
        public long TotalCount { get; set; }
        public long TotalPage { get; set; }
        public T[] Data { get; set; }
    }
}
