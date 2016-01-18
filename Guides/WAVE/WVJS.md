# NFX Wave.js
Java Script Client Library

## General-Purpose Functions
### String Functions
* bool strEmpty(string s)  
Returns true if string is undefined, null or empty.

## Record Model
Record model is in the WAVE.RecordModel namespace.

### [Record class](Record/readme.md)
The purpose of this class is to represent server-side NFX.DataAccess.CRUD.Schema on the client side along with view model/controller.
Record instances are initialized using server NFX.Wave.Client.RecordModelGenerator class that turns Schema into JSON object suitable for record initialization on client.
