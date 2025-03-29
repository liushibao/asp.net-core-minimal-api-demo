using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Redis.OM;
using Redis.OM.Searching;
using SqlSugar;
using System.Drawing.Printing;
using System.Web;
using WebApi.Models;
using WebApi.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApi.Endpoints
{
    public class BusinessEndpoint
    {
        private readonly ISqlSugarClient _db;
        private readonly RedisConnectionProvider _redis;

        public BusinessEndpoint(ISqlSugarClient db, RedisConnectionProvider redis)
        {
            this._db = db;
            _redis = redis;
        }

        public void Map(WebApplication app)
        {
            var publicGroup = app.MapGroup("/api/business/public");

            publicGroup.MapGet("/GdpData", GdpData).AddEndpointFilter(ValidateModel.ValidateModelFilter<GdpDataRequest>);
            publicGroup.MapGet("/Info", Info).AddEndpointFilter(ValidateModel.ValidateModelFilter<InfoRequest>);
        }

        private async Task<IResult> Info([AsParameters] InfoRequest req)
        {
            int pageNumber = (int)req.PageNumber;
            int pageSize = (int)req.PageSize;
            RefAsync<int> totalCount = 0;
            RefAsync<int> totalPage = 0;
            string query = $"{pageNumber}-{pageSize}";
            var redisCollection = this._redis.RedisCollection<InfoRes>();
            InfoRes cached = await redisCollection!.FindByIdAsync(query);
            if (cached == null)
            {
                Console.WriteLine("info cach not found");
                var data = await _db.Queryable<Info>().ToPageListAsync(pageNumber, pageSize, totalCount, totalPage);
                cached = new InfoRes()
                {
                    Query = query,
                    Data = new PagedData<Info>
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount.Value,
                        TotalPage = totalPage.Value,
                        Data = data.ToArray()
                    }
                };
                await redisCollection.InsertAsync(cached, new TimeSpan(0, 10, 0));
            }
            return Results.Ok(cached.Data);
        }

        private async Task<IResult> GdpData([AsParameters] GdpDataRequest req)
        {
            int yearStart = (int)req.YearStart;
            int yearEnd = (int)req.YearEnd;
            string query = $"{yearStart}-{yearEnd}";
            var redisCollection = this._redis.RedisCollection<GdpRes>();
            GdpRes cached = await redisCollection!.FindByIdAsync(query);
            if (cached == null)
            {
                Console.WriteLine("gdp cach not found");
                var data = await _db.Queryable<Gdp>().Where(t => t.Year >= yearStart && t.Year <= yearEnd).ToListAsync();
                cached = new GdpRes() { Query = query, Data = data.ToArray() };
                await redisCollection.InsertAsync(cached, new TimeSpan(24, 0, 0));
            }
            return Results.Ok(cached.Data);
        }
    }
}
