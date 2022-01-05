namespace shop.commerce.api.common
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// a class that defines the Intersection of two collection
    /// </summary>
    public class Intersection<TEntity>
    {
        /// <summary>
        /// create an instant of <see cref="Intersection{TEntity}"/>
        /// </summary>
        public Intersection()
        {
            Removed = new HashSet<TEntity>();
            Added = new HashSet<TEntity>();
        }

        /// <summary>
        /// the list of removed items
        /// </summary>
        public IEnumerable<TEntity> Removed { get; set; }

        /// <summary>
        /// list of added items
        /// </summary>
        public IEnumerable<TEntity> Added { get; set; }

        /// <summary>
        /// the common entities between the two lists
        /// </summary>
        public IEnumerable<TEntity> Common { get; set; }

        /// <summary>
        /// get the string representation of the object
        /// </summary>
        /// <returns>the string value</returns>
        public override string ToString()
            => $"removed: {Removed.Count()}, added: {Added.Count()}";
    }
}
