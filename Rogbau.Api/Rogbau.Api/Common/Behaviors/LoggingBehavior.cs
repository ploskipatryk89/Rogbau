using MediatR;
using System.Diagnostics;
using System.Text.Json;

namespace Rogbau.Api.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestId = Guid.NewGuid().ToString("N")[..8];

            // Logowanie wejścia
            _logger.LogInformation(
                "[{RequestId}] ➡️ START {RequestName}",
                requestId, requestName);

            // W trybie Debug - loguj pełny obiekt
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                try
                {
                    var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        MaxDepth = 3
                    });
                    _logger.LogDebug("[{RequestId}] Request: {Request}", requestId, requestJson);
                }
                catch
                {
                    _logger.LogDebug("[{RequestId}] Request: (nie można zserializować)", requestId);
                }
            }

            // Zmierz czas wykonania
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();

                stopwatch.Stop();

                // Logowanie sukcesu
                _logger.LogInformation(
                    "[{RequestId}] ✅ END {RequestName} ({ElapsedMs}ms)",
                    requestId, requestName, stopwatch.ElapsedMilliseconds);

                // Ostrzeżenie jeśli za długo
                if (stopwatch.ElapsedMilliseconds > 500)
                {
                    _logger.LogWarning(
                        "[{RequestId}] ⚠️ SLOW {RequestName} ({ElapsedMs}ms)",
                        requestId, requestName, stopwatch.ElapsedMilliseconds);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Logowanie błędu
                _logger.LogError(
                    ex,
                    "[{RequestId}] ❌ FAIL {RequestName} ({ElapsedMs}ms) - {ErrorMessage}",
                    requestId, requestName, stopwatch.ElapsedMilliseconds, ex.Message);

                throw;
            }
        }
    }
}
