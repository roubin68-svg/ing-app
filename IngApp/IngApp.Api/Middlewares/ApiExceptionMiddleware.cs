using System.Net;
using System.Text.Json;
using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Models;

namespace IngApp.Api.Middlewares
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                await WriteError(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (ValidationException ex)
            {
                var message = string.Join(" | ", ex.Errors);
                await WriteError(context, HttpStatusCode.BadRequest, message);
            }
            catch (AppException ex)
            {
                await WriteError(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                await WriteError(context, HttpStatusCode.Unauthorized, "دسترسی غیرمجاز");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                await WriteError(
                    context,
                    HttpStatusCode.InternalServerError,
                    "خطایی در سرور رخ داده است. لطفاً بعداً تلاش کنید."
                );
            }
        }

        private async Task WriteError(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var result = ApiResult.Fail(message);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
