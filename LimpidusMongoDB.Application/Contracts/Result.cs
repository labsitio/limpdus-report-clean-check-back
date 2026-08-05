using System.Text.Json.Serialization;

namespace LimpidusMongoDB.Application.Contracts
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Código estável de erro para o cliente decidir o tratamento/tradução.
        /// Omitido do JSON quando nulo para não alterar contratos existentes.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Code { get; set; }

        public Result(bool success, string message, T data, string code = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Code = code;
        }

        public static Result<T> Ok(string message = null, T data = default)
        {
            return new Result<T>(true, message, data);
        }

        public static Result<T> Error(string message, string code = null)
        {
            return new Result<T>(false, message, default, code);
        }
    }

    public class Result : Result<object>
    {
        public Result(bool success, string message, object data, string code = null)
            : base(success, message, data, code)
        {
        }

        public new static Result Ok(string message = null, dynamic data = null)
        {
            return new Result(true, message, data);
        }

        public new static Result Error(string message, string code = null)
        {
            return new Result(false, message, default, code);
        }
    }
}
