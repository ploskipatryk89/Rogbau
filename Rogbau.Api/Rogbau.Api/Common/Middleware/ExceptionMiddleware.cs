using Microsoft.AspNetCore.Http.HttpResults;
using Rogbau.Api.Common.Exceptions;
using Rogbau.Api.Common.Exceptions.Equipments;
using System.Net;
using System.Text.Json;

namespace Rogbau.Api.Common.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Nieobsłużony wyjątek: {Message}", exception.Message);

            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                // Własna walidacja lub FluentValidation
                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Type = "ValidationError";
                    response.Title = "Błędy walidacji";
                    response.Status = 400;
                    response.Errors = validationEx.Errors;
                    break;

                case FluentValidation.ValidationException fluentValidationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Type = "ValidationError";
                    response.Title = "Błędy walidacji";
                    response.Status = 400;
                    // Mapowanie błędów z FluentValidation na słownik IDictionary<string, string[]>
                    response.Errors = fluentValidationEx.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.ErrorMessage).ToArray()
                        );
                    break;

                // Nie znaleziono - 404 Not Found
                case NotFoundException notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Type = "NotFound";
                    response.Title = notFoundEx.Message;
                    response.Status = 404;
                    break;

                // Nieprawidłowy argument - 400 Bad Request
                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Type = "BadRequest";
                    response.Title = argEx.Message;
                    response.Status = 400;
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Type = "NotFound";
                    response.Title = keyNotFoundEx.Message;
                    response.Status = 404;
                    break;

                // Brak autoryzacji - 401 Unauthorized
                case UnauthorizedAccessException unauthorizedEx:
                    var authenticated = context.User.Identity?.IsAuthenticated == true;
                    context.Response.StatusCode = authenticated ? 403 : 401;
                    response.Type = authenticated ? "Forbidden" : "Unauthorized";
                    response.Title = authenticated ? "Brak uprawnień" : "Brak autoryzacji";
                    response.Detail = unauthorizedEx.Message;
                    response.Status = context.Response.StatusCode;
                    break;

                // Operacja niedozwolona - 403 Forbidden
                case InvalidOperationException invalidOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Type = "BadRequest";
                    response.Title = invalidOpEx.Message;
                    response.Status = 400;
                    break;

                case ConflictException conflictEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Type = "Conflict";
                    response.Title = conflictEx.Message;
                    response.Status = 409;
                    break;

                case AlreadyExistsException existEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Type = "Conflict";
                    response.Title = existEx.Message;
                    response.Status = 409;
                    break;



                // Wszystko inne - 500 Internal Server Error
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Type = "InternalError";
                    response.Title = _environment.IsDevelopment()
                        ? exception.Message
                        : "Wystąpił błąd serwera";
                    response.Status = 500;

                    if (_environment.IsDevelopment())
                    {
                        response.Detail = exception.StackTrace;
                    }
                    break;
            }

            response.TraceId = context.TraceIdentifier;

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await context.Response.WriteAsync(json);
        }
    }

    public class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Detail { get; set; }
        public string? TraceId { get; set; }
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}
