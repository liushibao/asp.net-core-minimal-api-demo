using SqlSugar;

namespace WebApi.Models
{
    [SugarIndex("info_created_at", "CreatedAt", OrderByType.Asc)]
    public class Info
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Detail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
