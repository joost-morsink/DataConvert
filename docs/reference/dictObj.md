# DictionaryObjectConverter
This component is able to convert back and forth between dictionaries (either `Dictionary<string, V>` or `IDictionary<string, V>`) and dataclasses.
The dataclasses need to conform to one of the following prescribed structure:
* Gettable properties and a constructor having a parameter for each property.
  The parameters should have the same name (case sensitive on all but the first character) as the properties.
* Settable properties and a parameterless constructor.

This converter cannot be configured and has no parameters.

### Future work
The current version requires all properties/parameters to be present in the input dictionary. 
A future version could relax this constraint in some way.
