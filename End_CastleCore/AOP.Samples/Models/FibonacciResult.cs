using System;

namespace AOP.Samples.Models
{
	public class FibonacciResult
	{
		public uint Value { get; set; }

		public ulong Result { get; set; }

		public TimeSpan EvaluationTime { get ; set; }
	}
}