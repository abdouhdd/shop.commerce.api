using System;

namespace shop.commerce.api.domain.Models
{
    public class MyResult<TResult>
    {
        public TResult Data { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public bool Success { get; set; }

        public static MyResult<TResult> ResultError(TResult data, Exception exception)
        {
            return new MyResult<TResult>
            {
                Data = data,
                Message = exception.Message,
                Code = MyResultCode.ErrorCode,
                Success = false
            };
        }

        public static MyResult<TResult> ResultError(TResult data, string message, string code)
        {
            return new MyResult<TResult>
            {
                Data = data,
                Message = message,
                Code = code,
                Success = false
            };
        }

        public static MyResult<TResult> ResultSuccess(TResult data, string message = "Operacion_successed", string code = MyResultCode.SuccessCode)
        {
            return new MyResult<TResult>
            {
                Data = data,
                Message = message,
                Code = code,
                Success = true
            };
        }
    }
}
