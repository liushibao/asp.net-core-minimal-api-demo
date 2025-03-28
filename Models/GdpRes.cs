using Redis.OM.Modeling;

namespace WebApi.Models
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "GdpRes" })]
    public class GdpRes
    {
        [RedisIdField]
        [Indexed]
        public string Query { get; set; }
        public Gdp[] Data { get; set; }
    }
}
