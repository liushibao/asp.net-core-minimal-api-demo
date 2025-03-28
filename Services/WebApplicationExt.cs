using Redis.OM;
using SqlSugar;
using System.Security.Cryptography;
using WebApi.Endpoints;
using WebApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApi.Services
{
    public static class WebApplicationExt
    {
        public static void AddSqlSugarDb(this IServiceCollection services)
        {
            services.AddSingleton<ISqlSugarClient>(s =>
            {
                SqlSugarScope sqlSugar = new SqlSugarScope(new ConnectionConfig()
                {
                    DbType = SqlSugar.DbType.SqlServer,
                    ConnectionString = EnvironmentConfig.Instance.SqlConnectionString,
                    IsAutoCloseConnection = true,
                },
               db =>
               {
                   //单例参数配置，所有上下文生效
                   db.Aop.OnLogExecuting = (sql, pars) =>
                   {
                       ////获取作IOC作用域对象
                       //var appServive = s.GetService<IHttpContextAccessor>();
                       //var obj = appServive?.HttpContext?.RequestServices.GetService<ILogger>();
                       //Console.WriteLine("AOP" + obj.GetHashCode());

                       Console.WriteLine(sql);
                       Console.WriteLine(pars);
                   };
               });
                return sqlSugar;
            });
        }

        public static void MapEndPoints(this WebApplication app)
        {
            var db = app.Services.GetService<ISqlSugarClient>();
            var redis = app.Services.GetService<RedisConnectionProvider>();
            var smsClient = app.Services.GetService<IMobVerifier>();
            var authEndpoint = new AuthEndpoint(db, redis, smsClient);
            authEndpoint.Map(app);
            var businessEndpoint = new BusinessEndpoint(db, redis);
            businessEndpoint.Map(app);
        }

        public static void ConfigDb(this WebApplication app)
        {
            var db = app.Services.GetService<ISqlSugarClient>();
            string tableName = db.EntityMaintenance.GetTableName(typeof(Info));
            //if (db.DbMaintenance.IsAnyTable(tableName, false))
            //    return;
            db.DbMaintenance.CreateDatabase();
            db.CodeFirst.InitTables<User>();
            db.CodeFirst.InitTables<Gdp>();
            db.CodeFirst.InitTables<Info>();

            if (db.Queryable<Info>().Count() == 0)
            {
                var now = DateTime.Now;
                for (int i = 1; i <= 100; i++)
                    db.Insertable<Info>(new
                    {
                        name = $"信息{i}",
                        detail = i % 3 == 0 ? $"这是一条比较长的信息{i}内容，用作展示两列不对称效果" : $"信息{i}内容",
                        contact = $"某某{i}",
                        createdAt = now.AddMinutes(i)
                    }).ExecuteCommand();
            }

            // 插入gdp模拟数据
            if (db.Queryable<Gdp>().Count() == 0)
            {
                for (int i = 2001; i <= 2024; i++)
                    db.Insertable<Gdp>(new
                    {
                        year = i,
                        amount = 10000 + RandomNumberGenerator.GetInt32(10, 1000),
                        growthRate = RandomNumberGenerator.GetInt32(-10, 10) * 0.01,
                    }).ExecuteCommand();
            }
        }
    }

}
