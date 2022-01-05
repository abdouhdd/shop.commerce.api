namespace shop.commerce.api.common
{
    using shop.commerce.api.common;
    using System;

    /// <summary>
    /// the base exception Class for all Application Exceptions
    /// </summary>
    [Serializable]
    public class AppException : Exception
    {
        /// <summary>
        /// the result instant associated with this exception
        /// </summary>
        public Result Result { get; }

        /// <summary>
        /// create an instant of <see cref="AppException"/>
        /// </summary>
        /// <param name="message">the message associated with the exceptions</param>
        public AppException(string message, string code)
            : base(message) 
        { 
            Result = Result.Failed(message, code); 
        }

        /// <summary>
        /// create an instant of <see cref="AppException"/>
        /// </summary>
        /// <param name="message">the message associated with the exceptions</param>
        /// <param name="code">the message code</param>
        /// <param name="innerException">the inner exception</param>
        public AppException(string message, string code, Exception innerException)
            : base(message, innerException)
        {
            Result = Result.Failed(message, code);
        }

        /// <summary>
        /// Occurs when an exception is serialized to create an exception state object that contains serialized data about the exception.
        /// </summary>
        /// <param name="serializationInfo">the serializationInfo</param>
        /// <param name="streamingContext">the streamingContext</param>
        protected AppException(
            System.Runtime.Serialization.SerializationInfo serializationInfo,
            System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) { }
    }
}
