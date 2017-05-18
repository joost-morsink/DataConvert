# LosslessStringToTupleConverter
This component converts strings which are formatted using a 'separator' into tuples.
It does so without losing any information during conversion.  
It accomplishes conversion by using the `Split` on the input string.
This converter is supposed to be used in conjunction with other converters and therefore implements the `IDataConverterRef` interface.

After splitting the input string up into its parts, precedence is given to the parts at the end of the string.
For instance if a value of `"1-2-3"` (separator = `-`) is converted to a `(string,string)` the result will be `("1-2","3")`.

### Parameters
The LosslessStringToTupleConverter class is configurable through the following constructor parameters:

| Parameter | Property | Type | Description | 
| --------- | -------- | ---- | ----------- | 
| separator | Separator | char | The string separator character to use. |




