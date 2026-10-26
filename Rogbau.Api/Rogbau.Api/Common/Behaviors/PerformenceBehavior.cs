using MediatR;
using System.Diagnostics;

namespace Rogbau.Api.Common.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        private readonly int _thresholdMs;

        public PerformanceBehavior(
            ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _thresholdMs = configuration.GetValue("Performance:ThresholdMs", 500);
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            var response = await next();

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > _thresholdMs)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning(
                    "🐢 Long Running Request: {Name} ({ElapsedMilliseconds}ms)",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
    }
}
