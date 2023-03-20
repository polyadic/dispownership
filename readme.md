# Dispownership
Bringing ownership to disposables.

*Well, this may be a bit overpromising, but this library does provide useful utilities for working with disposables.*

## Packages

* **Dispownership** \
  [![NuGet package](https://buildstats.info/nuget/Dispownership)](https://www.nuget.org/packages/Dispownership)
* **Dispownership.Sources** \
  [![NuGet package](https://buildstats.info/nuget/Dispownership.Sources)](https://www.nuget.org/packages/Dispownership.Sources)

## Examples
These examples use the [TempSubdirectory](https://github.com/messerli-informatik-ag/temp-directory) type as a stand-in for a disposable value.

### Borrowed or Owned
```cs
using Dispownership;

public sealed class Example : IDisposable
{
    private readonly Disposable<TempSubdirectory> _tempDirectory;

    public Example()
    {
        // No temp directory provided, let's create one ourselves.
        // We have to ensure cleanup ourselves, hence owned.
        _tempDirectory = Disposable.Owned(TempSubdirectory.Create());
    }

    public Example(TempSubdirectory tempDirectory)
    {
        // Temp directory provided by caller.
        // We want the caller to be responsible for disposal, hence "borrowed".
        _tempDirectory = Disposable.Borrowed(tempDirectory);
    }

    public void Dispose()
    {
        // The wrapper takes care of tracking if disposal should be propagated or not,
        // depending on whether the value was wrapped using Owned or Borrowed.
        _tempDirectory.Dispose();
    }
}
```

### "Move"
```cs
using Dispownership;

public TempSubdirectory CreateTempSubdirectoryWithExampleFiles()
{
    using var tempDirectory = Disposable.Owned(TempSubdirectory.Create());
    //                        ^^^^^^^^^^^^^^^^
    //  This creates a wrapper around our disposable, allowing us to later "move" the disposable.

    File.WriteAllText(Path.Combine(tempDirectory.Value.FullName, "example.txt"), contents: "Example");
    //   ^^^^^^^^^^^^
    // If this throws, our disposable will be cleaned up by the wrapper.

    return tempDirectory.Move();
    //                  ^^^^^^^
    //   This "moves" the disposable out of the wrapper,
    //   preventing the wrapper from disposing our value when the function returns.
}
```

## Detailed Example
Sometimes you might create a disposable, that you eventually want to return.
But before that, you perform some work on the disposable:

```cs
public TempSubdirectory CreateTempSubdirectoryWithExampleFiles()
{
    var tempDirectory = TempSubdirectory.Create();
    File.WriteAllText(Path.Combine(tempDirectory.FullName, "example.txt"), contents: "Example");
    return tempDirectory;
}
```

But wait, what if `File.WriteAllText` fails? We still want the temp directory to be cleaned up then.
Adding a `using` to `var tempDirectory` is no option, because it would dispose the temp directory before the function returns.

The solution is to add a `try` / `catch` and dispose in case of errors:

```cs
public TempSubdirectory CreateTempSubdirectoryWithExampleFiles()
{
    var tempDirectory = TempSubdirectory.Create();

    try
    {
        File.WriteAllText(Path.Combine(tempDirectory.FullName, "example.txt"), contents: "Example");
        return tempDirectory;
    }
    catch
    {
        tempDirectory.Dispose();
        throw;
    }
}
```

This is not as readable compared to the ease of `using` declarations.
Using `Dispownership` we can rewrite example as follows, preserving behaviour:

```cs
using Dispownership;

public TempSubdirectory CreateTempSubdirectoryWithExampleFiles()
{
    using var tempDirectory = Disposable.Owned(TempSubdirectory.Create());
    //                        ^^^^^^^^^^^^^^^^
    //  This creates a wrapper around our disposable, allowing us to later "move" the disposable.

    File.WriteAllText(Path.Combine(tempDirectory.Value.FullName, "example.txt"), contents: "Example");
    //   ^^^^^^^^^^^^
    // If this throws, our disposable will be cleaned up by the wrapper.

    return tempDirectory.Move();
    //                  ^^^^^^^
    //   This "moves" the disposable out of the wrapper,
    //   preventing the wrapper from disposing our value when the function returns.
}
```
