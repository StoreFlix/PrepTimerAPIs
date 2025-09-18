namespace PrepTimerAPIs.Dtos
{
    public class ResponseResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ResponseResult Success(string message) =>
            new ResponseResult { IsSuccess = true, Message = message };

        public static ResponseResult Failure(string message) =>
            new ResponseResult { IsSuccess = false, Message = message };
    }
}
