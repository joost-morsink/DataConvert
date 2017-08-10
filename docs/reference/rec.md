# RecordConverter
This component is able to convert back and forth between dictionary-like types, called _record_s, and dataclasses.

_Record_ is an abstract concept and any type can be used if:
* `IRecord<T>` and `IRecordCreator` (nested) interface implementations can be provided.
* It has a parameterless constructor.

The interfaces are as follows:

```csharp
interface IRecordCreator
{
    bool CanConvertFromRecord { get; }
    bool CanConvertToRecord { get; }
    bool IsTypeCompatible(Type t);
    Type GetValueType(Type t);
    LambdaExpression Creator(Type t);
}
interface IRecord<T>
{
    bool TryGetValue(string key, out T value);
    void SetValue(string key, T value);
}
```

The creator interface is responsible for:
* Determining which ways the converter supports conversion through the `CanConvertFromRecord` and `CanConvertToRecord` properties.
* Determining whether some type is supported by this creator through implementation of `IsTypeCompatible`.
* Determining what the value type of some record type is through implementation of `GetValueType`.
* Creating a lambda expression that can create an `IRecord<T>` instance for the passed parameter through implementation of `Creator`.

The implementation of `IRecord<T>` is then used internally for getting and setting values. 
The interface can be viewed as the minimal subset of `IDictionary<K,V>` needed for this converter.

Implementations of these interfaces for `IDictionary<string, T>` and `Dictionary<string, T>` are given as nested classes on the RecordConverter and a RecordConverter using these can be constructed using the static `ForDictionaries()` method.

There is also an implementation for `IReadOnlyDictionary<string, T>` (only for conversions from implementing types) accesible through the static `ForReadOnlyDictionaries()` method.

The dataclasses need to conform to one of the following prescribed structure:
* Gettable properties and a constructor having a parameter for each property.
  The parameters should have the same name (case sensitive on all but the first character) as the properties.
* Settable properties and a parameterless constructor.

This converter cannot be configured and has no parameters.

### Success and Failure
For types that support the 'setter-pattern' all the properties are optional, but if a value is present and the value cannot be converted to the property type, the whole conversion fails.

For types that support the 'constructor-pattern', parameters without a default value are required.
If a value is present for either a required or an optional parameter and the value cannot be converted to the parameter type, the entire conversion fails.
