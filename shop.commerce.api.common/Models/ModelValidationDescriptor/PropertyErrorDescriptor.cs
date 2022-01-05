namespace shop.commerce.api.common.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// this class describe property error
    /// </summary>
    public partial class PropertyErrorDescriptor
    {
        /// <summary>
        /// the property name
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// list of errors associated with the property
        /// </summary>
        public IEnumerable<ErrorDescriptor> Errors { get; set; }
    }

    /// <summary>
    /// partial part for <see cref="PropertyErrorDescriptor"/>
    /// </summary>
    public partial class PropertyErrorDescriptor
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public PropertyErrorDescriptor()
        {
            Errors = new List<ErrorDescriptor>();
        }

        /// <summary>
        /// construct a new instant of <see cref="PropertyErrorDescriptor"/>
        /// </summary>
        /// <param name="proprety">the name of the property</param>
        public PropertyErrorDescriptor(string proprety) : this()
        {
            Property = proprety;
        }

        /// <summary>
        /// convert the object to a string format
        /// </summary>
        /// <returns>string representation of the object</returns>
        public override string ToString()
            => $"Property: {Property} Errors Count: {Errors.Count()}";
    }
}
