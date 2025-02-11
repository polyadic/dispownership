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

    [Theory]
    [MemberData(nameof(Disposables))]
    public void ThrowsWhenAccessingADisposedValue(Disposable<DisposableStub> disposable)
    {
#pragma warning disable IDISP007
        disposable.Dispose();
#pragma warning restore IDISP007
        Assert.Throws<ObjectDisposedException>(() => _ = disposable.Value);
        Assert.Throws<ObjectDisposedException>(() => _ = disposable.Take());
    }

    [Fact]
    public void OnlyCallsDisposeOnTheInnerValueOnce()
    {
#pragma warning disable IDISP001
        var stub = new CountingDisposableStub();
        var disposable = Disposable.Owned(stub);
#pragma warning restore IDISP001

        foreach (var unused in Enumerable.Range(0, count: 10))
        {
            disposable.Dispose();
        }

        Assert.Equal(1, stub.Disposed);
    }

#pragma warning disable IDISP004
    public static TheoryData<Disposable<DisposableStub>> Disposables()
        => [
            Disposable.Owned(new DisposableStub()),
            Disposable.Borrowed(new DisposableStub()),
        ];
#pragma warning restore IDISP004

    public sealed class DisposableStub : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    public sealed class CountingDisposableStub : IDisposable
    {
        public int Disposed { get; private set; }

        public void Dispose() => Disposed += 1;
    }
}
