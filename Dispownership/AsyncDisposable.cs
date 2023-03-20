#pragma warning disable SA1201
#pragma warning disable IDE0005 // Disable unused usings warning if enabled in consuming project

#if (NETSTANDARD2_1 || NET5_0_OR_GREATER) && DISPOWNERSHIP_ASYNC
using System;
using System.Threading.Tasks;

namespace Dispownership;

#if DISPOWNERSHIP_VISIBILITY_PUBLIC
public
#else
internal
#endif
static class AsyncDisposable
{
    /// <summary>Creates a owned wrapper around a disposable i.e. the disposable will be disposed when the wrapper is disposed.</summary>
    public static AsyncDisposable<TDisposable> Owned<TDisposable>(TDisposable value)
        where TDisposable : IAsyncDisposable
        => new(value, hasOwnership: true);

    /// <summary>Creates a owned wrapper around a disposable i.e. the disposable will be disposed when the wrapper is disposed.</summary>
    /// <remarks>This overload is useful to communicate the ownership to analyzers such as <c>IDisposableAnalyzers</c>.</remarks>
    public static AsyncDisposable<TDisposable> Owned<TDisposable>(Func<TDisposable> createValue)
        where TDisposable : IAsyncDisposable
        => Owned(createValue());

    /// <summary>Creates a borrowing wrapper around a disposable i.e. the disposable will not be disposed when the wrapper is disposed.</summary>
    public static AsyncDisposable<TDisposable> Borrowed<TDisposable>(TDisposable value)
        where TDisposable : IAsyncDisposable
        => new(value, hasOwnership: false);
}

/// <summary>A wrapper around an <see cref="IAsyncDisposable"/> that either owns or borrows the value.
/// Use <see cref="AsyncDisposable.Owned{TDisposable}(TDisposable)"/> or <see cref="AsyncDisposable.Borrowed{TDisposable}"/> to create an instance of this wrapper.</summary>
#if DISPOWNERSHIP_VISIBILITY_PUBLIC
public
#else
internal
#endif
sealed class AsyncDisposable<TDisposable> : IAsyncDisposable
    where TDisposable : IAsyncDisposable
{
    private readonly TDisposable _inner;
    private bool _hasOwnership;

    internal AsyncDisposable(TDisposable inner, bool hasOwnership)
    {
        _inner = inner;
        _hasOwnership = hasOwnership;
    }

    public TDisposable Value => _inner;

    /// <summary>Consumes the value leaving this wrapper without ownership.
    /// This is useful in scenarios where you want to create a disposable, do some work that might fail and then return it.</summary>
    /// <exception cref="InvalidOperationException">Thrown when this instance does not have ownership over the disposable.</exception>
    public TDisposable Take()
    {
        if (!_hasOwnership)
        {
            throw new InvalidOperationException(
                $"Value can only be consumed when owned. " +
                $"This error may occur if you call {nameof(Take)} more than once");
        }

        _hasOwnership = false;
        return _inner;
    }

#pragma warning disable IDISP007
    public ValueTask DisposeAsync()
        => _hasOwnership
            ? (_inner?.DisposeAsync() ?? default)
            : default;
#pragma warning restore IDISP007
}
#endif
