# SeparatedStringConverter
This component converts to and from strings which are formatted using a 'separator'. 
It accomplishes conversion by using the `Split` on the input string, or a `string.Join` on the input string array (The first character in the `Separators` property will be used).
This converter is supposed to be used in conjunction with other converters and therefore implements the `IDataConverterRef` interface.

Different strategies for conversion to a separated string are implemented:
* If the input is a `string[]`, it will use that `string.Join` overload directly.
* If the input type is assignable to `object[]`, it will cast and use that `string.Join` overload.
* If the input type is (assignable to) `IEnumerable<T>`, it will use the generic `string.Join<T>` overload.
* Otherwise the input will be converted to a `string[]` first, before `string.Join` is called. 

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

> `"1-123-456789"` converts to `new string[] {"1", "123", "456789"}`.

Then the string array is picked up by the `EnumerableToTupleConverter`, Converting all components using either the `IdentityConverter` (second component) or the `TryParseConverter` (first and third component).

> `"1"` converts to `1`.

> `"123"` converts to `"123"`.

> `"456789"` converts to `456789`.

Each value is set as a component value in the resulting `ValueTuple`:

> `new string[] {"1", "123", "456789"}` converts to `(1,"123",456789)`.

#### The other way around
With the addition of a [TupleToArrayConverter](tuple2arr.md) and a [ToStringConverter](toString.md) we can convert `(1,"123",456789)` back to the original value `"1-123-456789"`.

The first matching converter is again the `SeparatedStringConverter`, which tries to convert the tuple to a string array.
This conversion is picked up by the `TupleToArrayConverter`:

> `(1,"123",456789)` converts to `new string[] { "1", "123", "456789" }`.

The `TupleToArrayConverter` uses the `ToStringConverter` to convert the integer component values to strings.

Now that the `SeparatedStringConverter` has a `string[]` it can easily use the separator to perform the last step:

> `new string[] { "1", "123", "456789" }` converts `"1-123-456789"`.


