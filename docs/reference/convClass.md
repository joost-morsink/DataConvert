# ConversionClassConverter<T>

This class has a generic type `T` in which static conversion methods can be coded. 
It only exposes exact conversion matches. 

Two method signatures are supported:

```csharp
// The first signature, may fail
public static ConversionResult<TDestination> Convert(TSource from);

// the second signature, always succeeds
public static TDestination Convert(TSource from);
```

The methods can have *any* name, but *must* be `public static`.
