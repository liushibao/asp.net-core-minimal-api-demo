using Redis.OM.Modeling;

namespace WebApi.Models
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "SmsCodeCache" })]
    public class SmsCodeCache
    {
        [RedisIdField]
        [Indexed]
        public string UserId { get; set; }
        public string Mob { get; set; }
        public string SmsCode { get; set; }
    }
}
