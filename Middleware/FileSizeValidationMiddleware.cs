namespace AdvisorySystem.Api.Middleware;

public class FileSizeValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _maxFileSize;

    public FileSizeValidationMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _maxFileSize = config.GetValue<long>("Storage:MaxFileSize", 104857600); // Default 100MB
    }

    public async Task InvokeAsync(HttpContext context)
    {
   if (context.Request.HasFormContentType && context.Request.Form.Files.Any())
  {
    foreach (var file in context.Request.Form.Files)
   {
             if (file.Length > _maxFileSize)
                {
                  context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
           await context.Response.WriteAsJsonAsync(new
        {
  error = $"File {file.FileName} exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB"
            });
          return;
 }
   }
        }

 await _next(context);
    }
}

public static class FileSizeValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseFileSizeValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FileSizeValidationMiddleware>();
    }
}
