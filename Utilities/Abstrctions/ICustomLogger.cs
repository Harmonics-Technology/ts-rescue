using System;

namespace TimesheetBE.Utilities.Abstrctions
{
    public interface ICustomLogger<T>
    {
         void Info(string EventId,Exception ex);
         void Debug(string EventId,Exception ex);
         StandardResponse<N> Error<N>(string EventId,Exception ex);
         void Trace(string EventId,Exception ex);
         void Critical(string EventId,Exception ex);
         void Warning(string EventId, Exception ex);
         string GetMethodName();
    }
}