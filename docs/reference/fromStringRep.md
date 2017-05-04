# FromStringRepresentationConverter

This converter uses a backreference to try to convert the string representation of the input object.
It is meant to be used as a fallback component, because conversion to string and parsing could potentially be expensive operations.
It uses a `ToString` method with an `IFormatProvider` typed parameter if one is present, otherwise a parameterless `ToString` method if one is declared.
The `Object.ToString` method is only used if `RequireDeclaredMethod` is `false`.
The result is passed back into the data converter.
If the input is a `null` value (applies only to reference types), the conversion fails.

### Parameters
The TryParseConverter class is configurable through the following constructor parameters:

| Parameter | Property | Type | Description | Default |
| --------- | -------- | ---- | ----------- | ------- |
| requireDeclaredMethod | RequireDeclaredMethod | bool | Indicates whether a `ToString` method needs to be declared on the type. | 
| formatProvider | FormatProvider | IFormatProvider | The `IFormatProvider` to use as a `ToString` method parameter | CultureInfo.InvariantCulture | 

### Example
Let's assume a pipeline with the following components:
* FromStringRepresentationConverter
* [TryParseConverter](tryParse.md)

This configuration enables a lot of numeric conversions, for instance converting an integer to a decimal:

```csharp
int x = 42;
var c = convert.GetConverter<int, decimal>();
var result = c(x);
if(result.IsSuccessful && result.Result == 42m)
    Console.WriteLine("Success!");
// Prints Success!
```

#### How does this work? 

The `GetConverter` call filters out all components that cannot convert from int to decimal.
The `FromStringRepresentationConverter` can try to convert the string representation of the integer by passing the value back, but the `TryParseConverter` only works on `String`s. 
So the converter only uses the first component. 

So `42` is converted to the string `"42"` and passed back into the pipeline. 
Now a converter from `String` to `Decimal` is retrieved. 
The `FromStringRepresentationConverter` is skipped, because the input is already a `String` and we would like to prevent infinite loops and stack overflows.
The `TryParseConverter` however tries to find a `TryParse` method on `Decimal`, which it finds.
This method is used to successfully convert the value `"42"` to `42m` (decimal).

