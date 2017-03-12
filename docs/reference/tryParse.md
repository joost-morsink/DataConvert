# TryParseConverter
This converter is able to handle conversion through a static '_TryParse_' method. 
The good thing about these methods is they can indicate failure without throwing an exception.

At least the following classes have the TryParse method:
* All numeric datatypes, such as int, decimal, double
* Guid
* Version
* DateTime, although that method parses ISO 8601, it _may_ also parse other DateTime formats

The TryParseConverter class is not configurable does not take any constructor parameters.

