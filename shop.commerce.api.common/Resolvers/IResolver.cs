namespace shop.commerce.api.common.Resolvers
{
    using shop.commerce.api.common;

    /// <summary>
    /// service resolver base on the <see cref="ApplicationType"/>
    /// </summary>
    /// <typeparam name="TService">the type of the service to be resolved</typeparam>
    public interface IResolver<TService>
	{
		/// <summary>
		/// resolve the requested instant base on the given <see cref="ApplicationType"/>
		/// </summary>
		/// <param name="applicationType">the type of the application</param>
		/// <returns>the <see cref="TService"/> instant</returns>
		TService Resolve(ApplicationType applicationType);
	}
}