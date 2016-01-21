# Operations with objects and functions

##### clone(object obj): object
Deep clones data object (not functions).

##### extend(object obj, object ext, bool keepExisting): object
Mixin behavior - extends `obj` with properties of `ext`.
If flag `keepExisting=true` then existing object key is preserved, even if it is null.

##### isArray(object obj): bool
Returns true when the passed parameter is an array, not a map or function.

##### isFunction(object obj): bool
Returns true when the passed parameter is a function, not a map object or an array.

##### isMapOrArray(obj): bool
Returns true when the passed parameter is an array, or map but not a function.

##### isObject(object obj): bool
Returns true when the passed parameter is a map, not an array or function.

##### isSame(object obj1, object obj2): bool
Returns true if both objects represent the same scalar value or complex structure/map that is keys/values of maps/arrays. Nulls are considered equivalent.

##### overrideFunction(function original, function fn): function
Overrides existing function by wrapping in new one. May call base like so:
```js
object.about = WAVE.overrideFun(object.about, function(){ return this.baseFunction() + "overridden" });
```

##### propStrAsObject(object obj, string prop)
Checks object property for string value and if it is then converts it to object (map).
Does nothing if prop does not exist, is null or not a string value.
```js
var o1 = {a: 1, b: '{"c": "yes", "d": "no"}'};
// WAVE.isObject(o1.b)) = false

WAVE.propStrAsObject(o1, "b");
// WAVE.isObject(o1.b)) = true
```
 