# SeparatedStringConverter
This component converts to and from strings which are formatted using a 'separator'. 
It accomplishes conversion by using the `Split` on the input string, or a `string.Join` on the input string array (The first character in the `Separators` property will be used).
This converter is supposed to be used in conjunction with other converters and therefore implements the `IDataConverterRef` interface.

At the time of writing only conversion from strings is supported.

### Parameters
The SeparatedStringConverter class is configurable through the following constructor parameters:

| Parameter | Property | Type | Description | 
| --------- | -------- | ---- | ----------- | 
| separators | Separators | char[] | The string separator characters to use. |

### Example
Consider the following pipeline:

* [IdentityConverter](identity.md)
* [TryParseConverter](tryParse.md)
  * with separators = `new char[] {'-'}`
* SeparatedStringConverter
* [EnumerableToTupleConverter](enumerable2tuple.md)

When trying to convert the value `1-123-456789` to a `System.ValueTuple<int, string, int>` it first encounters the SeparatedStringConverter:

> `"1-123-456789"` converts to `new string[] {"1", "123", "456789"}`

Then the string array is picked up by the `EnumerableToTupleConverter`, Converting all components using either the `IdentityConverter` (second component) or the `TryParseConverter` (first and third component).

> `"1"` converts to `1`

> `"123"` converts to `"123"`

> `"456789"` converts to `456789`

Each value is set as a component value in the resulting `ValueTuple`:

> `new string[] {"1", "123", "456789"}` converts to `(1,"123",456789)`.



