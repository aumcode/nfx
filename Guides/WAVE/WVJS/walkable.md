# WAVE.Walkable
Mixin that enables function chaining that facilitates lazy evaluation via lambda-functions.

## Projection Operations
##### wSelect(transform)
Projects values that are based on a transform function.
```js
var a = [1, 2, 3]; 
var aw = WAVE.arrayWalkable(a);
var wSelect = aw.wSelect(function(e) { return e * 10; });
var result = wSelect.wToArray(); // [10, 20, 30]
```

##### wSelectMany(transform)
Projects sequences of values that are based on a transform function and then flattens them into one sequence.
```js
var a0 = [1, 2];
var a1 = [3, 4];

var aa = [WAVE.arrayWalkable(a0), WAVE.arrayWalkable(a1)];
var aw = WAVE.arrayWalkable(aa);
var result = aw.wSelectMany(function (e) { return e; });

// result.wAt(0) = 1
// result.wAt(1) = 2
// result.wAt(2) = 3
// result.wAt(3) = 4
```

## Filtering Data
##### wWhere(filter)
Selects values that are based on a predicate function `filter`. 
```js
var a = [1, 2, 3, 4, 5]; 
var aw = WAVE.arrayWalkable(a);
var whereWalkable = aw.wWhere(function(e) { return e > 3; });
var resultArray = whereWalkable.wToArray(); // [4, 5]
```


## Element Operations
##### wAt(int pos)
Returns the element at a specified index in a walkable collection.

#### wEach(action)
Does `action` for each element in collection.

##### wFirst(condition)
Returns the first element of a collection, or the first element that satisfies a condition.
```js
var a = [1, 2, 3]; 
var aw = WAVE.arrayWalkable(a);
var first;

first = aw.wFirst(); // 1
first = aw.wFirst(function(e) { return e > 2; }); // 3
first = aw.wFirst(function(e) { return e < 0; }); // null

aw = WAVE.arrayWalkable([]);
var cnt = aw.wFirst(); // null
```

##### wFirstIdx(condition)
Returns an index of element that satisfies a condition.
```js
var a = [1, 2, 3]; 
var aw = WAVE.arrayWalkable(a);
var idx;
idx = aw.wFirstIdx(function(e) { return e == 0; }); // -1
idx = aw.wFirstIdx(function(e) { return e == 1; }); // 0
```


## Aggregation Operations
##### wAggregate(aggregate, initVal)
Performs a custom aggregation operation on the values of a collection.

##### wCount(condition)
Counts the elements in a collection, optionally only those elements that satisfy a predicate function.

##### wMax(greater)
Determines the maximum value in a collection.

##### wMin(less)
Determines the minimum value in a collection.

##### wSum(initVal)
Calculates the sum of the values in a collection.


## Set Operations
#### wConcat(other)
Concatenates two sequences to form one sequence.
```js
var aw = WAVE.arrayWalkable([1, 2, 3]);
var bw = WAVE.arrayWalkable([5, -4]);
var ct = aw.wConcat(bw).wToArray(); // [1, 2, 3, 5, -4]
```

##### wDistinct(equal)
Returns distinct elements from a sequence by using the default equality comparer or `equal` to compare values.
```js
var a = [1, 2, 2, 3, 5, 4, 2, 4, 3]; 
var aw = WAVE.arrayWalkable(a);
var distinct = aw.wDistinct();
var result = distinct.wToArray(); // [1, 2, 3, 5, 4]
```
```js
var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 2, y: 1}, {x: 3, y: 1}, {x: 4, y: 3}];
var aw = WAVE.arrayWalkable(a);
var distinct = aw.wDistinct(function(a, b) { return a.y == b.y; });
var result = distinct.wToArray(); // [{x: 0, y: 0}, {x: 1, y: 1}, {x: 4, y: 3}]
```

##### wExcept(other, equal)
Produces the set difference of two sequences by using the default equality comparer or `equal`.
```js
var a = [1, 2, 3];
var b = [2];
var aw = WAVE.arrayWalkable(a);
var bw = WAVE.arrayWalkable(b);
var except = aw.wExcept(bw).wToArray(); // [1, 3]
```

#### wEqual(other, equalFunc)
Determines whether two sequences are equal by comparing elements with `equalFunc`(optional).

## Grouping Data
##### wGroup(keyer, valuer)
Groups elements that share a common attribute. Result has `k` (key) and `v` (values) properties.
```js
var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 0, y: 33}, {x: 0, y: 34}, {x: 1, y: -5}, {x: 0, y: 127}];

var group = WAVE.arrayWalkable(a).wGroup(function(e) { return e.x; }, function(e) { return e.y; }).wGroupIntoArray();
/*
group[0].k = 0
group[0].v.length = 4
group[0].v[0] = 0
group[0].v[1] = 33
group[0].v[2] = 34
group[0].v[3] = 127

group[1].k = 1
group[1].v.length = 2
group[1].v[0] = 1
group[1].v[1] = -5
*/              
```

##### wGroupAggregate(aggregator, initVal)
Performs a custom aggregation operation on the elements in groups.
```js
var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 0, y: 33}, {x: 0, y: 34}, {x: 1, y: -5}, {x: 0, y: 127}];

var group = WAVE.arrayWalkable(a).wGroup(function(e) { return e.x; }, function(e) { return e.y; }).wGroupAggregate(function(k, r, e) { return (r || 0) + e });

// group.wAt(0).v = 194
// group.wAt(1).v = -4
```

##### wGroupIntoArray()
Converts the result of grouping to array of objects like this: `{k: key, v: [values]}`

##### wGroupIntoObject()
Converts the result of grouping to object like this: `obj[key] = [values]`.

## Sorting Data
##### wOrder(order)
Sorts elements in according to `order`.
```js
var a = [1, -3, 2, 17, -9, 4, 5];
var ordered = WAVE.arrayWalkable(a).wOrder(function(a, b) { return a < b ? 1 : a > b ? -1 : 0; }).wToArray();
// ordered = [17, 5, 4, 2, 1, -3, -9]
```

## Partitioning Data
##### wTake(quantity)
Takes `quantity` elements from the head of collection.

##### wTakeWhile(condition)
Takes elements from the head of collection while `condition` is true.

##### wSkip(quantity)
Skips `quantity` elements from the head of collection.

## Quantifier Operations
##### wAny(condition)
Determines whether any elements in a sequence satisfy a condition.

##### wAll(condition)
Determines whether all the elements in a sequence satisfy a condition.


## Converting Data Types
#### wToArray()
Converts a collection to an array.

## Special Operations
#### wHarmonicFrequencyFilter(m, k)
Frequency filter takes:  
* "m" (f=ma Newtons law) -  in virtual units of mass (i.e. kilograms) - the higher the number the more attenuation for high frequencies,
* "k" (f=-kx Hooks law) - in virtual units of spring membrane resistance, the higher the number the more attenuation for low frequencies.

##### wConstLinearSampling(samplingStep)
Ensures that all {s, a} vectors in incoming walkable are equally spaced in time, so {a} component linearly interpolated between otherwise discrete points, i.e. [ {1, -47} , {10, 150}] => F(2) => yields vectors where respected components (0 -> 5, -47 -> 150) get linearly pro-rated.
```js
var w = WAVE.arrayWalkable([{s: 1, a: 10}, {s: 10, a: 700}]).wConstLinearSampling(2);
w.wAt(0).s = 0
w.wAt(0).a = 10
```

#### wSineGen / wSawGen / wSquareGen / wRandomGen(cfg)
Outputs {s,a} vectors per SINE/"Saw"/"Square"/Random function.

#### wWMA(k)
"weight"-based moving average filter (k is kind of "weight" 0..1):  
* if k == 0 - moving object has no weight (hence no momentum) and outbound signal is the same as inbound,
* if k == 1 - moving object has infinite weight (hence infinity momentum) and outbound signal will be permanent (equal to first sample amplitude).
```js
var aw = WAVE.arrayWalkable([{s: 0, a: 1}, {s: 1, a: 100}, {s: 2, a: -100}]);
var wma = aw.wWMA(0.1);
// wma.wAt(0).a = 1
// wma.wAt(1).a = 90.1
// wma.wAt(2).a = -80.99
```

##### WAVE.signalConstSrc(cfg)
 Walkable source of constant signal: `WAVE.signalConstSrc({a: 17.75, qty: 2})`.
 