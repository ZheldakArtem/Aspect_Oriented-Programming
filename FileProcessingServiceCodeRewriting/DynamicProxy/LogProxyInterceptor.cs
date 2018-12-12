using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicProxy
{
	public class LogProxyInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			invocation.Proceed();
			// подключить логер, и залогировать
		}
	}
}
