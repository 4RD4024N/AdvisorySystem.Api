namespace AdvisorySystem.Api.Middleware;

public class FileSizeValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;
    private readonly string[] _allowedContentTypes;

    public FileSizeValidationMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        // Yeni: 10MB limit
        _maxFileSize = config.GetValue<long>("Storage:MaxFileSize", 10485760); // Default 10MB
        
        // Ýzin verilen dosya uzantýlarý
        _allowedExtensions = new[] { ".pdf", ".docx", ".pptx" };
        
        // Ýzin verilen content type'lar
        _allowedContentTypes = new[]
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" 
        };
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
                        error = "File size exceeds limit",
                        message = $"File '{file.FileName}' is {file.Length / 1024 / 1024.0:F2}MB. Maximum allowed size is {_maxFileSize / 1024 / 1024}MB.",
                        maxSizeMB = _maxFileSize / 1024 / 1024,
                        fileSizeMB = file.Length / 1024 / 1024.0
                    });
                    return;
                }

                // Dosya uzantýsý kontrolü
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Invalid file type",
                        message = $"File type '{extension}' is not allowed. Only PDF, DOCX, and PPTX files are accepted.",
                        allowedTypes = _allowedExtensions,
                        providedType = extension
                    });
                    return;
                }

                // Content type kontrolü (ek güvenlik)
                if (!_allowedContentTypes.Contains(file.ContentType))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Invalid content type",
                        message = $"Content type '{file.ContentType}' is not allowed.",
                        allowedTypes = new[] { "PDF", "DOCX", "PPTX" }
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
