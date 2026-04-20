using Authen.Application.Common;
using System.Text.Json;

namespace AuthenService.WebApi.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = ex switch
            {
                ArgumentNullException => (400, "Dữ liệu đầu vào không hợp lệ."),
                UnauthorizedAccessException => (401, "Bạn không có quyền thực hiện hành động này."),
                KeyNotFoundException => (404, "Không tìm thấy tài nguyên."),
                InvalidOperationException => (409, ex.Message),
                _ => (500, "Đã có lỗi xảy ra. Vui lòng thử lại sau.")
            };

            context.Response.StatusCode = statusCode;

            var result = ServiceResult<object>.Fail(message, statusCode);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}

