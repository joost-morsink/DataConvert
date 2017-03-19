# IsoDateTimeConverter
This converter converts back and forth between `String`s and `DateTime`s and `DateTimeOffset`s. 
The standard ISO 8601 format is used. 
When `DateTime` is used, it is converted to `DateTimeKind.Utc`, but when `DateTimeOffset` is used the timezone information is considered part of the data.

The user still must take care that Utc and Local time are not mixed up when using `DateTime`s.
The standard BCL cannot considered to be safe. 

The string representation is exact up to the millisecond level.

This converter is not configurable and has no constructor parameters.
