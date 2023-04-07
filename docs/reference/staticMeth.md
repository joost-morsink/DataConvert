# StaticMethodConverter

The `StaticMethodConverter` uses a configurable method name to try and convert between source an destination types:

```csharp
public static bool TryConvert(Src source, out Dest result)
```

The converter has a static `Instance` property which uses the `TryConvert` method name.
