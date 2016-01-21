# String functions

##### charIsAZLetterOrDigit(char c): bool
Returns true if `c` is `[a-zA-Z0-9]`.

##### intValid(string val): bool
Returns true if `val` is valid integer number.

##### intValidPositive(string val): bool
Returns true if `val` is valid positive integer number.

##### intValidPositiveOrZero(string val)
Returns true if `val` is valid positive integer number or zero.

##### siNum(number num, int decimalPlaces): string
Converts `num` to its string representation in SI (Le Systeme International d’Unites) with precision desired. `1000 = "1.00k", .1="100.00m", 23.55 = "23.55", 999.999="1.00k"`.

##### strCaps(string s, bool normalize): string
Capitalizes first chars after spaces or dots, optionally converting chars in between to lower case.

##### strContains(string str, string seg, bool considCase): bool
Returns true when `str` contains a `seg` optionally respecting case.

##### strDefault(string s, string dflt): string
If `s` is null or undefined then assigns `dflt` to it. If `dflt` is null or undefined then empty string.

##### strEmpty(string s): bool
Returns true if string is undefined, null or empty.

##### strEnsureEnding(string s, string ending): string
Ensures that string ends with the specified string: `strEnsureEnding("path",'/')`.

##### strOneOf(string str, array set, string del): bool
Returns true if the case-insensitive trimmed string is in the set of values. Neither string nor set value may contain delimiter which is '|' by default: `strOneOf("car", ["car", "house", "tax"], ';')`. 

##### strTrim / strLTrim / strRTrim(string s): string
Remove spaces from both/left/right sides of `s`.

##### strTrunc(string s, int maxLen, string endWith): string
Truncates `s` if its length exceeds `maxLen` and adds `endWith` string to result end.

##### strSame(string str1, string str2): bool
Returns true if both string contain the same trimmed case-insensitive value. This method is useful for tasks like searches of components by name.

##### strStartsWith / strEndsWith(string str, string seg, bool considCase): bool
Returns true if `str` starts/ends with `seg` optionally respecting case.

##### strEscapeHTML(content): string
Replace html escape characters in `content` with special entities: `& --> &amp;`.

##### strHTMLTemplate(string tpl, object args): string
Returns content like `'<td>@name@</td>' --> '<td>Alex &amp; Helen</td>'` provided that `args = {name: 'Alex & Helen'}`. Data is HTML escaped.

##### strTemplate(string tpl, object args): string
Returns content like `'{a: "@name@"}' -> '{a: "Alex & Boris"}'` provided that `args = {name: 'Alex & Boris'}`. Data is not HTML escaped.

##### strIsEMail(string str): bool
Returns true if `str` is valid email like `a@bc.de`

##### isValidScreenNameLetter(char c): bool
Returns true if `c` is allowable screen symbol including diacritic.

##### isValidScreenNameLetterOrDigit(char c): bool
Returns true if `c` is number (0-9) or allowable screen symbol including diacritic.

##### isValidScreenNameSeparator(char c): bool
Returns true if `c` is one from following set: ['.', '-', '_'].

##### strIsScreenName(string s): bool
Returns true if `s` contains valid screen name/ID.

##### strNormalizeUSPhone(string s)
Normalizes US phone string so it looks like (999) 999-9999x9999. If `s` starts with '+'(international phone, just return as-is).

##### strLocalize(string iso, string schema, string fld, string val): string
Localizes string per supplied lang iso code within schema/field.

##### toISODateTimeString(datetime dt): string
Returns '`dt` as string in ISO format (e.g. "2012-01-22T12:30:15.120Z").

##### toUSDateTimeString(datetime dt): string
Returns `dt` as string in "MM/DD/YYYY HH:MM:SS" format.

##### toUSDateString(datetime dt): string
Returns `dt` as string in "MM/DD/YYYY" format.

##### toSeconds(string dur): int
Parses duration string to total seconds. Example of duration string: "1d 10h 7m 13s".

##### genRndKey(int keyLen, string alphabet): string
Generates random key with specified length from the alphabet of possible characters: `rndKey(10,"abcdefzq2")`.

##### genAutoincKey(string seq, int val)
Returns next value of named sequence as result of: previous value (0 if function was not called for this sequence) + val.

##### isScalar(val): bool
Returns true for scalar vars and false for arrays and objects.

##### rnd(int min, int max): int
Returns random number in the range of min/max where min=0 max=100 by default: `rnd(10,57); rnd(30);`

##### id(string s): object
`document.getElementById(s)`.

##### getCookie(string name): string
Returns cookie by name or false if not found;

##### setCookie(string name, string value)
Set cookie as `name = escape(value) + "; path=/"`.

##### deleteCookie(string name)
Set expires as -1.

##### Checking types
Returns true if `tp` is in:
* **isObjectType(tp)** - ["object", "json", "map", "array"]
* **isIntType(tp)** - ["int", "integer"]
* **isRealType(tp)** - ["float", "real", "double", "money"]
* **isBoolType(tp)** - ["bool", "boolean", "logical"]
* **isStringType(tp)** - ["str", "string", "char[]", "char", "varchar", "text"]
* **isDateType(tp)** - ["date", "datetime", "time", "timestamp"]

##### convertScalarType(bool nullable, scalar value, string type, scalar dflt)
Converts scalar value into the specified type:
```js
var v;
v = convertScalarType(false, 13, "string", "<unconvertible>"); // v = '13'
v = convertScalarType(true, "13", "int");       // v = 13
v = convertScalarType(false, "abc", "int", 10); // v = 10
```