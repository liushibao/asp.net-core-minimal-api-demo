using FluentValidation;
using WebApi.Models;

namespace WebApi.Services
{
    public class UserRegRequestValidator : AbstractValidator<UserRegRequest>
    {
        public UserRegRequestValidator()
        {
            RuleFor(x => x.Mob)
            .NotNull().WithMessage("请填写手机号")
            .Matches(@"^(?:(?:\+|00)86)?1(?:(?:3[\d])|(?:4[5-79])|(?:5[0-35-9])|(?:6[5-7])|(?:7[0-8])|(?:8[\d])|(?:9[1589]))\d{8}$").WithMessage("手机号格式错误");

            RuleFor(x => x.Name)
            .NotNull().WithMessage("请填写姓名")
            .Matches(@"^(?:[\u4e00-\u9fa5·]{2,16})$").WithMessage("姓名格式错误");

            RuleFor(x => x.Birthday)
            .NotNull().WithMessage("请填写生日")
            .Must(birthday =>
            {
                var now = DateTime.Now;
                DateTime b;
                if (!DateTime.TryParse(birthday, null, out b))
                    return false;
                if (b > now)
                    return false;
                var diff = now.Year - b.Year;
                return now > b && diff < 120;
            }).WithMessage("生日格式错误");

            RuleFor(x => x.IdCardNumber)
            .NotNull().WithMessage("请填写身份证号")
            .Must(idNumber =>
            {
                long n = 0;
                if (long.TryParse(idNumber.Remove(17), out n) == false
                    || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
                {
                    return false;//数字验证  
                }
                string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
                if (address.IndexOf(idNumber.Remove(2)) == -1)
                {
                    return false;//TODO 省份验证或可更加细化  
                }
                string birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
                DateTime time = new DateTime();
                if (DateTime.TryParse(birth, out time) == false)
                {
                    return false;//生日验证  
                }
                string[] arrVarifyCode = "1,0,x,9,8,7,6,5,4,3,2".Split(',');
                string[] Wi = "7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2".Split(',');
                char[] Ai = idNumber.Remove(17).ToCharArray();
                int sum = 0;
                for (int i = 0; i < 17; i++)
                {
                    sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
                }
                int y = -1;
                Math.DivRem(sum, 11, out y);
                if (arrVarifyCode[y] != idNumber.Substring(17, 1).ToLower())
                {
                    return false;//校验码验证  
                }
                return true;
            }).WithMessage("身份证号格式错误");

            RuleFor(x => x).Must(x => x.Birthday?.Replace("-", "") == x.IdCardNumber?.Substring(6, 8)).WithMessage("身份证日期和生日不一致").WithName("Birthday");
        }
    }
}
