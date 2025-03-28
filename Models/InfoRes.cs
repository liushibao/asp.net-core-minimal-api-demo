using Redis.OM.Modeling;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json.Serialization;

namespace WebApi.Models
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "InfoRes" })]
    public class InfoRes : PagedRes<Info>
    {
    }
}
