using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Xperitos.Common.DiApp
{
    public static class DiAppServiceProviderMixins
    {
	    public static IServiceCollection AddMulti<TImpl>(this IServiceCollection collection, TImpl implementation = null)
		    where TImpl : class
	    {
		    if (implementation == null)
			    collection.AddSingleton<TImpl>();
		    else
			    collection.AddSingleton<TImpl>(implementation);


			return collection;
	    }

	    public static IServiceCollection AddMulti<TIface1, TImpl>(this IServiceCollection collection, TImpl implementation = null)
		    where TImpl : class, TIface1
		    where TIface1 : class
	    {
		    if (implementation == null)
			    collection.AddSingleton<TImpl>();
		    else
			    collection.AddSingleton<TImpl>(implementation);

			collection.AddSingleton<TIface1>(p => p.GetRequiredService<TImpl>());

		    return collection;
	    }

	    public static IServiceCollection AddMulti<TIface1, TIface2, TImpl>(this IServiceCollection collection, TImpl implementation = null)
		    where TImpl : class, TIface1, TIface2
		    where TIface1 : class
		    where TIface2 : class
	    {
		    if (implementation == null)
			    collection.AddSingleton<TImpl>();
		    else
			    collection.AddSingleton<TImpl>(implementation);

			collection.AddSingleton<TIface1>(p => p.GetRequiredService<TImpl>());
		    collection.AddSingleton<TIface2>(p => p.GetRequiredService<TImpl>());

			return collection;
	    }

	    public static IServiceCollection AddMulti<TIface1, TIface2, TIface3, TImpl>(this IServiceCollection collection, TImpl implementation = null)
		    where TImpl : class, TIface1, TIface2, TIface3
		    where TIface1 : class
		    where TIface2 : class
		    where TIface3 : class
	    {
		    if (implementation == null)
			    collection.AddSingleton<TImpl>();
		    else
			    collection.AddSingleton<TImpl>(implementation);

			collection.AddSingleton<TIface1>(p => p.GetRequiredService<TImpl>());
		    collection.AddSingleton<TIface2>(p => p.GetRequiredService<TImpl>());
			collection.AddSingleton<TIface3>(p => p.GetRequiredService<TImpl>());

			return collection;
	    }

	    public static IServiceCollection AddMulti<TIface1, TIface2, TIface3, TIface4, TImpl>(this IServiceCollection collection, TImpl implementation = null)
		    where TImpl : class, TIface1, TIface2, TIface3, TIface4
		    where TIface1 : class
		    where TIface2 : class
		    where TIface3 : class
		    where TIface4 : class
	    {
			if ( implementation == null )
				collection.AddSingleton<TImpl>();
			else
			    collection.AddSingleton<TImpl>(implementation);

		    collection.AddSingleton<TIface1>(p => p.GetRequiredService<TImpl>());
		    collection.AddSingleton<TIface2>(p => p.GetRequiredService<TImpl>());
			collection.AddSingleton<TIface3>(p => p.GetRequiredService<TImpl>());
			collection.AddSingleton<TIface4>(p => p.GetRequiredService<TImpl>());

			return collection;
	    }
	}
}
