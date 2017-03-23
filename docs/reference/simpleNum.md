# SimpleNumericConverter
The `SimpleNumericConverter` takes care of converting between different numeric types. 
Currently all signed .Net numeric types from the `System` namespace are supported:
* sbyte
* short
* int
* long
* decimal
* float
* double

This converter cannot be configured and has no parameters.

This type supports the singleton pattern by implementation of a static property `Instance`.

### Implementation
If the type being converted from is 'smaller' than the type being converted to, a simple implicit conversion can be done.
If it is 'bigger', then the bounds will be checked before conversion.
Precision might be lost when using this converter.

### Future work
This converter could be extended with more types.
However, limiting the converter to this set of types simplifies the logic substantially.
The limitation allows for straightforward reasoning about _smaller_ and _larger_ numerical types.

A more advanced numerical converter might be constructed in a future version.
