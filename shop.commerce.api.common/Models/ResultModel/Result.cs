namespace shop.commerce.api.common
{
    using shop.commerce.api.common.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// the base result class
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public partial class Result
    {
        private string _logTraceCode;


        /// <summary>
        /// construct a <see cref="Result"/> instant with default values
        /// </summary>
        public Result()
        {
            Status = ResultStatus.Succeed;
            Message = "Operation Succeeded";
            Errors = new HashSet<ResultError>();
            Code = ResultCode.OperationSucceeded;
        }

        //public TResult Data { get; set; }
        //public string Message { get; set; }
        //public string Code { get; set; }
        //public bool Success { get; set; }

        /// <summary>
        /// get the message associated with this result, in case of an ResultError
        /// this property will hold the ResultError description
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// a code that represent a message, used to identify the ResultError
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// the Status of the result
        /// </summary>
        public ResultStatus Status { get; set; }

        /// <summary>
        /// a unique log trace code used to trace the result in logs
        /// </summary>
        public string LogTraceCode
        {
            get
            {
                if (Status == ResultStatus.Failed && !_logTraceCode.IsValid())
                    _logTraceCode = Generator.GenerateLogTraceErrorCode();

                return _logTraceCode;
            }
            set => _logTraceCode = value;
        }

        /// <summary>
        /// this list of errors associated with the failure of the operation
        /// </summary>
        public ICollection<ResultError> Errors { get; set; }
    }

    /// <summary>
    /// the partial part of <see cref="Result"/>
    /// </summary>
    public partial class Result
    {
        /// <summary>
        /// check if the operation associated with this result has produce a value
        /// </summary>
        public virtual bool HasValue() => false;

        /// <summary>
        /// is this Result has raised an ResultError
        /// </summary>
        public bool HasErrors() => !(Errors is null) && Errors.Any();

        /// <summary>
        /// if there is no ResultErrors and the status of the result is Success this will be true
        /// </summary>
        public bool Success => Status == ResultStatus.Succeed && !HasErrors();

        /// <summary>
        /// get the string reApplication of the object
        /// </summary>
        /// <returns>the string value</returns>
        public override string ToString()
            => $"Status: {Status}, HasValue: {HasValue()}";

        /// <summary>
        /// extract list of errors from the given exception by looking inside the innerException
        /// </summary>
        /// <param name="exception">the exception to </param>
        /// <returns>an array of <see cref="ResultError"/></returns>
        public static ResultError[] GetErrorsFromException(Exception exception)
        {
            var errors = new HashSet<ResultError>();

            if (exception is null)
                return errors.ToArray();

            foreach (var error in exception.FromHierarchy(ex => ex.InnerException)
                .Select(ex => ResultError.MapFromException(exception)))
            {
                errors.Add(error);
            }

            return errors.ToArray();
        }

        /// <summary>
        /// create an instant of <see cref="Result"/> form the given result
        /// </summary>
        /// <param name="result">the result to clone</param>
        /// <returns>an instant of <see cref="Result"/></returns>
        public static Result Clone(Result result)
            => new Result()
            {
                Code = result.Code,
                Errors = result.Errors,
                Status = result.Status,
                Message = result.Message,
            };

        /// <summary>
        /// create a new Success <see cref="Result"/> with a message of "Operation Succeeded", and
        /// a <see cref="Axiobat.string"/> of <see cref="ResultCode.OperationSucceeded"/>
        /// </summary>
        /// <returns>an instant of <see cref="Result"/></returns>
        public static Result ResultSuccess() => new Result();

        /// <summary>
        /// create a new success <see cref="Result"/> with a status of <see cref="ResultStatus.Succeed"/>
        /// and a <see cref="string"/> of <see cref="ResultCode.OperationSucceeded"/>
        /// </summary>
        /// <param name="message">the message associated with the <see cref="Result"/> instant</param>
        /// <returns>the <see cref="Result"/> instant</returns>
        public static Result ResultSuccess(string message)
            => new Result()
            {
                Message = message,
                Status = ResultStatus.Succeed,
                Code = ResultCode.OperationSucceeded,
            };

        /// <summary>
        /// create a new success <see cref="Result"/> with a status of <see cref="ResultStatus.Succeed"/>
        /// </summary>
        /// <param name="message">the message associated with the <see cref="Result"/> instant</param>
        /// <param name="string">the <see cref="Axiobat.string"/> associated with the result</param>
        /// <returns>the <see cref="Result"/> instant</returns>
        public static Result ResultSuccess(string message, string code)
            => new Result()
            {
                Code = code,
                Message = message,
                Status = ResultStatus.Succeed,
            };

        /// <summary>
        /// create a new Success <see cref="Result{TResult}"/> with a status of <see cref="ResultStatus.Succeed"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result value</typeparam>
        /// <param name="value">the result value</param>
        /// <param name="message">the message associated with the <see cref="Result{TResult}"/> instant</param>
        /// <param name="code">the code of the result, defaulted to <see cref="ResultCode.OperationSucceeded"/></param>
        /// <returns>the <see cref="Result{TResult}"/> instant</returns>
        public static Result<TResult> ResultSuccess<TResult>(TResult value, string message = "Operation Succeeded", string code = ResultCode.OperationSucceeded)
            => new Result<TResult>()
            {
                Code = code,
                Data = value,
                Message = message,
                Status = ResultStatus.Succeed,
            };

        /// <summary>
        /// create a new Success <see cref="ListResult{TResult}"/> with a status of <see cref="ResultStatus.Succeed"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result value</typeparam>
        /// <param name="value">the result value</param>
        /// <param name="message">the message associated with the <see cref="ListResult{TResult}"/> instant</param>
        /// <param name="code">the code of the result, defaulted to <see cref="ResultCode.OperationSucceeded"/></param>
        /// <returns>the <see cref="ListResult{TResult}"/> instant</returns>
        public static ListResult<TResult> ListSuccess<TResult>(TResult[] value, string message = "Operation Succeeded", string code = ResultCode.OperationSucceeded)
            => new ListResult<TResult>()
            {
                Code = code,
                Data = value,
                Message = message,
                Status = ResultStatus.Succeed,
            };

        /// <summary>
        /// create a new Success result with message "Operation Succeeded" and message code of "1"
        /// </summary>
        /// <param name="value">the result value</param>
        /// <returns>the <see cref="ListResult{TResult}"/> instant</returns>
        public static PagedResult<TResult> PagedSuccess<TResult>(TResult[] value, int currentPage, int pageSize, int rowsCount)
            => new PagedResult<TResult>()
            {
                Status = ResultStatus.Succeed,
                Message = "Operation Succeeded",
                Code = ResultCode.OperationSucceeded,
                Data = value,
                PageIndex = currentPage,
                PageSize = pageSize,
                TotalRows = rowsCount,
            };

        /// <summary>
        /// create a new Success result with message "Operation Succeeded" and message code of "1"
        /// </summary>
        /// <param name="value">the result value</param>
        /// <returns>the <see cref="ListResult{TResult}"/> instant</returns>
        public static PagedResult<TOut> PagedSuccess<TOut, TResult>(TOut[] value, PagedResult<TResult> pagedResult)
            => new PagedResult<TOut>()
            {
                Status = ResultStatus.Succeed,
                Message = "Operation Succeeded",
                Code = ResultCode.OperationSucceeded,
                Data = value,
                PageIndex = pagedResult.PageIndex,
                PageSize = pagedResult.PageSize,
                TotalRows = pagedResult.TotalRows,
            };

        /// <summary>
        /// create a Failed <see cref="Result"/> with a status of <see cref="ResultStatus.Failed"/>
        /// </summary>
        /// <param name="message">the message associated with the <see cref="Result"/> instant, defaulted to "Operation Failed"</param>
        /// <param name="code">the <see cref="Axiobat.string"/>, defaulted to <see cref="ResultCode.OperationFailed"/></param>
        /// <param name="exception">the <see cref="Exception"/> associated with the <see cref="Result"/> instant</param>
        /// <returns></returns>
        public static Result Failed(string message = "Operation Failed", string code = ResultCode.OperationFailed, Exception exception = null)
            => Failed(message, code, GetErrorsFromException(exception));

        /// <summary>
        /// create a Failed <see cref="Result"/> with a status of <see cref="ResultStatus.Failed"/>
        /// </summary>
        /// <param name="message">the message associated with the <see cref="Result"/> instant, defaulted to "Operation Failed"</param>
        /// <param name="code">the <see cref="Axiobat.string"/>, defaulted to <see cref="ResultCode.OperationFailed"/></param>
        /// <param name="errors">the <see cref="ResultError"/> associated with the <see cref="Result"/> instant</param>
        /// <returns></returns>
        public static Result Failed(string message = "Operation Failed", string code = ResultCode.OperationFailed, params ResultError[] errors)
            => new Result()
            {
                Code = code,
                Errors = errors,
                Message = message,
                Status = ResultStatus.Failed,
                LogTraceCode = Generator.GenerateLogTraceErrorCode(),
            };

        /// <summary>
        /// create a Failed <see cref="Result"/> with a status of <see cref="ResultStatus.Failed"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result value</typeparam>
        /// <param name="message">the message associated with the <see cref="Result"/> instant defaulted to "Operation Failed"</param>
        /// <param name="code">the code the result, defaulted to <see cref="ResultCode.OperationFailed"/></param>
        /// <param name="exception">the <see cref="Exception"/> associated with the <see cref="Result"/> instant, if any defaulted to <see cref="null"/></param>
        /// <returns>an instant of <see cref="Result{TResult}"/></returns>
        public static Result<TResult> Failed<TResult>(string message = "Operation Failed", string code = ResultCode.OperationFailed, Exception exception = null)
            => Failed<TResult>(message, code, GetErrorsFromException(exception));

        /// <summary>
        /// create a Failed <see cref="Result"/> with a status of <see cref="ResultStatus.Failed"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result value</typeparam>
        /// <param name="message">the message associated with the <see cref="Result"/> instant defaulted to "Operation Failed"</param>
        /// <param name="code">the code the result, defaulted to <see cref="ResultCode.OperationFailed"/></param>
        /// <param name="errors">the <see cref="ResultError"/> associated with the <see cref="Result"/> instant, if any defaulted to <see cref="null"/></param>
        /// <returns>an instant of <see cref="Result{TResult}"/></returns>
        public static Result<TResult> Failed<TResult>(string message = "Operation Failed", string code = ResultCode.OperationFailed, params ResultError[] errors)
            => new Result<TResult>()
            {
                Code = code,
                Data = default,
                Errors = errors,
                Message = message,
                Status = ResultStatus.Failed,
                LogTraceCode = Generator.GenerateLogTraceErrorCode(),
            };

        /// <summary>
        /// create a Failed <see cref="ListResult{TResult}"/> with a status of <see cref="ResultStatus.Failed"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result value</typeparam>
        /// <param name="message">the message associated with the <see cref="ListResult{TResult}"/> instant defaulted to "Operation Failed"</param>
        /// <param name="code">the code the result, defaulted to <see cref="ResultCode.OperationFailed"/></param>
        /// <param name="exception">the exception associated with the <see cref="Result"/> instant, if any defaulted to <see cref="null"/></param>
        /// <returns>an instant of <see cref="ListResult{TResult}"/></returns>
        public static ListResult<TResult> ListFailed<TResult>(string message = "Operation Failed", string code = ResultCode.OperationFailed, Exception exception = null)
            => ListFailed<TResult>(message, code, GetErrorsFromException(exception));

        /// <summary>
        /// create a Failed <see cref="ListResult{TResult}"/> with a status of <see cref="ResultStatus.Failed"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result value</typeparam>
        /// <param name="message">the message associated with the <see cref="ListResult{TResult}"/> instant defaulted to "Operation Failed"</param>
        /// <param name="code">the code the result, defaulted to <see cref="ResultCode.OperationFailed"/></param>
        /// <param name="errors">the <see cref="ResultError"/> associated with the <see cref="Result"/> instant, if any defaulted to <see cref="null"/></param>
        /// <returns>an instant of <see cref="ListResult{TResult}"/></returns>
        public static ListResult<TResult> ListFailed<TResult>(string message = "Operation Failed", string code = ResultCode.OperationFailed, params ResultError[] errors)
            => new ListResult<TResult>()
            {
                Code = code,
                Data = default,
                Errors = errors,
                Message = message,
                Status = ResultStatus.Failed,
                LogTraceCode = Generator.GenerateLogTraceErrorCode(),
            };

        /// <summary>
        /// create a Failed <see cref="PagedResult{TResult}"/> with a status of <see cref="ResultStatus.Failed"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result value</typeparam>
        /// <param name="message">the message associated with the <see cref="PagedResult{TResult}"/> instant defaulted to "Operation Failed"</param>
        /// <param name="code">the code the result, defaulted to <see cref="ResultCode.OperationFailed"/></param>
        /// <param name="exception">the exception associated with the <see cref="Result"/> instant, if any defaulted to <see cref="null"/></param>
        /// <returns>an instant of <see cref="PagedResult{TResult}"/></returns>
        public static PagedResult<TResult> PagedFailed<TResult>(string message = "Operation Failed", string code = ResultCode.OperationFailed, Exception exception = null)
            => PagedFailed<TResult>(message, code, GetErrorsFromException(exception));

        /// <summary>
        /// create a Failed <see cref="PagedResult{TResult}"/> with a status of <see cref="ResultStatus.Failed"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result value</typeparam>
        /// <param name="message">the message associated with the <see cref="PagedResult{TResult}"/> instant defaulted to "Operation Failed"</param>
        /// <param name="code">the code the result, defaulted to <see cref="ResultCode.OperationFailed"/></param>
        /// <param name="errors">the <see cref="ResultError"/> associated with the <see cref="Result"/> instant, if any defaulted to <see cref="null"/></param>
        /// <returns>an instant of <see cref="PagedResult{TResult}"/></returns>
        public static PagedResult<TResult> PagedFailed<TResult>(string message = "Operation Failed", string code = ResultCode.OperationFailed, params ResultError[] errors)
            => new PagedResult<TResult>()
            {
                Code = code,
                Data = default,
                Errors = errors,
                Message = message,
                Status = ResultStatus.Failed,
                LogTraceCode = Generator.GenerateLogTraceErrorCode(),
                PageIndex = 1,
                PageSize = 10,
                TotalRows = 0,
            };

        /// <summary>
        /// build a <see cref="Result"/> form the given result instant
        /// </summary>
        /// <param name="result">the type of the result</param>
        /// <returns>the result instant</returns>
        public static Result<TOut> From<TOut>(Result result)
            => new Result<TOut>()
            {
                Data = default,
                Code = result.Code,
                Status = result.Status,
                Errors = result.Errors,
                Message = result.Message,
                LogTraceCode = result.LogTraceCode,
            };

        /// <summary>
        /// build a <see cref="Result"/> instant from the given <see cref="Result{TResult}"/>
        /// </summary>
        /// <typeparam name="TResult">the type of th result</typeparam>
        /// <param name="result">the <see cref="Result{TResult}"/> instant</param>
        /// <returns>the generated <see cref="Result"/></returns>
        public static Result From<TResult>(Result<TResult> result)
            => new Result()
            {
                Code = result.Code,
                Errors = result.Errors,
                Status = result.Status,
                Message = result.Message,
                LogTraceCode = result.LogTraceCode,
            };

        /// <summary>
        /// build a <see cref="Result"/> form the given result instant
        /// </summary>
        /// <param name="result">the type of the result</param>
        /// <returns>the result instant</returns>
        public static Result<TOut> From<TOut, TResult>(Result<TResult> result)
            => new Result<TOut>()
            {
                Data = default,
                Code = result.Code,
                Status = result.Status,
                Errors = result.Errors,
                Message = result.Message,
                LogTraceCode = result.LogTraceCode,
            };

        /// <summary>
        /// build a <see cref="Result"/> form the given result instant
        /// </summary>
        /// <param name="result">the type of the result</param>
        /// <returns>the result instant</returns>
        public static ListResult<TOut> ListFrom<TOut, TResult>(Result<TResult> result)
            => new ListResult<TOut>()
            {
                Data = default,
                Code = result.Code,
                Status = result.Status,
                Errors = result.Errors,
                Message = result.Message,
                LogTraceCode = result.LogTraceCode,
            };

        /// <summary>
        /// build a <see cref="Result{TResult}"/> form the given <see cref="ListResult{TResult}"/>
        /// </summary>
        /// <param name="result">the <see cref="ListResult{TResult}"/> instant</param>
        /// <returns>the <see cref="Result{TResult}"/> instant</returns>
        public static ListResult<TOut> ListFrom<TOut, TResult>(ListResult<TResult> result)
            => new ListResult<TOut>()
            {
                Data = default,
                Code = result.Code,
                Status = result.Status,
                Errors = result.Errors,
                Message = result.Message,
                LogTraceCode = result.LogTraceCode,
            };

        /// <summary>
        /// build a <see cref="Result{TResult}"/> form the given <see cref="ListResult{TResult}"/>
        /// </summary>
        /// <param name="result">the <see cref="ListResult{TResult}"/> instant</param>
        /// <returns>the <see cref="Result{TResult}"/> instant</returns>
        public static PagedResult<TOut> ListFrom<TOut, TResult>(PagedResult<TResult> result)
            => new PagedResult<TOut>()
            {
                Data = default,
                Code = result.Code,
                Status = result.Status,
                Errors = result.Errors,
                Message = result.Message,
                LogTraceCode = result.LogTraceCode,
            };

        /// <summary>
        /// build a <see cref="Result{TResult}"/> form the given <see cref="ListResult{TResult}"/>
        /// </summary>
        /// <param name="result">the <see cref="ListResult{TResult}"/> instant</param>
        /// <returns>the <see cref="Result{TResult}"/> instant</returns>
        public static Result<TOut> From<TOut, TResult>(ListResult<TResult> result)
            => new Result<TOut>()
            {
                Data = default,
                Code = result.Code,
                Status = result.Status,
                Errors = result.Errors,
                Message = result.Message,
                LogTraceCode = result.LogTraceCode,
            };

        /// <summary>
        /// build a <see cref="Result{TResult}"/> form the given <see cref="PagedResult{TResult}"/>
        /// </summary>
        /// <param name="result">the <see cref="PagedResult{TResult}"/> instant</param>
        /// <returns>the <see cref="Result{TResult}"/> instant</returns>
        public static Result<TOut> From<TOut, TResult>(PagedResult<TResult> result)
            => new Result<TOut>()
            {
                Data = default,
                Code = result.Code,
                Status = result.Status,
                Errors = result.Errors,
                Message = result.Message,
                LogTraceCode = result.LogTraceCode,
            };
    }

    /// <summary>
    /// the result class with a value property
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class Result<TResult> : Result
    {
        /// <summary>
        /// construct a <see cref="Result{TResult}"/> instant with default values
        /// </summary>
        public Result() : base() => Data = default;

        /// <summary>
        /// the result of the operation
        /// </summary>
        public TResult Data { get; set; }

        /// <summary>
        /// check if the operation associated with this result has produce a value
        /// </summary>
        public override bool HasValue()
            => !EqualityComparer<TResult>.Default.Equals(Data, default);

        /// <summary>
        /// get the string reApplication of the object
        /// </summary>
        /// <returns>the string value</returns>
        public override string ToString()
            => $"{base.ToString()}, Value: {Data}";

        /// <summary>
        /// create an instant of <see cref="Result{TResult}"/> form the given result
        /// </summary>
        /// <param name="result">the result to clone</param>
        /// <returns>an instant on <see cref="Result{TResult}"/></returns>
        public static new Result<TResult> Clone(Result result)
            => new Result<TResult>()
            {
                Status = result.Status,
                Errors = result.Errors,
                Message = result.Message,
                Code = result.Code,
                Data = default
            };

        #region Operators overrides

        public static implicit operator TResult(Result<TResult> result)
            => result is null || !result.HasValue() ? (default) : result.Data;

        public static implicit operator Result<TResult>(TResult result)
            => ResultSuccess(result);

        #endregion
    }
}
