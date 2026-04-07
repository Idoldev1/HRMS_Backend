using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRMS.API.Validations
{
    public class ValidationApiBehaviorOptionsSetup : IConfigureOptions<ApiBehaviorOptions>
    {
        public void Configure(ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Validation");

                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "Invalid value."
                                : error.ErrorMessage)
                            .ToArray());

                logger.LogInformation(
                    "Validation failed. TraceId: {TraceId}, Path: {Path}, Method: {Method}, Errors: {@Errors}",
                    context.HttpContext.TraceIdentifier,
                    context.HttpContext.Request.Path,
                    context.HttpContext.Request.Method,
                    errors);

                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed.",
                    Detail = "One or more validation errors occurred."
                };

                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problemDetails);
            };
        }
    }
}
