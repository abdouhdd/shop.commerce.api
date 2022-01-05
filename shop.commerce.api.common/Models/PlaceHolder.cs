namespace shop.commerce.api.common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// this class defines a place holder
    /// </summary>
    public partial struct PlaceHolder
    {
        /// <summary>
        /// the name of the place holder, must be in format : ##placeholderName##
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the value of the place holder
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// partial class for <see cref="PlaceHolder"/>
    /// </summary>
    public partial struct PlaceHolder : IEquatable<PlaceHolder>
    {
        public static readonly PlaceHolder Empty = new PlaceHolder("", "");
        public static readonly string RigthBound = "#";
        public static readonly string LeftBound = "#";

        /// <summary>
        /// create a new <see cref="PlaceHolder"/>
        /// </summary>
        /// <param name="name">the name of the place holder</param>
        /// <param name="value">the value of the place holder</param>
        public PlaceHolder(string name, string value)
            : this()
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// get the string representation of the object
        /// </summary>
        /// <returns>the string value</returns>
        public override string ToString()
            => $"Name: {Name} => vale: {Value}";

        /// <summary>
        /// set the place holders values to the given string
        /// </summary>
        /// <param name="content">the content to set the place holders for it</param>
        /// <param name="placeHolders">the list of place holders</param>
        /// <returns>the new string value</returns>
        public static string Set(string content, params PlaceHolder[] placeHolders)
        {
            foreach (var placeHolder in placeHolders)
            {
                var placeHolderName = $"{LeftBound}{placeHolder.Name}{RigthBound}";
                content = content.Replace(placeHolderName, placeHolder.Value);
            }

            return content;
        }

        /// <summary>
        /// create an array of place holders with one <see cref="PlaceHolder"/> instant using the given name and value
        /// </summary>
        /// <param name="name">the name of the place holder</param>
        /// <param name="value">the value of the place holder</param>
        /// <returns>an array of <see cref="PlaceHolder"/></returns>
        public static PlaceHolder[] Get(string name, string value)
            => new PlaceHolder[] { new PlaceHolder(name, value) };

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(obj, this)) return true;
            if (obj.GetType() != typeof(PlaceHolder)) return false;
            return Equals((PlaceHolder)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 13;
                hashCode = (hashCode * 397) ^ EqualityComparer<string>.Default.GetHashCode(Value ?? "");
                return (hashCode * 397) ^ EqualityComparer<string>.Default.GetHashCode(Name ?? "");
            }
        }

        /// <inheritdoc/>
        public bool Equals(PlaceHolder other)
            => Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase) &&
               Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public static bool operator ==(PlaceHolder left, PlaceHolder right)
            => EqualityComparer<PlaceHolder>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(PlaceHolder left, PlaceHolder right) => !(left == right);
    }
}
