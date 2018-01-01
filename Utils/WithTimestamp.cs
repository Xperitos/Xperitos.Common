using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

namespace Xperitos.Common.Utils
{
	/// <summary>
	/// Interface timestamped instance similar to <see cref="Timestamped{T}"/> but with interface implementation.
	/// </summary>
	public struct WithTimestamp<T> : ITimestamp, IEquatable<WithTimestamp<T>>
	{
		public WithTimestamp(T value, DateTimeOffset timestamp)
		{
			Timestamp = timestamp;
			Value = value;
		}

		public DateTimeOffset Timestamp { get; }
		public T Value { get; }

		public bool Equals(WithTimestamp<T> other)
		{
			return Timestamp.Equals(other.Timestamp) && EqualityComparer<T>.Default.Equals(Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is WithTimestamp<T> timestamp && Equals(timestamp);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Timestamp.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
			}
		}

		public static bool operator ==(WithTimestamp<T> left, WithTimestamp<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(WithTimestamp<T> left, WithTimestamp<T> right)
		{
			return !left.Equals(right);
		}
	}

	public static class WithTimestamp
	{
		public static WithTimestamp<T> ToWithTimestamp<T>(this Timestamped<T> item)
		{
			return new WithTimestamp<T>(item.Value, item.Timestamp);
		}

		public static IObservable<WithTimestamp<T>> ToWithTimestamp<T>(this IObservable<Timestamped<T>> ob)
		{
			return ob.Select(v => v.ToWithTimestamp());
		}

		public static WithTimestamp<T> Create<T>(T item, DateTimeOffset ts)
		{
			return new WithTimestamp<T>(item, ts);
		}
	}
}