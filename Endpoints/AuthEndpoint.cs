using Microsoft.AspNetCore.Mvc;
using System.Web;
using SqlSugar;
using WebApi.Services;
using Redis.OM;
using WebApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WebApi.Endpoints
{
    public class AuthEndpoint
    {
        private readonly ISqlSugarClient _db;
        private readonly RedisConnectionProvider _redis;
        private readonly IMobVerifier _smsClient;

        public AuthEndpoint(ISqlSugarClient db, RedisConnectionProvider redis, IMobVerifier smsClient)
        {
            this._smsClient = smsClient;
            _db = db;
            _redis = redis;
        }

        public void Map(WebApplication app)
        {
            var loginGroup = app.MapGroup("/api/auth/login");
            var regGroup = app.MapGroup("/api/auth/reg").RequireAuthorization();

            loginGroup.MapGet("/", Login).AddEndpointFilter(ValidateModel.ValidateModelFilter<LoginRequest>);
            loginGroup.MapGet("/FakeWeixinLogin", FakeWeixinLogin).AddEndpointFilter(ValidateModel.ValidateModelFilter<FakeWeixinLoginRequest>);
            loginGroup.MapGet("/Token", GetToken).AddEndpointFilter(ValidateModel.ValidateModelFilter<GetTokenRequest>);

            regGroup.MapPost("/SendSmsCode", SendSmsCode).AddEndpointFilter(ValidateModel.ValidateModelFilter<SendSmsCodeRequest>);
            regGroup.MapPost("/VerifySmsCode", VerifySmsCode).AddEndpointFilter(ValidateModel.ValidateModelFilter<VerifySmsCodeRequest>);
            regGroup.MapPost("/", Reg).AddEndpointFilter(ValidateModel.GetFluentValidateModelFilter<UserRegRequest>(new UserRegRequestValidator()));
        }


        private string GenerateToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim("id", userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, EnvironmentConfig.Instance.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, EnvironmentConfig.Instance.Audience)
                }),
                Expires = DateTime.Now.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(EnvironmentConfig.Instance.JwtKey)), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 由前端App根据身份认证需要调用
        /// </summary>
        /// <param name="redirect_uri">用户打开前端App需要身份认证的页面时的相对路径</param>
        /// <param name="context"></param>
        /// <returns></returns>
        private IResult Login([AsParameters] LoginRequest req, HttpContext context)
        {
            string redirect_uri = req.redirect_uri;
            // 根据微信开发文档 https://developers.weixin.qq.com/doc/offiaccount/OA_Web_Apps/Wechat_webpage_authorization.html#0
            var url = EnvironmentConfig.Instance.WxAppId == null
               ? $"{(context.Request.IsHttps ? "https" : "http")}://{context.Request.Host}/api/auth/login/FakeWeixinLogin?redirect_uri={HttpUtility.UrlEncode(redirect_uri)}&response_type=code&scope=snsapi_base#wechat_redirect"
               : $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={EnvironmentConfig.Instance.WxAppId}&redirect_uri={HttpUtility.UrlEncode(redirect_uri)}&response_type=code&scope=snsapi_base#wechat_redirect";
            Console.WriteLine(url);
            return Results.Redirect(url);
        }

        /// <summary>
        /// 开发调试用
        /// </summary>
        /// <param name="redirect_uri"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private IResult FakeWeixinLogin([AsParameters] FakeWeixinLoginRequest req)
        {
            string redirect_uri = req.redirect_uri;
            return Results.Redirect($"{redirect_uri}?code=1111");
        }

        /// <summary>
        /// 前端浏览器在Login重定向到微信授权后，再重定向到redirect_uri（前端APP），前端App加载后调用本api获取api访问token
        /// </summary>
        /// <param name="code">微信授权页面返回给redirect_uri的code，用于获取微信openid</param>
        /// <returns></returns>
        private async Task<IResult> GetToken([AsParameters] GetTokenRequest req)
        {
            string code = req.code;
            string wxOpenId;
            if (EnvironmentConfig.Instance.WxAppId != null)
            {
                // 根据微信开发文档 https://developers.weixin.qq.com/doc/offiaccount/OA_Web_Apps/Wechat_webpage_authorization.html#1
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync($"https://api.weixin.qq.com/sns/oauth2/access_token?appid=${EnvironmentConfig.Instance.WxAppId}&secret=${EnvironmentConfig.Instance.WxAppSecret}&code=${code}&grant_type=authorization_code");
                response.EnsureSuccessStatusCode();
                var wxToken = await response.Content.ReadFromJsonAsync<dynamic>();
                if (wxToken == null)
                    return Results.Problem(new ProblemDetails() { Title = "微信ID返回空值" });
                else if ((string)wxToken.errcode != null)
                    return Results.Problem(new ProblemDetails() { Title = "微信ID返回错误" });
                else
                {
                    wxOpenId = wxToken.openid;
                    var redisCollection = this._redis.RedisCollection<WxToken>();
                    await redisCollection.InsertAsync(wxToken as WxToken, new TimeSpan(0, Math.Floor(wxToken.expiresIn / 3600), 0));
                }
            }
            else
            {
                // 开发环境执行
                wxOpenId = code;
            }
            var user = await this._db.Queryable<User>().SingleAsync(t => t.WxOpenId == wxOpenId);
            if (user == null)
            {
                user = new User() { WxOpenId = wxOpenId };
                await this._db.Insertable<User>(user).ExecuteCommandAsync();
            }

            var token = GenerateToken(user.Id.ToString());

            return Results.Ok(new
            {
                token,
                user
            });
        }

        private async Task<IResult> SendSmsCode(SendSmsCodeRequest req, ClaimsPrincipal claims)
        {
            string userId = claims.FindFirstValue("id");
            var count = await _db.Queryable<User>().CountAsync(t => t.Mob == req.Mob && t.Id != int.Parse(userId));
            if (count > 0)
                return Results.BadRequest(new ProblemDetails() { Title = "手机号已绑定其他用户。" });
            var smsCode = new Random().Next(1000000, 9999999).ToString().Substring(0, 6);
            var result = await this._smsClient.SendSmsCode(req.Mob, [smsCode]);
            if (result == true)
            {
                var redisCollection = this._redis.RedisCollection<SmsCodeCache>();
                await redisCollection.InsertAsync(new SmsCodeCache() { Mob = req.Mob, SmsCode = smsCode, UserId = userId }, new TimeSpan(0, 10, 0));
                return Results.Ok(new { isSuccess = true, expireSeconds = 600 });
            }
            else
            {
                return Results.Problem(new ProblemDetails() { Title = "短信服务异常" });
            }
        }
        private async Task<IResult> VerifySmsCode(VerifySmsCodeRequest req, ClaimsPrincipal claims)
        {
            string userId = claims.FindFirstValue("id");
            var redisCollection = this._redis.RedisCollection<SmsCodeCache>();
            var smsCodeCache = await redisCollection.FindByIdAsync(userId);
            var isSuccess = smsCodeCache?.Mob == req.Mob && smsCodeCache?.SmsCode == req.SmsCode;
            if (isSuccess == true)
            {
                await _db.Updateable<User>().SetColumns(t => new User { Mob = smsCodeCache.Mob }).Where(t => t.Id == int.Parse(userId)).ExecuteCommandAsync();
            }
            return Results.Ok(new { isSuccess });
        }
        private async Task<IResult> Reg(UserRegRequest req, ClaimsPrincipal claims)
        {
            string userId = claims.FindFirstValue("id");
            await _db.Updateable<User>().SetColumns(t => new User()
            {
                Name = req.Name,
                IdCardNumber = req.IdCardNumber,
                Birthday = req.Birthday,
            }).Where(t => t.Id == int.Parse(userId) && t.Mob == req.Mob).ExecuteCommandAsync();
            var user = await _db.Queryable<User>().SingleAsync(t => t.Id == int.Parse(userId));
            return Results.Ok(new { isSuccess = true, user });
        }
    }

}
