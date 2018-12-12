using System.Diagnostics;
using System.Web.Mvc;
using AOP.Samples.CacheLib;
using AOP.Samples.Models;
using Castle.DynamicProxy;

namespace AOP.Samples.Controllers
{
	public class HomeController : Controller
	{
		private IFibonacciEvaluator _fibonacciEvaluator;
		public HomeController(IProxyGenerator generator, IFibonacciEvaluator fibonacciEvaluator, IInterceptor interceptor)
		{
			this._fibonacciEvaluator = generator.CreateInterfaceProxyWithTarget(fibonacciEvaluator, interceptor);
		}

		// GET: Home
		public ActionResult Index()
		{
			return View(new FibonacciResult());
		}

		[HttpPost]
		public ActionResult Index(uint value)
		{
			var stopwatch = Stopwatch.StartNew();
			var result = _fibonacciEvaluator.Evaluate(value);
			stopwatch.Stop();

			return View(new FibonacciResult { Value = value, Result = result, EvaluationTime = stopwatch.Elapsed });
		}
	}
}