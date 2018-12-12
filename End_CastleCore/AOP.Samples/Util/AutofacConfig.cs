using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Web.Mvc;
using AOP.Samples.CacheLib;
using AOP.Samples.Models;
using Castle.DynamicProxy;

namespace AOP.Samples.Util
{
	public class AutofacConfig
	{
		public static void ConfigureContainer()
		{
			var builder = new ContainerBuilder();//MvcApplication
			builder.RegisterControllers(typeof(MvcApplication).Assembly);

			builder.RegisterType<ProxyGenerator>().As<IProxyGenerator>();
			builder.RegisterType<FibonacciEvaluator>().As<IFibonacciEvaluator>();
			builder.RegisterType<CacheInterceptor>().As<IInterceptor>();

			var container = builder.Build();

			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
		}
	}
}