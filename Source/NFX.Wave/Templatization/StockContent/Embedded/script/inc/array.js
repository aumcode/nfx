published.arrayDelete = function(array, element) {
  for (var i in array){
    if (array[i] === element){
      array.splice(i, 1);
      return true;
    }
  }
    return false;
};

published.arrayShallowCopy = function(source){
    var copy = [];
    if (source && source.length>0)
    for (var i=0, max=source.length; i < max; i++) copy.push(source[i]);
    return copy;
};

published.arrayClear = function(array) {  while(array.length > 0) array.pop(); };

published.mergeArrays = function(left, right, matcher, transform) {
  if(!published.isArray(left) && !published.isArray(right)) return [];

  var m = published.isFunction(matcher) ? matcher : function(a, b) { return a === b; };
  var t = published.isFunction(transform) ? transform : function(a){ return a; };

  if(!published.isArray(left)) left = right;
  if(!published.isArray(right)) right = left;

  var a = left.concat(right);
  for(var i = 0; i < a.length; i++) {
    for(var j = i + 1; j < a.length; j++) {
      if (m(a[i],a[j]))
        a.splice(j--, 1);
    }
    a[i] = t(a[i]);
  }
  return a;
};

published.inArray = Array.prototype.indexOf ?
                    function(array, value) { return array.indexOf(value) !== -1;} :
                    function(array, value) {
                        var i = array.length;
                        while (i--) if (array[i] === value) return true;
                        return false;
                    };