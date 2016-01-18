# NFX Wave.js
Java Script Client Library

## General-Purpose Functions

### Array Functions
* arrayDelete(object array, object element):bool  
Delete `element` from `array` and return true if `element` was deleted.
* arrayShallowCopy(object source): object  
Return new array - copy of `source` array.
* arrayClear(object array)  
Delete all elements from `array`.

### String Functions
* strEmpty(string s): bool  
Returns true if string is undefined, null or empty.
* strTrunc(string str, int maxLen, string endWith)  
Truncates str if its length exceeds maxLen and adds endWith string to result end.

## Record Model
Record model is in the WAVE.RecordModel namespace.

### Classes
* [Record](Record/readme.md)
* [Field](Field/readme.md)
* [RecordView](RecordView/readme.md)
