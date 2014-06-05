ManiaNet
========

TrackMania Dedicated Server Controller in C#/.NET

---------------------------------------------------------------------------------------------------------------------------------

Based on the code in the [ManiaPlanet Dedicated Server API](https://github.com/maniaplanet/dedicated-server-api/tree/master/libraries/Maniaplanet/DedicatedServer) in PHP,
the structure seems to be as follows:

###Initialization###

The dedicated server listens for TCP connections on port 5000 by default, and uses ASCII encodings (PHP only supports one-byte characters in normal strings) or UTF-8 (in the XML declaration).

When there's a connection it sends a 4 byte uint, which contains the number of bytes that follow and are to be read as characters, labeling the used protocol.

The two possible protocols are `GBXRemote 1` and `GBXRemote 2`, so the character count should always be 11.

###Calling Methods###

Method calls are send to the server in XML format.

For `GBXRemote 2` the XML declaration has to be preceded by 8 bytes, containing two uints for the length of the response and the handle respectively.
For `GBXRemote 1` it's only 4 bytes for the length.

For a parameterless method it would be (formatting added by me):

``` XML
<?xml version="1.0" encoding="utf-8" ?>
<methodCall>
	<methodName>methodName</methodName>
	<params></params>
</methodCall>
```

For a method with parameters, it would be (formatting added by me):

``` XML
<?xml version="1.0" encoding="utf-8" ?>
<methodCall>
	<methodName>Authenticate</methodName>
	<params>
		<param>
			<value>
				<string>SuperAdmin</string>
			</value>
		</param>
		<param>
			<value>
				<string>ManiaNet</string>
			</value>
		</param>
	</params>
</methodCall>
```

###Method Responses###

Method responses are send back in XML format.

For `GBXRemote 2` the XML declaration is preceded by 8 bytes, containing two uints for the length of the response and the handle respectively.
For `GBXRemote 1` it's only 4 bytes for the length.

---------------------------------------------------------------------------------------------------------------------------------

The value tag can contain one of the following tags:

* `<boolean>` containing a boolean, where `true` is `1` and `false` is `0`.

* `<int>` containing an integer number.

* `<double>` containing a float number.

* `<string>` containing a string, in which any special characters have been replaced by their HTML representation.

* `<array>` containing a `<data>` tag which contains `<value>` tags, which can contain any of the types in this list.

* `<struct>` containing a `<member>` tag which contains a `<name>` tag with the string name of the member, and a `<value>` tag, which can contain any of the types in this list.

* `<dateTime.iso8601>` containing a string representation of a date, formatted according to ISO-8601 `yyyymmddThh:mm:ss` (the T is a literal).

* `<base64>` containing data encoded into a base64 string.

---------------------------------------------------------------------------------------------------------------------------------

The list of methods (without description) can be found [here](https://github.com/Banane9/ManiaNet/blob/master/RPC-Method-List.md) and the official documentation is [here](http://maniaplanet.github.io/documentation//dedicated-server/methods/latest.html).

---------------------------------------------------------------------------------------------------------------------------------

##License##

#####[GPL V2](https://github.com/Banane9/ManiaNet/blob/master/LICENSE.md)#####
