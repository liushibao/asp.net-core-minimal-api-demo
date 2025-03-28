using Redis.OM.Modeling;

namespace WebApi.Models
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "WxToken" })]
    public class WxToken
    {
        [RedisIdField]
        [Indexed]
        public string openid { get; set; }
        public string accessToken { get; set; }
        public long expiresIn { get; set; }
        public string refreshToken { get; set; }
        public string scope { get; set; }
        public long isSnapshotuser { get; set; }
        public string unionid { get; set; }
    }

}
