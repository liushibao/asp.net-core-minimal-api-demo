using FluentValidation;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace WebApi.Services
{
    public static class ValidateModel
    {
        public static bool Try(object model, out Dictionary<string, string[]> validationResults)
        {
            validationResults = new Dictionary<string, string[]>();
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(model, context, results, true))
            {
                return true;
            }
            foreach (var validationResult in results)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
#pragma warning disable CS8620 // 由于引用类型的可为 null 性差异，实参不能用于形参。
                    validationResults.Add(memberName, new[] {
                validationResult.ErrorMessage
            });
#pragma warning restore CS8620 // 由于引用类型的可为 null 性差异，实参不能用于形参。
                }
            }
            return false;
        }

        /// <summary>
        /// 使用Attribute风格的校验过滤器
        /// </summary>
        /// <typeparam name="T">Endpoint的请求json数据类型</typeparam>
        /// <param name="validator">将需要验证的model放在endpoint delegate的第一个入参</param>
        /// <returns>如有校验错误则返回错误，否则下一步</returns>
        public static async ValueTask<object?> ValidateModelFilter<T>(EndpointFilterInvocationContext efiContext, EndpointFilterDelegate next)
        {
            var req = efiContext.GetArgument<T>(0);
            if (!Try(req, out var validationResults))
                return Results.ValidationProblem(validationResults);
            return await next(efiContext);
        }

        /// <summary>
        /// 使用FluentValidate风格的校验过滤器
        /// </summary>
        /// <typeparam name="T">Endpoint的请求json数据类型</typeparam>
        /// <param name="validator">将需要验证的model放在endpoint delegate的第一个入参</param>
        /// <returns>如有校验错误则返回错误，否则下一步</returns>
        public static Func<EndpointFilterInvocationContext, EndpointFilterDelegate, ValueTask<object?>> GetFluentValidateModelFilter<T>(IValidator<T> validator)
        {
            return async (efiContext, next) =>
            {
                var req = efiContext.GetArgument<T>(0);
                var validationResults = validator.Validate(req);
                if (!validationResults.IsValid)
                    return Results.ValidationProblem(validationResults.ToDictionary());
                return await next(efiContext);
            };
        }
    }
}

