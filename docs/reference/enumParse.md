# EnumParseConverter
This converter uses the `Enum.TryParse<T>` method to convert a string to an enum value.
It can parse numeric and string representations of enum values as well as comma separated _Flags_ enums.

### Parameters
The EnumParseConverter can be configured using the following parameter:

| Parameter | Property | Type | Description |
| --------- | -------- | ---- | ----------- | 
| ignoreCase | IgnoreCase | Boolean | Indicates if casing should be ignored when parsing. | 

