namespace shop.commerce.api.common.Models
{
    /// <summary>
    /// this class is used to describe the error
    /// </summary>
    public partial class ErrorDescriptor
    {
        /// <summary>
        /// the code of the message
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// the error message
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// partial part for <see cref="ErrorDescriptor"/>
    /// </summary>
    public partial class ErrorDescriptor : System.IEquatable<ErrorDescriptor>
    {
        /// <summary>
        /// construct a new instant of <see cref="ErrorDescriptor"/>
        /// </summary>
        public ErrorDescriptor() { }

        /// <summary>
        /// construct a new instant of <see cref="ErrorDescriptor"/>
        /// </summary>
        /// <param name="message">the error message</param>
        /// <param name="code">the message code</param>
        public ErrorDescriptor(string message, string code)
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// check if the given <see cref="ErrorDescriptor"/> equals the current instant,
        /// two <see cref="ErrorDescriptor"/> are equals if they have the same <see cref="Message"/> and <see cref="Code"/>
        /// </summary>
        /// <param name="other">the <see cref="ErrorDescriptor"/> to compare</param>
        /// <returns>true if equals false if not</returns>
        public bool Equals(ErrorDescriptor other)
            => other is null ? false : Message.Equals(other.Message, System.StringComparison.OrdinalIgnoreCase) && Code.Equals(other.Code) ? true : false;

        /// <summary>
        /// check if the given object equals the current instant
        /// </summary>
        /// <param name="obj">the object to check</param>
        /// <returns>true if equals, false if not</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(ErrorDescriptor)) return false;
            if (ReferenceEquals(obj, this)) return true;
            return Equals(obj as ErrorDescriptor);
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
                return (hashCode * 397) ^ (Message.IsValid() ? Message.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// the string representation of the object
        /// </summary>
        /// <returns>the string value</returns>
        public override string ToString()
            => $"Message: {Message}, Code: {Code}";
    }
}
