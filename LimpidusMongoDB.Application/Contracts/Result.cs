namespace LimpidusMongoDB.Application.Contracts
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }

        public Result(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static Result<T> Ok(string message = null, T data = default)
        {
            return new Result<T>(true, message, data);
        }

        public static Result<T> Error(string message)
        {
            return new Result<T>(false, message, default);
        }
    }

    public class Result : Result<object>
    {
        public Result(bool success, string message, object data) : base(success, message, data)
        {
        }

        public static Result Ok(string message = null, dynamic data = null)
        {
            return new Result(true, message, data);
        }

        public static Result Error(string message)
        {
            return new Result(false, message, default);
        }
    }
}