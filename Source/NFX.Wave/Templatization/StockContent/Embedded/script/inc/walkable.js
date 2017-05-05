    // Mixin that enables function chaining that facilitates lazy evaluation via lambda-funcs
    published.Walkable =
    {
        wSelect: function(selector) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        var curr;
                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() {
                            if (!walker.moveNext()) return false;
                            curr = selector(walker.current());
                            return true;
                          },
                          current: function() { return curr; }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wSelect

        wSelectMany: function (e2Walkable) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function () {
                        var walker = srcWalkable.getWalker();
                        var currWalker, curr;
                        return {
                          reset: function () {
                            walker.reset();
                            while (walker.moveNext()) {
                              var walkerCurrent = walker.current();
                              if (walkerCurrent === null) continue;
                              var eWalkable = e2Walkable ? e2Walkable(walkerCurrent) : walkerCurrent;
                              currWalker = eWalkable.getWalker();
                              break;
                            }
                          },

                          moveNext: function () {
                            if (currWalker && currWalker.moveNext()) {
                              curr = currWalker.current();
                              return true;
                            }
                            while (walker.moveNext()) {
                              var walkerCurrent = walker.current();
                              if (walkerCurrent === null) continue;
                              var eWalkable = e2Walkable ? e2Walkable(walkerCurrent) : walkerCurrent;
                              currWalker = eWalkable.getWalker();
                              if (currWalker.moveNext()) {
                                curr = currWalker.current();
                                return true;
                              }
                            }
                            return false;
                          },

                          current: function () { return curr; }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;

        }, //wSelectMany

        wWhere: function(filter) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() {
                            while (walker.moveNext()) {
                              var has = filter(walker.current());
                              if (has) return true;
                            }
                            return false;
                          },
                          current: function() { return walker.current(); }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wWhere

        wAt: function(pos) {
              var walker = this.getWalker();
              var idx = -1;
              while(walker.moveNext()) {
                idx++;
                if (idx === pos) return walker.current();
              }

              return null;
        }, //wIdx

        wFirst: function(condition) {
                  var walker = this.getWalker();
                  while(walker.moveNext())
                    if (condition) {
                      if (condition(walker.current()))
                        return walker.current();
                    } else {
                      return walker.current();
                    }

                  return null;
        }, //wFirst

        wFirstIdx: function(condition) {
                    var walker = this.getWalker();
                    var idx = -1;
                    while(walker.moveNext()) {
                      idx++;
                      if (condition(walker.current())) return idx;
                    }
                    return -1;
        }, //wFirstIdx

        wCount: function(condition) {
                var walker = this.getWalker();
                var cnt = 0;
                while(walker.moveNext())
                  if (condition){
                    if (condition(walker.current()))
                      cnt++;
                  } else {
                    cnt++;
                  }
                return cnt;
        }, //wCount

        wDistinct: function(equal) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        function WW()
                        {
                          var existing = [], existingWalkable = WAVE.arrayWalkable(existing);
                          this.reset = function() { walker.reset(); existing.splice(0, existing.length); };

                          function filter(e){
                            if (equal)
                                return equal(e, walker.current());
                            else
                                return e === walker.current();
                          }


                          this.moveNext = function() {
                            while (walker.moveNext()) {
                              if (existingWalkable.wFirstIdx(filter) === -1)
                              {
                                existing.push(walker.current());
                                return true;
                              }
                            }
                            return false;
                          };
                          this.current = function() { return walker.current(); };
                        }
                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wDistinct

        wConcat: function(other) {
          var srcWalkable = this;

          var walkable = {
            getWalker: function() {
              var walker = srcWalkable.getWalker();
              var walkerOther = null;
              return {
                reset: function() { walker.reset(); walkerOther = null; },
                moveNext: function() {
                  if (walkerOther === null) {
                    if (walker.moveNext()) {
                      return true;
                    } else {
                      walkerOther = other.getWalker();
                    }
                  }
                  return walkerOther.moveNext();
                },
                current: function() { return walkerOther === null ? walker.current() : walkerOther.current(); }
              };
            }
          }; //wConcat

          WAVE.extend(walkable, WAVE.Walkable);
          return walkable;
        },

        wExcept: function(other, equal) {
                    var srcWalkable = this;

                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();

                        function filter(e){
                            if (equal)
                                return equal(walker.current(), e);
                            else
                                return e === walker.current();
                        }

                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() {
                            while (walker.moveNext()) {
                              if (other.wFirstIdx(filter) === -1)
                              {
                                return true;
                              }
                            }
                            return false;
                          },
                          current: function() { return walker.current(); }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        },//wExcept

        wGroup: function(keyer, valuer) {
                    var srcWalkable=this;
                    var walkable = {
                      getWalker: function() {
                        function WW() {
                          var distinct = srcWalkable.wDistinct(function (a, b) { return keyer(a) === keyer(b); });
                          var distinctWalker = distinct.getWalker();

                          this.reset = function() { distinctWalker.reset(); };
                          this.moveNext = function() { return distinctWalker.moveNext(); };
                          this.current = function() {
                                           return {
                                                    k: keyer(distinctWalker.current()),
                                                    v: srcWalkable.wWhere(function(e) { return keyer(e) === keyer(distinctWalker.current());}).wSelect(function(e) { return valuer(e); })
                                                  };
                                         };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wGroup

        wGroupIntoObject: function() {
                var obj = {};
                this.wEach(function(e) {   obj[e.k] = e.v.wToArray();   });
                return obj;
        }, //wGroupIntoObject

        wGroupIntoArray: function(keyer, valuer) {
                var arr = [];
                this.wEach(function(e) { arr.push({k: e.k, v: e.v.wToArray()}); } );
                return arr;
        }, //wGroupIntoArray

        wGroupAggregate: function(aggregator, initVal) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() { return walker.moveNext(); },
                          current: function() {
                            var cur = walker.current();
                            return {
                              k: cur.k,
                              v: cur.v.wAggregate( function(r, e) { return aggregator(cur.k, r, e); })
                            };
                          }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wGroupAggregate

        wOrder: function(order) {
                var arr = this.wToArray();
                if (!order) order = function(a, b) { return a < b ? -1 : a > b ? 1 : 0; };
                arr.sort(order);
                return WAVE.arrayWalkable(arr);
        }, //wOrder

        wTake: function(qty) {
                    var srcWalkable=this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        function WW() {
                          var taken = 0;
                          this.reset = function() { walker.reset(); taken = 0; };
                          this.moveNext = function() {
                            var has = walker.moveNext() && taken < qty;
                            if (!has) return false;
                            taken++;
                            return true;
                          };
                          this.current = function() { return walker.current(); };
                        }
                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wTake

        wTakeWhile: function(cond) {
                    var srcWalkable=this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() {
                            if (!walker.moveNext()) return false;
                            if (!cond(walker.current())) return false;
                            return true;
                          },
                          current: function() { return walker.current(); }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wTakeWhile

        wSkip: function(qty) {
                    var srcWalkable=this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();

                        function WW() {
                          var idx = -1;
                          this.reset = function() { walker.reset(); idx = -1; };
                          this.moveNext = function() {
                            while(walker.moveNext()) {
                              idx++;
                              if (idx >= qty) return true;
                            }
                            return false;
                          };
                          this.current = function() { return walker.current(); };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wSkip

        wAny: function(condition) {
                    var walkable = this;
                    var walker = walkable.getWalker();
                    if (!condition) return walker.moveNext();

                    while(walker.moveNext()) if (condition(walker.current())) return true;

                    return false;
        }, //wAny

        wAll: function(condition) {
                    var walkable = this;
                    var walker = walkable.getWalker();
                    if (!condition) return !walker.moveNext();

                    while(walker.moveNext()) if (!condition(walker.current())) return false;

                    return true;
        }, //wAll

        wMin: function(less) {
                    if (!less) less = function(a, b) { return a < b; };

                    var walker = this.getWalker();

                    if (!walker.moveNext()) return null;

                    var minVal = walker.current();

                    while(walker.moveNext()) if (less(walker.current(), minVal)) minVal = walker.current();

                    return minVal;
        }, //wMin

        wMax: function(greater) {
                    if (!greater) greater = function(a, b) { return a > b; };

                    var walker = this.getWalker();

                    if (!walker.moveNext()) return null;

                    var maxVal = walker.current();

                    while(walker.moveNext()) if (greater(walker.current(), maxVal)) maxVal = walker.current();

                    return maxVal;
        }, //wMax

        wAggregate: function(aggregate, initVal) {
                      var r = initVal;
                      this.wEach(function(e) { r = aggregate(r, e); });
                      return r;
        }, //wAggregate

        wSum: function(initVal) {
                      if (typeof(initVal) === tUNDEFINED) initVal = 0;
                      return this.wAggregate(function(r, e) { return r + e; }, initVal);
        }, //wSum

        wEqual: function(other, equalFunc) {
          var selfWalker = this.getWalker();
          var otherWalker = other.getWalker();
          do {
            var selfHasValue = selfWalker.moveNext();
            var otherHasValue = otherWalker.moveNext();

            if (!selfHasValue && !otherHasValue) return true;
            if (selfHasValue !== otherHasValue) return false;

            var selfEl = selfWalker.current();
            var otherEl = otherWalker.current();

            if (!equalFunc) {
              if (selfEl !== otherEl) return false;
            } else {
              if (!equalFunc(selfEl, otherEl)) return false;
            }

          } while(true);
        }, //wEqual

        wToArray: function() {
                    var walkable = this;
                    var walker = walkable.getWalker();
                    var arr = [];
                    while(walker.moveNext()) arr.push(walker.current());

                    return arr;
        }, //wToArray

        wEach: function(action) {
                    var walkable = this;
                    var walker = walkable.getWalker();

                    var i = 0;
                    while(walker.moveNext()) action(walker.current(), i++);

                    return walkable;
        },  //wEach

        // "weight"-based moving average filter (k is kind of "weight" 0..1):
        // if k == 0 - moving object has no weight (hense no momentum) and outbound signal is the same as inbound,
        // if k == 1 - moving object has infinite weight (hense infinity momentum)
        // and outbound signal will be permanent (equal to first sample amplitude)
        wWMA: function(k) {
                    var srcWalkable = this;
                    if (typeof(k) === tUNDEFINED) k = 0.5;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        var prevOutA = null; // previous oubound (processed) amplitude
                        var currOutS = {s: -1, a: 0}; // current outbound (processed) sample

                        return {
                          reset: function() {
                            prevOutA = null;
                            currOutS = {s: -1, a: 0};
                            walker.reset();
                          },

                          moveNext: function() {
                            if (!walker.moveNext()) return false;

                            currOutS.s = walker.current().s;
                            var inA = walker.current().a;

                            if (currOutS.s === 0) {
                              currOutS.a = inA;
                            } else {
                              currOutS.a = (1-k) * inA + k * prevOutA;
                            }

                            prevOutA = currOutS.a;

                            return true;
                          },

                          current: function() { return currOutS; },

                          samplingRate: function() { return walker.samplingRate ? walker.samplingRate() : 2; }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wWMA

        // Frequency filter takes:
        //  "m"(f=ma Newtons law) -  in virtual units of mass (i.e. kilograms) - the higher the number the more attenuation for high frequencies
        //  "k"(f=-kx Hooks law) - in virtual units of spring mimbrane resistance, the higher the number the more attenuation for low frequencies
        wHarmonicFrequencyFilter: function(m,k) {
                    var srcWalkable = this;
                    if (typeof(m) === tUNDEFINED) m = 1;
                    if (typeof(k) === tUNDEFINED) k = 1;
                    if (m<1.0001) m = 1.0001;
                    if (k<0.0001) k = 0.0001;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        var prevOutA = 0; // previous oubound (processed) amplitude
                        var currOutS = {s: -1, a: 0}; // current outbound (processed) sample
                        var v = 0;
                        var prevInA = 0;

                        return {
                          reset: function() {
                            prevOutA = 0;
                            v = 0;
                            prevInA = 0;
                            currOutS = {s: -1, a: 0};
                            walker.reset();
                          },

                          moveNext: function() {
                            if (!walker.moveNext()) return false;

                            currOutS.s = walker.current().s;
                            var inA = walker.current().a;


                            var da = inA - prevInA;
                            prevInA = inA;

                            v += (da - (k * prevOutA)) / m;

                            //console.log(da, v);

                            v*=0.98;//active resistance

                            currOutS.a = prevOutA + v;
                            prevOutA = currOutS.a;

                            return true;
                          },

                          current: function() { return currOutS; },

                          samplingRate: function() { return walker.samplingRate ? walker.samplingRate() : 2; }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wHarmonicFrequencyFilter

        //SA vector definition
        // {s: int, a: float}
        // [s]ample = monotonously increasing integer value
        // [a]mplitude = floating point, the amplitude of the signal

        //Ensures that all {s, a} vectors in incoming walkable are equally spaced in time, so {a} component linearly interpolated between
        //otherwise descret points, i.e. [ {1, -47} , {10, 150} ] => F(2) => yields vectors where
        // respected components (0 -> 5, -47 -> 150)  get linearly pro-rated
        wConstLinearSampling: function(samplingStep) {
                                var srcWalkable = this;
                                var walkable = {
                                  getWalker: function() {
                                    samplingStep = samplingStep || 1;
                                    var walker = srcWalkable.getWalker();
                                    var walkerExausted = false, inSampleLastS = null;
                                    var halfSamplingStep = samplingStep / 2;
                                    var outSampleIdx = 0; // index sample number (natural integer)
                                    var outSampleT; // t of current sample (float point)
                                    var outSampleLeftBorder, outSampleRightBorder; // left and right borders of this sample (float point)
                                    var inSamplesBuf = [];
                                    var curr = {};

                                    function WW() {
                                      this.reset = function() {
                                        inSamplesBuf.splice(0, inSamplesBuf.length);
                                        outSampleIdx = 0;
                                        walkerExausted = false; inSampleLastS = null;
                                        walker.reset();
                                      };

                                      this.moveNext = function() {

                                        function cutBufFromLeft(left, right) {
                                          while(inSamplesBuf.length > 1 && inSamplesBuf[0].s < left && inSamplesBuf[1].s < right) inSamplesBuf.shift();
                                        }

                                        function hasBufNeighbourhoodSamples(left, right) {
                                          for(var i=0; i<inSamplesBuf.length; i++) {
                                            var inSampleS = inSamplesBuf[i].s;
                                            if (inSampleS > left && inSampleS <= right) return true;
                                          }
                                          return false;
                                        }

                                        function walkerMoveNext() {
                                          if (walker.moveNext()) {
                                            inSampleLastS = walker.current().s;
                                            return true;
                                          } else {
                                            walkerExausted = true;
                                            return false;
                                          }
                                        }

                                        function avgNeighbourhoodSamples(left, right) {
                                          var sum = 0, sumQty = 0;
                                          for(var i=0; i<inSamplesBuf.length; i++) {
                                            var inSample = inSamplesBuf[i];
                                            if (inSample.s > left && inSample.s <= right) {
                                              sum += inSample.a;
                                              sumQty++;
                                            }
                                          }
                                          var avg = sum / sumQty;
                                          return avg;
                                        }

                                        function fillBuf(right) {
                                          while(walkerMoveNext()) {
                                            if (walker.current().s >= right) {
                                              inSamplesBuf.push(walker.current());
                                              break;
                                            } else {
                                              inSamplesBuf.push(walker.current());
                                            }
                                          }
                                        }

                                        if (!outSampleIdx) { // first iteration
                                          if (!walkerMoveNext()) return false;

                                          inSamplesBuf.push(walker.current());

                                          outSampleT = walker.current().s;
                                          outSampleLeftBorder = outSampleT - halfSamplingStep;
                                          outSampleRightBorder = outSampleT + halfSamplingStep;

                                          fillBuf(outSampleRightBorder);

                                          curr.s = outSampleIdx++;
                                          var avg = avgNeighbourhoodSamples(-Number.MAX_VALUE, outSampleRightBorder);
                                          curr.a = avg;

                                          return true;
                                        }

                                        outSampleT += samplingStep;
                                        outSampleLeftBorder = outSampleT - halfSamplingStep;
                                        outSampleRightBorder = outSampleT + halfSamplingStep;

                                        if (walkerExausted && inSampleLastS < outSampleLeftBorder) return false;

                                        curr.s = outSampleIdx++;

                                        cutBufFromLeft(outSampleLeftBorder, outSampleRightBorder);

                                        if (hasBufNeighbourhoodSamples(outSampleLeftBorder, outSampleRightBorder)) {
                                          curr.a = avgNeighbourhoodSamples(outSampleLeftBorder, outSampleRightBorder);
                                          return true;
                                        }

                                        fillBuf(outSampleRightBorder);

                                        if (hasBufNeighbourhoodSamples(outSampleLeftBorder, outSampleRightBorder)) {
                                          curr.a = avgNeighbourhoodSamples(outSampleLeftBorder, outSampleRightBorder);
                                          return true;
                                        }

                                        var inSampleLeft = inSamplesBuf[0];
                                        var inSampleRight = inSamplesBuf[1];

                                        var k = (inSampleRight.a - inSampleLeft.a) / (inSampleRight.s - inSampleLeft.s);
                                        var a = inSampleLeft.a - k * inSampleLeft.s;

                                        curr.a = k * outSampleT + a;

                                        return true;
                                      };

                                      this.current = function() {
                                        return curr;//{int sample, float Amplitude}=> {s,a}
                                      };
                                    }

                                    return new WW();
                                  }
                                };

                                WAVE.extend(walkable, WAVE.Walkable);
                                return walkable;
        }, //wConstLinearSampling

        //Outputs {s,a} vectors per SINE function
        wSineGen: function(cfg) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        cfg = cfg || {};
                        var f = cfg.f || 1.0; // Frequency in Hertz assuming that sampling rate is / second
                        var a = typeof(cfg.a) === tUNDEFINED ? 1 : cfg.a; // Amplitude
                        var d = cfg.d || 0; // DC Offset
                        var r = cfg.r || (walker.samplingRate ? walker.samplingRate() : 2); // Sampling Rate in arbitrary units (i.e. seconds)
                        var p = cfg.p || 0; // Phase offset in radians

                        if (r<2) r = 2;//Nyquist frequency

                        var deltaPeriod = (pi2 / r) * f; // the radian cost of 1 period of sine at supplied sampling rate multiplied by desired frequency

                        var sampleIdx = 0; // sample idx (sequental #)
                        var period = p; // current argument
                        var curr = {}; // current sample vector {s,a}

                        walker = srcWalkable.getWalker();
                        function WW() {
                          this.reset = function() { sampleIdx = 0; period = p; walker.reset(); };
                          this.moveNext = function() {
                            if (!walker.moveNext()) return false;
                            curr.s = sampleIdx++;
                            curr.a = (a * Math.sin(period)) + d + walker.current().a;
                            period = geometry.wrapAngle(period, deltaPeriod);
                            return true;
                          };
                          this.current = function() {
                            return curr;//{int sample, float Amplitude}=> {s,a}
                          };

                          this.samplingRate = function() { return r; };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        },

        //Outputs {s,a} vectors per "Saw" function
        wSawGen: function(cfg) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        cfg = cfg || {};
                        var f = cfg.f || 1.0; // Frequency in Hertz assuming that sampling rate is per second
                        var a = typeof(cfg.a) === tUNDEFINED ? 1 : cfg.a; // Amplitude
                        var d = cfg.d || 0; // DC Offset
                        var r = cfg.r || (walker.samplingRate ? walker.samplingRate() : 2); // Sampling Rate in arbitrary units (i.e. seconds)
                        var p = cfg.p || 0; // Phase offset in radians
                        var s = cfg.s || 0.9; // Symmetry (attack percentage (0..1))

                        if (r<2) r = 2;//Nyquist frequency

                        var k1 = 1 / pi / s;
                        var a1 = -1;

                        var k2 = 1 / pi / (s - 1);
                        var a2 = 1 - 2 * s / (s - 1);

                        var deltaPeriod = (pi2 / r) * f; // the radian cost of 1 period of saw at supplied sampling rate multiplied by desired frequency

                        var sampleIdx = 0; // sample idx (sequental #)
                        var period = p; // current argument
                        var curr = {}; // current sample vector {s,a}

                        walker = srcWalkable.getWalker();
                        function WW() {
                          this.reset = function() { sampleIdx = 0; period = p; walker.reset(); };
                          this.moveNext = function() {
                            if (!walker.moveNext()) return false;
                            curr.s = sampleIdx++;
                            var unitV;
                            if (period < pi2 * s)
                              unitV = k1 * period + a1;
                            else
                              unitV = k2 * period + a2;
                            curr.a = (a * unitV) + d + walker.current().a;
                            period = geometry.wrapAngle(period, deltaPeriod);
                            return true;
                          };

                          this.current = function() { return curr; };

                          this.samplingRate = function() { return r; };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;

        },

        //Outputs {s,a} vectors per "Square" function
        wSquareGen: function(cfg) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        cfg = cfg || {};
                        var f = cfg.f || 1.0; // Frequency in Hertz assuming that sampling rate is per second
                        var a = typeof(cfg.a) === tUNDEFINED ? 1 : cfg.a; // Amplitude
                        var d = cfg.d || 0; // DC Offset
                        var r = cfg.r || (walker.samplingRate ? walker.samplingRate() : 2); // Sampling Rate in arbitrary units (i.e. seconds)
                        var p = cfg.p || 0; // Phase offset in radians
                        var s = cfg.s || 0.9; // Symmetry (attack percentage (0..1))

                        if (r<2) r = 2;//Nyquist frequency

                        var k1 = -2 / pi / (1 - s);
                        var a1 = 1 - k1 * pi * s;

                        var k2 = 2 / pi / (1 - s);
                        var a2 = -1 - k2 * pi * (1 + s);

                        var deltaPeriod = (pi2 / r) * f; // the radian cost of 1 period of sine at supplied sampling rate multiplied by desired frequency

                        var sampleIdx = 0; // sample idx (sequental #)
                        var period = p; // current argument
                        var curr = {}; // current sample vector {s,a}

                        walker = srcWalkable.getWalker();
                        function WW() {
                          this.reset = function() { sampleIdx = 0; period = p; walker.reset(); };
                          this.moveNext = function() {
                            if (!walker.moveNext()) return false;
                            curr.s = sampleIdx++;
                            var unitV;
                            if(period < pi) {
                              if (period < pi * s) unitV = 1;
                              else unitV = k1 * period + a1;
                            } else {
                              if (period < pi + pi * s) unitV = -1;
                              else unitV = k2 * period + a2;
                            }
                            curr.a = (a * unitV) + d + walker.current().a;
                            period = geometry.wrapAngle(period, deltaPeriod);
                            return true;
                          };

                          this.current = function() { return curr; };

                          this.samplingRate = function() { return r; };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        },

        wRandomGen: function(cfg) {
                      var srcWalkable = this;
                      var walkable = {
                        getWalker: function() {
                          var walker = srcWalkable.getWalker();
                          cfg = cfg || {};
                          var a = typeof(cfg.a) === tUNDEFINED ? 1 : cfg.a; // Amplitude
                          var r = cfg.r || (srcWalkable.samplingRate ? srcWalkable.samplingRate() : 2); // Sampling Rate in arbitrary units (i.e. seconds)

                          if (r<2) r = 2;//Nyquist frequency

                          var sampleIdx = 0; // sample idx (sequental #)
                          var curr = {}; // current sample vector {s,a}

                          walker = srcWalkable.getWalker();

                          function WW() {
                            this.reset = function() { walker.reset(); sampleIdx = 0; };
                            this.moveNext = function() {
                              if (!walker.moveNext()) return false;
                              curr.s = sampleIdx++;
                              curr.a = walker.current().a + 2 * a * (Math.random() - 0.5);
                              return true;
                            };
                            this.current = function() { return curr; };
                            this.samplingRate = function() { return r; };
                          }

                          return new WW();
                        }
                      };

                      WAVE.extend(walkable, WAVE.Walkable);
                      return walkable;
        }
    };// Walkable

    // walkable source of constant signal
    published.signalConstSrc = function(conf) {

              function WW(cfg) {
                var a = cfg.a || 0; // Amplitude
                var qty = cfg.qty; // Values count limit (if no value infinite set of values are genereted)
                var r = cfg.r || 2; // Sampling Rate in arbitrary units (i.e. seconds)
                if (r<2) r = 2;//Nyquist frequency

                var i=0;
                var curr = {};

                this.reset = function() { i = 0; };
                this.moveNext = function() {
                  if (qty && i >= qty) return false;

                  curr.s = i;
                  curr.a = a;

                  i++;
                  return true;
                };
                this.current = function() { return curr; };
                this.samplingRate = function() { return r; };
              }

      conf = conf || {};

      var walkable = { getWalker: function() { return new WW(conf); }};
      WAVE.extend(walkable, WAVE.Walkable);

      return walkable;
    };

    // Provides walking capability for array
    published.arrayWalkable = function(arr) {
      var walker = function() {
        var idx = -1;
        this.reset = function() { idx = -1; };
        this.moveNext = function() { return (++idx < arr.length); };
        this.current = function() { return (idx < 0 || idx >= arr.length) ? null : arr[idx]; };
      };

      var walkable = { getWalker: function() { return new walker(); }, OriginalArray: arr };
      WAVE.extend(walkable, WAVE.Walkable);
      return walkable;
    };

    // Converts array with {k : {}, v: []} structure to Walkable group operation result (inverse method to Walkable.wGroupToArray)
    published.groupWalkable = function(arr) {
      var walker = function() {
        var idx = -1;
        var cur = null;
        this.reset = function() { idx = -1; cur = null; };
        this.moveNext = function() {
          if (++idx < arr.length) {
            var arrCur = arr[idx];
            cur = { k: arrCur.k, v: WAVE.arrayWalkable(arrCur.v) };
            return true;
          } else {
            return false;
          }
          //Dkh1222015  return (++idx < arr.length);
        };
        this.current = function() { return cur; };
      };

      var walkable = { getWalker: function() { return new walker(); }, OriginalArray: arr};
      WAVE.extend(walkable, WAVE.Walkable);
      return walkable;
    };