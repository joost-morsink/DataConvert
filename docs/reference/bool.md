# BooleanConverter
This class takes care of converting from and to boolean values. 
The following .Net types are supported:
* String
* Byte
* Int16 (short)
* Int32 (int)
* Int64 (long)

Conversion from and to numeric types always succeed.
Any non-zero integer is converted to `true`.

Numerically formatted strings are treated as ints.
Any casing variant of `true` or `false` will be converted to the corresponding boolean values.
Any other string (including the empty string) fails conversion.

This component is not configurable and does not take any constructor parameters. 

This type supports the singleton pattern by implementation of a static property `Instance`.

