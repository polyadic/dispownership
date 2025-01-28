using System;
using System.Threading.Tasks;
using Xunit;

namespace Dispownership.Test;

public sealed class AsyncDisposableTest
{
    [Fact]
    public async Task DisposesInnerValueIfOwned()
    {
        var value = new AsyncDisposableStub();

        await using (AsyncDisposable.Owned(value))
        {
        }

        Assert.True(value.Disposed);
    }

    [Fact]
    public async Task DoesNotDisposeInnerValueIfBorrowed()
    {
        var value = new AsyncDisposableStub();

        await using (AsyncDisposable.Borrowed(value))
        {
        }

        Assert.False(value.Disposed);
    }

    [Fact]
    public async Task DoesNotDisposeOwnedValueAfterMove()
    {
        var value = new AsyncDisposableStub();

        await using (var disposable = AsyncDisposable.Owned(value))
        {
            _ = disposable.Take();
        }

        Assert.False(value.Disposed);
    }

    [Fact]
    public async Task ThrowsWhenMovingAValueTwice()
    {
        await using var disposable = AsyncDisposable.Owned(new AsyncDisposableStub());
        _ = disposable.Take();
        Assert.Throws<InvalidOperationException>(() => disposable.Take());
    }

    [Fact]
    public async Task ThrowsWhenMovingABorrowedValue()
    {
        await using var disposable = AsyncDisposable.Borrowed(new AsyncDisposableStub());
        Assert.Throws<InvalidOperationException>(() => disposable.Take());
    }

    [Theory]
    [MemberData(nameof(Disposables))]
    public async Task ThrowsWhenDisposingTwice(AsyncDisposable<AsyncDisposableStub> disposable)
    {
#pragma warning disable IDISP007
        await disposable.DisposeAsync();
#pragma warning restore IDISP007
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposable.DisposeAsync());
    }

    public static TheoryData<AsyncDisposable<AsyncDisposableStub>> Disposables()
        => [
            AsyncDisposable.Owned(new AsyncDisposableStub()),
            AsyncDisposable.Borrowed(new AsyncDisposableStub()),
        ];

    public sealed class AsyncDisposableStub : IAsyncDisposable
    {
        public bool Disposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
