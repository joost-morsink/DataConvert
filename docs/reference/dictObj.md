# DictionaryObjectConverter
This component is able to convert back and forth between dictionaries (either `Dictionary<string, V>` or `IDictionary<string, V>`) and dataclasses.
The dataclasses need to conform to one of the following prescribed structure:
* Gettable properties and a constructor having a parameter for each property.
  The parameters should have the same name (case sensitive on all but the first character) as the properties.
* Settable properties and a parameterless constructor.

This converter cannot be configured and has no parameters.

### Success and Failure
For types that support the 'setter-pattern' all the properties are optional, but if a value is present and the value cannot be converted to the property type, the whole conversion fails.

For types that support the 'constructor-pattern', parameters without a default value are required.
If a value is present for either a required or an optional parameter and the value cannot be converted to the parameter type, the entire conversion fails.
