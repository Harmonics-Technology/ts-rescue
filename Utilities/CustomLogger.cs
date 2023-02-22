using System;
using Microsoft.Extensions.Logging;
using TimesheetBE.Utilities.Abstrctions;

namespace TimesheetBE.Utilities
{
    public class CustomLogger<T> : ICustomLogger<T>
    {
        private readonly ILogger<T> _logger;

        public CustomLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Info(string EventId,Exception ex)
        {
            _logger.LogInformation(EventId, ex);
        }

        public void Debug(string EventId,Exception ex)
        {
            _logger.LogDebug(EventId, ex);
        }

        public StandardResponse<N> Error<N>(string EventId,Exception ex)
        {
            _logger.LogError(EventId, ex);
            return StandardResponse<N>.Error(ex.Message);
        }

        public void Trace(string EventId,Exception ex)
        {
            _logger.LogTrace(EventId, ex);
        }

        public void Critical(string EventId,Exception ex)
        {
            _logger.LogCritical(EventId , ex);
        }

        public void Warning(string EventId, Exception ex)
        {
            _logger.LogWarning(EventId, ex);
        }

        public string GetMethodName()
        {
            return System.Reflection.MethodBase.GetCurrentMethod().Name;
        }
    }
}