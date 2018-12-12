namespace AOP.Samples.Models
{
	public class FibonacciEvaluator : IFibonacciEvaluator
	{
		public virtual ulong Evaluate(uint value)
		{
			if (value == 0)
				return 0;

			if (value == 1)
				return 1;

			return Evaluate(value - 1) + Evaluate(value - 2);
		}

		public virtual void Test()
		{
			var s = "wec";
			int f = s.Length;
		}
	}
}
