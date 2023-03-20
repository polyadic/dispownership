using System;
using Xunit;

namespace Dispownership.Test;

public sealed class DisposableTest
{
    [Fact]
    public void DisposesInnerValueIfOwned()
    {
#pragma warning disable IDISP001
        var value = new DisposableStub();
#pragma warning restore IDISP001

        using (Disposable.Owned(value))
        {
        }

        Assert.True(value.Disposed);
    }

    [Fact]
    public void DoesNotDisposeInnerValueIfBorrowed()
    {
#pragma warning disable IDISP001
        var value = new DisposableStub();
#pragma warning restore IDISP001

        using (Disposable.Borrowed(value))
        {
        }

        Assert.False(value.Disposed);
    }

    [Fact]
    public void DoesNotDisposeOwnedValueAfterMove()
    {
#pragma warning disable IDISP001
        var value = new DisposableStub();
#pragma warning restore IDISP001

        using (var disposable = Disposable.Owned(value))
        {
            _ = disposable.Take();
        }

        Assert.False(value.Disposed);
    }

    [Fact]
    public void ThrowsWhenMovingAValueTwice()
    {
#pragma warning disable IDISP004
        using var disposable = Disposable.Owned(new DisposableStub());
#pragma warning restore IDISP004
        _ = disposable.Take();
        Assert.Throws<InvalidOperationException>(() => disposable.Take());
    }

    [Fact]
    public void ThrowsWhenMovingABorrowedValue()
    {
#pragma warning disable IDISP004
        using var disposable = Disposable.Borrowed(new DisposableStub());
#pragma warning restore IDISP004
        Assert.Throws<InvalidOperationException>(() => disposable.Take());
    }

    private sealed class DisposableStub : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }
}
