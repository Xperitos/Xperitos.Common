using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class ServiceProviderMixins
    {
        /// <summary>
        /// Type-safe overload for get service.
        /// </summary>
        public static T GetService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T));
        }
    }
}
