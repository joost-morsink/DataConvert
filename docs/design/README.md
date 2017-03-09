# Design

## Introduction {#intro}
This library can convert a value of _any_ type into _any_ type, as long as it is told how. 
It does this based on a conversion pipeline. 
The first component that succeeds in converting the value determines how the conversion is done. 
If none of the components are able to do the conversion, the conversion fails. 
The return type of the conversion reflects the possibility of failure.

Although this library follows a dependency injection pattern it is not tied to any IoC container. 

The library is compiled for the .Net Standard 1.3, because it is the lowest version that supports all the dependencies. 
It is highly likely this will change in a future version to a higher version, if needed, or even to .Net Standard 2.0 to align better with current guidelines.

## Generic interface {#interface}
The generic interface consists of a result struct and the interface on which other (extension) methods can be based. 

```csharp
struct ConversionResult<T> : IConversionResult
{
    public bool IsSuccessful { get; }
    public new T Value { get; }
}
interface IDataConverter
{
    Delegate GetConverter(Type from, Type to);
    Func<object, IConversionResult> GetGeneralConverter(Type from, Type to);
}
```

Please note the difference in usage of generics in the result type, and the absence of it in the conversion method provider. The `Delegate` should **always** be a `Func<T, ConversionResult<U>>`, where `typeof(T) == from` and `typeof(U) == to`. 
To help with the consumption of general conversion results, a non-generic interface `IConversionResult` is added:

```csharp
interface IConversionResult {
    bool IsSuccessful { get; }
    object Value { get; }
} 
```

When using the `GetGeneralConverter` method the consumer of the resulting Func needs to make sure that the type of object of the input is actually a type of the type passed to the `from` parameter when retrieving the function. 
The value **will** be cast to it by the implementation.

In the `IDataConverter` interface the pipeline is not visible, because it is an implementation detail of the default class that implements this interface. 
In any case it is good to keep the pipeline in mind.

The class that implements `IDataConverter` is `DataConverter`. 
Its constructor takes an `IEnumerable<IConverter>`, which represents the pipeline of actual possible converters.

## Pipeline {#pipeline}

An `IConverter` is the interface for a component that can take care of some conversions. 
It is possible for it to have a back reference to the IDataConverter to delegate work back to another converter in the pipeline. 
The `IDataConverterRef` interface provides this capability.

```csharp
interface IConverter
{
    bool CanConvert(Type from, Type to);
    Delegate GetConverter(Type from, Type to);
}
```

An `IConverter` implementation should provide a single conversion strategy, to keep the pipeline as flexible as possible.
Examples are conversions to `string`, using the `ToString` method. Or conversions from `string` using a static `TryParse` method.

Delegates providing the actual implementation of conversion should be cached by the `IDataConverter`, and are composed of any delegates created by the `IConverter`s that indicate being able to do the conversion. 
For any combination of `from` and `to` types there are a number (could be 0) of converters that are chained together in one big Func. 
The `DataConverter` class handles this by chaining and caching delegates from `IConverter`s in a private nested class `Entry`.

Because of the interface requirements combined with the need for strongly typed delegates, code generation is required for all but the most simple conversions.
This library has a dependency on System.Linq.Expressions for code generation.
