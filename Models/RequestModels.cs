using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class SendSmsCodeRequest
    {
        [RegularExpression(@"^(?:(?:\+|00)86)?1(?:(?:3[\d])|(?:4[5-79])|(?:5[0-35-9])|(?:6[5-7])|(?:7[0-8])|(?:8[\d])|(?:9[1589]))\d{8}$", ErrorMessage = "手机号错误")]
        [Required(ErrorMessage = "请填写手机号")]
        public string? Mob { get; set; }
    }

    public class VerifySmsCodeRequest
    {
        [Required(ErrorMessage = "请填写手机号")]
        [RegularExpression(@"^(?:(?:\+|00)86)?1(?:(?:3[\d])|(?:4[5-79])|(?:5[0-35-9])|(?:6[5-7])|(?:7[0-8])|(?:8[\d])|(?:9[1589]))\d{8}$", ErrorMessage = "手机号错误")]
        public string? Mob { get; set; }
        [Required(ErrorMessage = "请填写手机验证码")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "手机验证码格式错误")]
        public string? SmsCode { get; set; }
    }

    public class UserRegRequest
    {
        public string? Mob { get; set; }
        public string? Name { get; set; }
        public string? IdCardNumber { get; set; }
        public string? Birthday { get; set; }
    }

    public class GdpDataRequest
    {
        [Required]
        public int? YearStart { get; set; }
        [Required]
        public int? YearEnd { get; set; }
    }

    public class PagedRequest
    {
        [Required]
        public int? PageNumber { get; set; }
        [Required]
        public int? PageSize { get; set; }
    }

    public class InfoRequest : PagedRequest
    {

    }

    public class LoginRequest
    {
        [Required]
        public string? redirect_uri { get; set; }
    }

    public class FakeWeixinLoginRequest
    {
        [Required]
        public string? redirect_uri { get; set; }
    }

    public class GetTokenRequest
    {
        [Required]
        public string? code { get; set; }
    }

}
