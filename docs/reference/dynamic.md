# DynamicConverter
The dynamic converter does not do any conversions of its own, but uses runtime type checking and a backreference to the `IDataConverter` to convert.
The backreference is automatically obtained by implementation of the `IDataConverterRef` interface.

The input type is _always_ `System.Object`. 
The following steps apply:
* If the input value is null, the conversion fails (fast).
* The type is retrieved by using the `GetType` method. Let's call this type _RuntimeFrom_. 
* The type is used to get a converter from the back reference through the `GetGeneralConverter` method. The parameters are _RuntimeFrom_ and _To_.
* This converter is a `Func<object, IConversionResult>`, but it is known to actually return a `ConversionResult<To>`.
* A cast is done to `ConversionResult<To>`

Obviously this is not a very fast way to convert data, but considering the given type information it is the best we can do.

It is highly recommended to put this converter at the _end_ of a pipeline, to allow for other, smarter ways of conversion to be tried first.
