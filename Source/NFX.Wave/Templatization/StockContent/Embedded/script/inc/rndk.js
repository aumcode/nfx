    // Generates random key with specified length from the alphabet of possible characters: rndKey(10,"abcdefzq2")
    published.genRndKey = function(keyLen, alphabet) {
      var key = "";
      if (!published.intValidPositive(keyLen)) keyLen = 8;
      if (published.strEmpty(alphabet)) alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      while(key.length<keyLen)
        key += alphabet.charAt(published.rnd(alphabet.length));
      return key;
    };

    var _autoInc = {};
    //returns auto-inced named value: getAutoincKey('card', 1)
    published.genAutoincKey = function(seqName, num){
        var current = 0;
        if (published.strEmpty(seqName)) seqName = "Unspecified";
        if (!published.intValidPositive(num)) num = 1;

        if (seqName in _autoInc)
          current = _autoInc[seqName];

        var result = current;

        current += num;
        _autoInc[seqName] = current;
        return result;
    };

    // Returns random number in the range of min/max where min=0 max =100 by default:  rnd(10,57)
    published.rnd = function() {
      var min = 0;
      var max = 100;

      if (arguments.length === 1)  max = arguments[0];
      else if (arguments.length === 2)
      {
        min = arguments[0];
        max = arguments[1];
       }

      return min+Math.floor(Math.random()*(max-min+1));
    };