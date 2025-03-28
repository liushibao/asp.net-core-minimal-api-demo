using SqlSugar;

namespace WebApi.Models
{
    public class Gdp
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Year { get; set; }
        public int Amount { get; set; }
        public double GrowthRate { get; set; }
    }
}
