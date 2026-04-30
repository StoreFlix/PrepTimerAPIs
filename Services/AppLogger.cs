using PrepTimerAPIs.Model;
using PrepTimerAPIs.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrepTimerAPIs.Services
{
    public class AppLogger : IAppLogger
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AppLogger(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task LogInfo(
            string apiName,
            string message,
            object request = null,
            object response = null)
        {
            return SafeLog(() =>
                LogToDb("INFO", apiName, message, null, request, response));
        }

        public Task LogError(
            string apiName,
            Exception ex,
            string message = null,
            object request = null,
            object response = null)
        {
            return SafeLog(() =>
                LogToDb("ERROR", apiName, message, ex?.ToString(), request, response));
        }

        private async Task SafeLog(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch
            {
                // swallow all logging exceptions
            }
        }

        private async Task LogToDb(
            string level,
            string apiName,
            string message,
            string exception,
            object request,
            object response)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StoreLynkDbProd01Context>();

            db.AppLog.Add(new AppLog
            {
                ApiName = apiName,
                LogLevel = level,
                Message = message,
                Exception = exception,
                RequestPayload = SafeSerialize(request),
                ResponsePayload = SafeSerialize(response),
                CreatedOn = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        private static string SafeSerialize(object obj)
        {
            if (obj == null) return null;

            try
            {
                return JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                });
            }
            catch
            {
                return "Serialization failed";
            }
        }

    }

}
