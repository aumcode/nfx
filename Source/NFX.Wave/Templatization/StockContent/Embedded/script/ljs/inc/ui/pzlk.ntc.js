
var PUZZLE_DFLT_HELP = "Please enter the following security information by touching the symbols below";
var PUZZLE_DFLT_PLACEHOLDER = "···············";
var PUZZLE_DFLT_CLEAR = "Clear";
var PUZZLE_DFLT_INPUT_SYMBOL = "*";

//Puzzle keypad class: new PuzzleKeypad({DIV: divPuzzle, Image: '/security/captcha?for=login', Question: 'Your first name spelled backwards'});
published.PuzzleKeypad = function(init) {
  if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || WAVE.strEmpty(init.Image)) throw "PuzzleKeypad.ctor(init.DIV, init.Image)";

  var keypad = this;
  WAVE.extend(keypad, WAVE.EventManager);

  var rndKey = WAVE.genRndKey(4);
  var fDIV = init.DIV;
  var fHelp = WAVE.strEmpty(init.Help) ? PUZZLE_DFLT_HELP : init.Help;
  var fQuestion = WAVE.strEmpty(init.Question) ? "" : init.Question;
  var fValue = [];
  var fImage = init.Image;

  var fdivHelp = null,
      fdivQuestion = null,
      fdivInput = null,
      fdivPlaceholder = null,
      fbtnClear = null,
      fimgKeys = null;

  function rebuild() {
    var seed = "pzl_" + rndKey + "_" + WAVE.genAutoincKey("_puzzle#-?Keypad@Elements");

    var args = {
      hid: "divHelp_" + seed,
      qid: "divQuestion_" + seed,
      tid: "tbValue_" + seed,
      bid: "btnClear_" + seed,
      iid: "img_" + seed,
      pid: "plhd_" + seed,
      help: fHelp,
      question: fQuestion,
      img: fImage + "&req=" + WAVE.genRndKey(),
      clear: WAVE.strDefault(init.Clear, PUZZLE_DFLT_CLEAR),
      placeholder: WAVE.strDefault(init.Placeholder, PUZZLE_DFLT_PLACEHOLDER),
      symbol: WAVE.strDefault(init.InputSymbol, PUZZLE_DFLT_INPUT_SYMBOL)
    };

    function buildPuzzle(root, data) {
      /***
      div {
        div {
          id=?data.hid
          class=wvPuzzleHelp
          div="Protect yourself from fraud!" { class=wvPuzzleHeader }
          div=?data.help { class=wvPuzzleBody }
        }
        div=?data.question { id=?data.qid class=wvPuzzleQuestion }
        div {
          class=wvPuzzleImg
          img {
            id=?data.iid
            src=?data.img
          }
        }
        div {
          class=wvPuzzleControls
          div {
            class=wvPuzzleInputs
            div {
              id=?data.tid
              class='wvInput hidden'
            }
            div=?data.placeholder {
              id=?data.pid
              class='wvPlaceholder'
            }
          }
          button=?data.clear {
            id=?data.bid
          }
        }
      }
      ***/
    }

    buildPuzzle(fDIV, args);

    fdivHelp = WAVE.id(args.hid);
    fdivQuestion = WAVE.id(args.qid);
    fdivInput = WAVE.id(args.tid);
    fbtnClear = WAVE.id(args.bid);
    fimgKeys = WAVE.id(args.iid);
    fdivPlaceholder = WAVE.id(args.pid);

    $(fbtnClear).click(function(evt) {//CLEAR
      evt.preventDefault();
      fdivInput.innerHTML = "";
      fValue = [];

      WAVE.addClass(fdivInput, "hidden");
      WAVE.removeClass(fdivPlaceholder, "hidden");

      keypad.eventInvoke(published.EVT_PUZZLE_KEYPAD_CHANGE, fValue);
    });

    $(fimgKeys).click(function(e) {//IMAGE click
      var offset = $(this).offset();
      var point = { x: Math.round(e.pageX - offset.left), y: Math.round(e.pageY - offset.top) };
      fValue.push(point);
      fdivInput.appendChild("*** span=?args.symbol { } ***");

      WAVE.addClass(fdivPlaceholder, "hidden");
      WAVE.removeClass(fdivInput, "hidden");

      keypad.eventInvoke(published.EVT_PUZZLE_KEYPAD_CHANGE, fValue);
    });
  }

  this.destroy = function() {
    fDIV.innerHTML = "";
  };

  //Returns value as an array of points where user clicked
  this.value = function() { return fValue; };

  //Returns value as a JSON array of points where user clicked
  this.jsonValue = function() { return JSON.stringify(fValue); };

  this.help = function(val) {
    if (typeof (val) === tUNDEFINED) return fHelp;
    if (WAVE.strEmpty(val)) fHelp = PUZZLE_DFLT_HELP;
    else fHelp = val;
    fdivHelp.innerHTML = fHelp;
  };

  this.question = function(val) {
    if (typeof (val) === tUNDEFINED) return fQuestion;
    if (WAVE.strEmpty(val)) fQuestion = "";
    else fQuestion = val;
    fdivQuestion.innerHTML = fQuestion;
  };

  this.image = function(url) {
    if (typeof (url) === tUNDEFINED) return fImage;
    if (WAVE.strEmpty(url)) fImage = "";
    else fImage = url;
    fimgKeys.src = fImage + "&req=" + WAVE.genRndKey();
  };

  rebuild();
};//keypad