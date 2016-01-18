# NFX Wave.js
Java Script Client Library

## General-Purpose Functions
### String Functions
#### strEmpty(string)
Returns true if string is undefined, null or empty

## Record Model
Record model is in the WAVE.RecordModel namespace.

### Record class
The purpose of this class is to represent server-side NFX.DataAccess.CRUD.Schema on the client side along with view model/controller.
Record instances are initialized using server NFX.Wave.Client.RecordModelGenerator class that turns Schema into JSON object suitable for record initialization on client.
* [.ctor()](Record/record.md)
* [allValidationErrorStrings()](Record/allValidationErrorStrings.md)
* [data()](Record/data.md)