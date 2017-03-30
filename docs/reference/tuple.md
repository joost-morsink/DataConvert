# TupleConverter
This component converts between tuple types of the same arity by converting the component values one by one. 
It supports regular `System.Tuple` types as well as the new `System.ValueTuple` types. 
Support depends on naming and structure.

This converter is not configurable and has no constructor parameters.

This type supports the singleton pattern by implementation of a static property `Instance`.

### Example
Assuming a simple pipeline with:
* [IdentityConverter](identity.md)
* [ToStringConverter](toString.md)
* TupleConverter

Trying to transform a `Tuple<int, decimal, string>` to a `ValueTuple<string, string, string>`:

```csharp
(var x, var y, var z) = converter.Convert(Tuple.Create(1, 2m, "x")).To<(string, string, string)>();
if (x == "1" && y == "2" && z == "x")
    Console.WriteLine("Success!");
// Prints Success!
```