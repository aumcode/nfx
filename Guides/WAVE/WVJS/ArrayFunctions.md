# Array functions

##### arrayClear(array)
Delete all elements from `array`.

##### arrayDelete(array, object element): bool
Delete `element` from `array` and return true if `element` was found.

##### arrayShallowCopy(source_array): object
Return new array - copy of `source_array`.

##### arrayWalkable(array)
Provides walking (see WAVE.Walkable) capability for array.

##### groupWalkable(array)
Converts array with {k : {}, v: []} structure to Walkable group operation result (inverse method to Walkable.wGroupToArray).

##### inArray(array, object element): bool
Returns true when `array` contains `element`.
