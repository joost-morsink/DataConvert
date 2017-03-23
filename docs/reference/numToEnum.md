# NumericToEnumConverter
This converter converts numeric values to enum values. 
For regular enums it performs a simple bounds check based on the minimum and maximum possible values for the enum.
Checking for every possible value could be a performance hit, but might be supported in a future version.
Enums attributed with the `FlagsAttribute` are checked using a bitmask.
If the check fails the conversion fails.

Every enum is actually just a simple numeric primitive with some type information on top. 
The numeric type that can represent an enum is called its underlying type.
If the input type does not match the enum's underlying type, the input parameter will be fed back into the pipeline to convert to the underlying type first.
Failure of conversion to the underlying type propagates to this conversion. 

This converter cannot be configured and has no constructor parameters.