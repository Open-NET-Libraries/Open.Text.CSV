using BenchmarkDotNet.Attributes;
using Open.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Open.Text.CSV.Test.Benchmarks;

internal class SimpleBenchmark<T>
	: BenchmarkBase<T>
{
	readonly T _target;

	public SimpleBenchmark(uint size, uint repeat, T param)
		: base(size, repeat, param)
	{
		_target = param ?? throw new System.ArgumentNullException(nameof(param));
	}


	protected override IEnumerable<TimedResult> TestOnceInternal()
	{
		var methods = _target.GetType().GetMethods()
					  .Where(m => m.GetCustomAttributes(typeof(BenchmarkAttribute), false).Length > 0);

		foreach (var method in methods)
		{
			yield return TimedResult.Measure(method.Name,
			() =>
			{
				for (var i = 0; i < TestSize; ++i)
				{
					_ = method.Invoke(_target, null);
				}
			});
		}
	}

	public static TimedResult[] Results(uint size, uint repeat, T tests)
		=> new SimpleBenchmark<T>(size, repeat, tests).Result;
}
