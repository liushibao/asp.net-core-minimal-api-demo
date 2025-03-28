using SqlSugar;

namespace WebApi.Models
{
    [SugarIndex("user_wxopenid", "WxOpenId", OrderByType.Asc)]
    [SugarIndex("user_mob", "Mob", OrderByType.Asc)]
    [SugarIndex("user_id_card_number", "IdCardNumber", OrderByType.Asc)]
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public string WxOpenId { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? Name { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? Mob { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? IdCardNumber { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? Birthday { get; set; }
    }


}
