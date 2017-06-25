# Base64IntegerConverter
This component converts integral numeric types from and to base64 encoded strings.
The encoded string does not use padding, because an integral type's padding has no semantic value.
The supported types are the primitives signed and unsigned of 16, 32 and 64 bits, as well as the `BigInteger` type.
The maximum length for `BigInteger` base-64 encoded strings is currently set to 128 (768 significant bits).

### Parameters
The Base64IntegerConverter can be configured using the following parameter:

| Parameter | Property | Type | Description |
| --------- | -------- | ---- | ----------- | 
| mapChars | - | char[] | The set of base-64 characters to use. Should have length 64 with unique characters from the 7-bit ASCII range. | 

The class has two static readonly char arrays:

| Constant name   | Description |
| --------------- | ----------- |
| BASE64_STANDARD | The standard base-64 character set. |
| BASE64_URL      | A URL-safe base-64 character set. See [RFC4648](https://tools.ietf.org/html/rfc4648#page-8) |

