using Splat;

namespace Xperitos.Common.Utils
{
    public static class LocatorMixins
    {
        /// <summary>
        /// Registers a singleton.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The container.</returns>
        public static IMutableDependencyResolver Singleton<TService, TImplementation>(this IMutableDependencyResolver container, string key = null)
            where TImplementation : TService, new()
        {
            container.RegisterLazySingleton(() => new TImplementation(), typeof(TService), key);
            return container;
        }

        /// <summary>
        /// Registers an service to be created on each request.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns>The container.</returns>
        public static IMutableDependencyResolver PerRequest<TService, TImplementation>(this IMutableDependencyResolver container, string key = null)
            where TImplementation : TService, new()
        {
            container.Register(() => new TImplementation(), typeof(TService), key);
            return container;
        }
    }
}
