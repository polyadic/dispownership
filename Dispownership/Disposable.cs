#pragma warning disable SA1201
#pragma warning disable IDE0005 // Disable unused usings warning if enabled in consuming project

using System;

namespace Dispownership;

#if DISPOWNERSHIP_VISIBILITY_PUBLIC
public
#else
internal
#endif
static class Disposable
{
    /// <summary>Creates a owned wrapper around a disposable i.e. the disposable will be disposed when the wrapper is disposed.</summary>
    public static Disposable<TDisposable> Owned<TDisposable>(TDisposable value)
        where TDisposable : IDisposable
        => new(value, hasOwnership: true);

    /// <summary>Creates a owned wrapper around a disposable i.e. the disposable will be disposed when the wrapper is disposed.</summary>
    /// <remarks>This overload is useful to communicate the ownership to analyzers such as <c>IDisposableAnalyzers</c>.</remarks>
    public static Disposable<TDisposable> Owned<TDisposable>(Func<TDisposable> createValue)
        where TDisposable : IDisposable
        => Owned(createValue());

    /// <summary>Creates a borrowing wrapper around a disposable i.e. the disposable will not be disposed when the wrapper is disposed.</summary>
    public static Disposable<TDisposable> Borrowed<TDisposable>(TDisposable value)
        where TDisposable : IDisposable
        => new(value, hasOwnership: false);
}

/// <summary>A wrapper around an <see cref="IDisposable"/> that either owns or borrows the value.
/// Use <see cref="Disposable.Owned{TDisposable}"/> or <see cref="Disposable.Borrowed{TDisposable}"/> to create an instance of this wrapper.</summary>
#if DISPOWNERSHIP_VISIBILITY_PUBLIC
public
#else
internal
#endif
struct Disposable<TDisposable> : IDisposable
    where TDisposable : IDisposable
{
    private readonly TDisposable _inner;
    private bool _hasOwnership;

    internal Disposable(TDisposable inner, bool hasOwnership)
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

    public void Dispose()
    {
        if (_hasOwnership)
        {
#pragma warning disable IDISP007
            _inner?.Dispose();
#pragma warning restore IDISP007
        }
    }
}
