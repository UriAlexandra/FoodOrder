using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Api;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.FirstOrDefault(a => a?.GetType() == typeof(T)) as T;
        if (argument == null) return await next(context);

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(argument, new ValidationContext(argument), validationResults, true);

        if (!isValid)
        {
            var errors = validationResults
                .GroupBy(e => e.MemberNames.FirstOrDefault() ?? string.Empty, e => e.ErrorMessage ?? "Érvénytelen érték.")
                .ToDictionary(g => g.Key, g => g.ToArray());
            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}