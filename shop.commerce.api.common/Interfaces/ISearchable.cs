namespace shop.commerce.api.common
{
    /// <summary>
    /// mark an entity as searchable
    /// </summary>
    public interface ISearchable
    {
        /// <summary>
        /// represent a set of search terms
        /// </summary>
        string SearchTerms { get; }

        /// <summary>
        /// build the set of search terms for the object
        /// </summary>
        void BuildSearchTerms();
    }
}
