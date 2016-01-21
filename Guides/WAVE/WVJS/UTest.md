# WAVE.UTest
Static class which implements unit testing concept.

##### run(string area, string name, function fn)
Wrapper for testing statements which placed in `fn`.
```js
run("Assertions", "assertTrue", function(){
  assertTrue(true);
  log("This must pass 1");
});
```

##### testingStarted()
Returns true to indicate that testing has been activated.

##### log(string msg)
Writes log message in new line.

##### logi(string msg)
Writes log message inline.

##### assertTrue(bool assertion, string msg)
If bool statement `assertion` is not true, then throws exception with `msg` ("Assertion not true" if empty).

##### assertFalse(bool assertion, string msg)
If bool statement `assertion` is not false, then throws exception with `msg` ("Assertion not false" if empty).
