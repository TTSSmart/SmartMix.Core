using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace SmartMix.Core.API.Middleware;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;
    
    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;
            
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            var validator = _serviceProvider.GetService(validatorType) as IValidator;
            
            if (validator != null)
            {
                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext);
                
                if (!result.IsValid)
                {
                    var problemDetails = new ValidationProblemDetails(result.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation Failed",
                        Detail = "One or more validation errors occurred",
                        Instance = context.HttpContext.Request.Path
                    };
                    
                    context.Result = new BadRequestObjectResult(problemDetails);
                    return;
                }
            }
        }
        
        await next();
    }
}

public static class ValidationFilterExtensions
{
    public static IServiceCollection AddValidationFilter(this IServiceCollection services)
    {
        services.AddScoped<ValidationFilter>();
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });
        return services;
    }
}