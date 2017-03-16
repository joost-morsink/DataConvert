# TryParseConverter
This converter is able to handle conversion through a static `TryParse` method. 
If multiple methods are found, the method with an `IFormatProvider` parameter and optionally a `NumberStyles` parameter takes precedence.
The good thing about these methods is they can indicate failure without throwing an exception.

At least the following classes have the TryParse method:
* All numeric datatypes, such as int, decimal, double
* Guid
* Version
* DateTime, although that method parses ISO 8601, it _may_ also parse other DateTime formats

### Parameters
The TryParseConverter class is configurable through the following constructor parameters:

| Parameter | Property | Type | Description | Default |
| --------- | -------- | ---- | ----------- | ------- |
| formatProvider | FormatProvider | IFormatProvider | The `IFormatProvider` to use as a `TryParse` method parameter | CultureInfo.InvariantCulture | 
| numberStyles | NumberStyles | NumberStyles | The NumberStyles value to use in `TryParse` methods. | NumberStyles.Float |
