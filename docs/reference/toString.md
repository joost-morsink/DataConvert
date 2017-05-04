# ToStringConverter

This converter converts _any_ value to string, except object itself. 
Objects should be typechecked before trying to find a more specific conversion.
`System.Object` lacks a `ToString(IFormatProvider)` method, reflection on that method is a performance hit and not checking means having different behaviour depending on how the conversion is called.

### Parameters
| Parameter | Property | Type | Description |
| --------- | -------- | ---- | ----------- |
| succeedOnNull | SucceedOnNull | bool | Indicates whether the conversion should succeed if the input value is a _null_. In that case the result is an empty string. |
| requireDeclaredMethod | RequireDeclaredMethod | bool | Indicates whether a `ToString` method needs to be declared on the type. | 
| formatProvider | FormatProvider | IFormatProvider | The `IFormatProvider` to use as a `ToString` method parameter. |

The third parameter is optional, and if not provided it will use `CultureInfo.InvariantCulture`. 
Note that this may not be proper behaviour for `DateTime`s and `DateTimeOffset`s when an ISO 8601 is required.
In that case putting a specific `DateTime` to string converter before this component in the pipeline will be a solution.
