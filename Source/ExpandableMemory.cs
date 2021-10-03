using Microsoft.Toolkit.HighPerformance.Buffers;
using System;

namespace Open.Text.CSV;

public class ExpandableMemory<T> : IDisposable
{
	MemoryOwner<T> _owner;

	const int DefaultInitialCapacity = 8;
	public ExpandableMemory()
	{
		_owner = MemoryOwner<T>.Allocate(DefaultInitialCapacity);
	}

	public int Capacity { get; private set; } = DefaultInitialCapacity;
	public int Length { get; private set; }

	public Memory<T> Memory => _owner.Memory.Slice(0, Length);
	public Span<T> Span => _owner.Span.Slice(0, Length);

	public void Add(in T value)
	{
		if (Length == Capacity)
		{
			checked { Capacity *= 2; }

			var newMemory = MemoryOwner<T>.Allocate(Capacity);
			_owner.Memory.CopyTo(newMemory.Memory);
			_owner.Dispose();
			_owner = newMemory;
		}

		_owner.Span[Length] = value;
		Length++;
	}

	public void Clear()
	{
		Length = 0;
	}

	public MemoryOwner<T> ExtractOwner()
	{
		var owner = _owner.Slice(0, Length);
		_owner = MemoryOwner<T>.Allocate(Capacity);
		Length = 0;
		return owner;
	}

	public void Dispose() => _owner.Dispose();

}
