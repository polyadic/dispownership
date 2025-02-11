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
    public async Task ThrowsWhenAccessingADisposedValue(AsyncDisposable<AsyncDisposableStub> disposable)
    {
#pragma warning disable IDISP007
        await disposable.DisposeAsync();
#pragma warning restore IDISP007
        Assert.Throws<ObjectDisposedException>(() => _ = disposable.Value);
        Assert.Throws<ObjectDisposedException>(() => _ = disposable.Take());
    }

    [Fact]
    public async Task OnlyCallsDisposeOnTheInnerValueOnce()
    {
#pragma warning disable IDISP001
        var stub = new CountingDisposableStub();
        var disposable = AsyncDisposable.Owned(stub);
#pragma warning restore IDISP001

        foreach (var unused in Enumerable.Range(0, count: 10))
        {
            await disposable.DisposeAsync();
        }

        Assert.Equal(1, stub.Disposed);
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

    public sealed class CountingDisposableStub : IAsyncDisposable
    {
        public int Disposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            Disposed += 1;
            return default;
        }
    }
}
