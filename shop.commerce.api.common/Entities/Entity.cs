namespace shop.commerce.api.common
{
    using System;

    /// <summary>
    /// Entity class that implement <see cref="IEntity"/> and inherit from the <see cref="Entity"/> base class
    /// </summary>
    /// <typeparam name="Tkey">type of key</typeparam>
    public abstract class Entity<Tkey> : Entity, IEntity<Tkey>
    {
        /// <summary>
        /// the id of the entity
        /// </summary>
        public Tkey Id { get; set; }
    }

    /// <summary>
    /// Entity class that implement <see cref="IEntity"/>, which is the base entity class
    /// </summary>
    public abstract partial class Entity : IEntity
    {
        /// <summary>
        /// create an instant of <see cref=""/>
        /// </summary>
        protected Entity()
        {
            InsertDate = DateTime.Now;
            //LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// the creation time of the model
        /// </summary>
        public DateTime InsertDate { get; set; }

        /// <summary>
        /// the last time the model has been modified
        /// </summary>
        public DateTime? LastUpdate { get; set; }

        /// <summary>
        /// represent a set of search terms
        /// </summary>
        public string SearchTerms { get; set; }

        /// <summary>
        /// build the set of search terms for the object
        /// </summary>
        public abstract void BuildSearchTerms();

        /// <summary>
        /// Creates a new object that is a copy of the current instance
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone() => MemberwiseClone();
    }
}
