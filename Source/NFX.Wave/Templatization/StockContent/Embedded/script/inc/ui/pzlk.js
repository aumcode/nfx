
var PUZZLE_DFLT_HELP = "Please enter the following security information by touching the symbols below";

//Puzzle keypad class: new PuzzleKeypad({DIV: divPuzzle, Image: '/security/captcha?for=login', Question: 'Your first name spelled backwards'});
published.PuzzleKeypad = function (init) {
  if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || WAVE.strEmpty(init.Image)) throw "PuzzleKeypad.ctor(init.DIV, init.Image)";

  var keypad = this;
  WAVE.extend(keypad, WAVE.EventManager);

  var rndKey = WAVE.genRndKey(4);
  var fDIV = init.DIV;
  var fHelp = WAVE.strEmpty(init.Help) ? PUZZLE_DFLT_HELP : init.Help;
  var fQuestion = WAVE.strEmpty(init.Question) ? "" : init.Question;
  var fValue = [];
  var fImage = init.Image;

  var fdivHelp = null;
  var fdivQuestion = null;
  var ftbValue = null;
  var fbtnClear = null;
  var fimgKeys = null;

  function rebuild() {
    var seed = "pzl_" + rndKey + "_" + WAVE.genAutoincKey("_puzzle#-?Keypad@Elements");

    var args = {
      hid: "divHelp_" + seed,
      qid: "divQuestion_" + seed,
      tid: "tbValue_" + seed,
      bid: "btnClear_" + seed,
      iid: "img_" + seed,
      help: fHelp,
      question: fQuestion,
      img: fImage + "&req=" + WAVE.genRndKey(),
      clear: "Clear"
    };

    var html = WAVE.strTemplate(
      "<div class='wvPuzzleHelp'     id='@hid@'>@help@</div>" +
      "<div class='wvPuzzleQuestion' id='@qid@'>@question@</div>" +
      "<div class='wvPuzzleInputs'> <input id='@tid@' placeholder='···············' type='text' disabled /><button id='@bid@'>@clear@</button> </div>" +
      "<div class='wvPuzzleImg'> <img id='@iid@' src='@img@'/></div>", args);

    fDIV.innerHTML = html;

    $("#" + args.bid).click(function (evt) {//CLEAR
      evt.preventDefault();
      ftbValue.value = "";
      fValue = [];
      keypad.eventInvoke(published.EVT_PUZZLE_KEYPAD_CHANGE, fValue);
    });

    $("#" + args.iid).click(function (e) {//IMAGE click
      var offset = $(this).offset();
      var point = { x: Math.round(e.pageX - offset.left), y: Math.round(e.pageY - offset.top) };
      fValue.push(point);
      ftbValue.value += "*";
      keypad.eventInvoke(published.EVT_PUZZLE_KEYPAD_CHANGE, fValue);
    });


    fdivHelp = WAVE.id(args.hid);
    fdivQuestion = WAVE.id(args.qid);
    ftbValue = WAVE.id(args.tid);
    fbtnClear = WAVE.id(args.bid);
    fimgKeys = WAVE.id(args.iid);
  }

  this.destroy = function () {
    fDIV.innerHTML = "";
  };

  //Returns value as an array of points where user clicked
  this.value = function () { return fValue; };

  //Returns value as a JSON array of points where user clicked
  this.jsonValue = function () { return JSON.stringify(fValue); };

  this.help = function (val) {
    if (typeof (val) === tUNDEFINED) return fHelp;
    if (WAVE.strEmpty(val)) fHelp = PUZZLE_DFLT_HELP;
    else fHelp = val;
    fdivHelp.innerHTML = fHelp;
  };

  this.question = function (val) {
    if (typeof (val) === tUNDEFINED) return fQuestion;
    if (WAVE.strEmpty(val)) fQuestion = "";
    else fQuestion = val;
    fdivQuestion.innerHTML = fQuestion;
  };

  this.image = function (url) {
    if (typeof (url) === tUNDEFINED) return fImage;
    if (WAVE.strEmpty(url)) fImage = "";
    else fImage = url;
    fimgKeys.src = fImage + "&req=" + WAVE.genRndKey();
  };

  rebuild();
};//keypad