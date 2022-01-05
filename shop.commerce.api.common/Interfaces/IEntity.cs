namespace shop.commerce.api.common
{
    /// <summary>
    /// a class that defines an entity
    /// </summary>
    public interface IEntity : ISearchable, System.ICloneable
    {
        /// <summary>
        /// the creation time of the model
        /// </summary>
        System.DateTime InsertDate { get; set; }

        /// <summary>
        /// the last time the model has been modified
        /// </summary>
        System.DateTime? LastUpdate { get; set; }
    }

    /// <summary>
    /// a class that defines an entity with a an id
    /// </summary>
    /// <typeparam name="Tkey">the type of the id</typeparam>
    public interface IEntity<Tkey> : IEntity, IPrimaryKey<Tkey> { }
}
