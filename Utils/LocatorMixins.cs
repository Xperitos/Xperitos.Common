using System;
using Splat;

namespace Xperitos.Common.Utils
{
    public static class LocatorMixins
    {
        /// <summary>
        /// Registers a constant.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="obj">Constant object</param>
        /// <param name="key">The key.</param>
        /// <returns>The container.</returns>
        public static IMutableDependencyResolver RegisterConstant<TService>(this IMutableDependencyResolver container, TService obj, string key = null)
        {
            container.RegisterConstant(obj, typeof(TService), key);
            return container;
        }

        /// <summary>
        /// Registers a singleton - create the object on demand.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="lazyFunc">The creator function</param>
        /// <param name="key">The key.</param>
        /// <returns>The container.</returns>
        public static IMutableDependencyResolver RegisterLazySingleton<TService>(this IMutableDependencyResolver container, Func<TService> lazyFunc, string key = null)
        {
            container.RegisterLazySingleton(() => lazyFunc(), typeof(TService), key);
            return container;
        }

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
