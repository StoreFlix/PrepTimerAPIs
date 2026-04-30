namespace PrepTimerAPIs.Services
{
    public interface IAppLogger
    {
        Task LogInfo(
            string apiName,
            string message,
            object request = null,
            object response = null);

        Task LogError(
            string apiName,
            Exception ex,
            string message = null,
            object request = null,
            object response = null);
    }
}
