# Base85Converter
This converter converts back and forth between `String`s and `Byte[]`s using a custom Base-85 algorithm.
Base-85 is able to encode 32-bits in 5 ASCII characters. 

The algorithm only supports 'straight through' conversion using an alphabet of the specified 85 characters. 
It explicitly does not implement any shortcuts like 'z' for a 0 integer.

### Parameters
The Base85Converter class is configurable through the following constructor parameter:

| Parameter |  Type | Description | 
| --------- |  ---- | ----------- | 
| characters | char[] | An array of 85 unique ASCII characters to use for the encoding. |

This type has a static property `Default` that contains a Base85Converter with the default character set.
