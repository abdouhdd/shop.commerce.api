namespace shop.commerce.api.common
{
    /// <summary>
    /// the result error details
    /// </summary>
    public partial class ResultError
    {
        /// <summary>
        /// create an instant of <see cref="ResultError"/>
        /// </summary>
        /// <param name="message">the message that describe the error</param>
        /// <param name="code">the code associated with the error</param>
        /// <param name="source">the source of the error</param>
        /// <param name="type">the type of the error</param>
        public ResultError(string message, string code, string source, string type)
        {
            Message = message;
            Code = code;
            Source = source;
            Type = type;
        }

        /// <summary>
        /// the error message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// the code associated with the exception
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// the source of the error
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// type of the error
        /// </summary>
        public string Type { get; }
    }

    /// <summary>
    /// partial part for <see cref="ResultError"/>
    /// </summary>
    public partial class ResultError : System.IEquatable<ResultError>
    {
        /// <summary>
        /// check if the given ResultError equals the current instant,
        /// two <see cref="ResultError"/> are equals if they have the same <see cref="Value"/> and <see cref="Name"/>
        /// </summary>
        /// <param name="other">the <see cref="ResultError"/> to compare</param>
        /// <returns>true if equals false if not</returns>
        public bool Equals(ResultError other)
        {
            if (other is null) return false;

            if (!other.Code.IsValid() && Code.IsValid()) return false;
            if (!other.Message.IsValid() && Message.IsValid()) return false;
            if (!other.Type.IsValid() && Type.IsValid()) return false;
            if (!other.Source.IsValid() && Source.IsValid()) return false;

            return other.Code.Equals(Code) &&
                other.Message.Equals(Message) &&
                other.Type.Equals(Type) &&
                other.Source.Equals(Source);
        }

        /// <summary>
        /// check if the given object equals the current instant
        /// </summary>
        /// <param name="obj">the object to check</param>
        /// <returns>true if equals, false if not</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(ResultError)) return false;
            if (ReferenceEquals(obj, this)) return true;
            return Equals(obj as ResultError);
        }

        /// <summary>
        /// get the hasCode of the current instant
        /// </summary>
        /// <returns>the has value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 13;
                hashCode = (hashCode * 397) ^ (Code.IsValid() ? Code.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type.IsValid() ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Source.IsValid() ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Message.IsValid() ? Message.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Generate an Error Object from an exception
        /// </summary>
        /// <param name="exception">the exception</param>
        /// <returns>an Error Object instant</returns>
        public static ResultError MapFromException(System.Exception exception)
            => new ResultError(
                source : exception.Source,
                message : exception.Message,
                type : exception.GetType().Name,
                code : ResultCode.InternalException
            );

        /// <summary>
        /// build the string representation of the object
        /// </summary>
        /// <returns>the string value of the object</returns>
        public override string ToString()
            => $"Error message: '{Message}', Code: {Code}, Source: {Source}";
    }
}
