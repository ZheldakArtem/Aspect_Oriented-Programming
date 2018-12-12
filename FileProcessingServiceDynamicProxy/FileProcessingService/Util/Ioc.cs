using Autofac;
using Castle.DynamicProxy;
using DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessingService.Util
{
	public class Ioc
	{
		private static ILifetimeScope scope = Configure().BeginLifetimeScope();

		static Ioc()
		{

		}

		public static IContainer Configure()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<ProxyGenerator>().As<IProxyGenerator>();
			builder.RegisterType<LogProxyInterceptor>().As<IInterceptor>();
			builder.RegisterType<PdfCreator>();

			return builder.Build();
		}

		public static T GetInstance<T>()
		{
			return scope.Resolve<T>();
		}


	}
}
