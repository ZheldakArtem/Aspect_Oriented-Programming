using System.Runtime.Caching;
using Castle.DynamicProxy;

namespace AOP.Samples.CacheLib
{
	public class CacheInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			var key = GetCacheKey(invocation.Arguments);
			var value = MemoryCache.Default.Get(key);

			if (value == null)
			{
				invocation.Proceed();
				value = invocation.ReturnValue;

				MemoryCache.Default.Set(key, value, new CacheItemPolicy());
			}
			else
			{
				invocation.ReturnValue = value;
			}
		}

		string GetCacheKey(object[] arguments)
		{
			return string.Join(";", arguments);
		}
	}
}
