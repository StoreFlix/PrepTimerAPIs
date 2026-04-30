namespace PrepTimerAPIs.Model
{
    public class AppLog
    {
        public long Id { get; set; }
        public string ApiName { get; set; }
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
