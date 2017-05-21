var utest = {
  CSS_CLASS_AREA:  "wvUTestArea",
  CSS_CLASS_TESTTITLE:  "wvUTestTitle",
  CSS_CLASS_TESTNUMBER: "wvUTestNumber",
  CSS_CLASS_OK:    "wvUTestOK",
  CSS_CLASS_ERROR: "wvUTestError",
  CSS_CLASS_TABLEERROR: "wvUTestTableError",
  CSS_CLASS_TABLETOTAL: "wvUTestTableTotal",
  DIV_SUMMARY: "div_wv_utest_summary",
  DIV_AREA_PREFIX: "div_wv_utest_area_",
  DIV_TEST_PREFIX: "div_wv_utest_test_"
};




var _testOut = [];
var _areaIDs = {};
var _testingStarted = false;
var _testNumber = 0;

  function updateHeader()
  {
    var dis = published.id(utest.DIV_SUMMARY);
    if (!dis) return;
    var content = '<table style="width: 75%">';
    content += '<tr> <th>Area</th> <th>Total</th><th>Passed</th><th>Failed</th> </tr>';

    var gtAll = 0;
    var gtFailed = 0;

    for(var i in _areaIDs)
    {
        var dia = published.id(_areaIDs[i]);
        if (!dia) continue;
        content +=  published.strHTMLTemplate(
                '<tr @STL@> <td>@Area@</td> <td>@Total@</td><td>@Passed@</td><td>@Failed@</td> </tr>',
                {
                STL: dia.STAT_FAILED_TESTS > 0 ? "class="+utest.CSS_CLASS_TABLEERROR : "",
                Area: dia.STAT_NAME,
                Total: dia.STAT_TOTAL_TESTS,
                Passed: dia.STAT_TOTAL_TESTS - dia.STAT_FAILED_TESTS,
                Failed: dia.STAT_FAILED_TESTS});
        gtAll += dia.STAT_TOTAL_TESTS;
        gtFailed += dia.STAT_FAILED_TESTS;
    }

    content +=  published.strHTMLTemplate(
                '<tr class=@CLS@> <td>TOTAL</td> <td>@Total@</td><td>@Passed@</td><td>@Failed@</td> </tr>',
                {
                CLS: utest.CSS_CLASS_TABLETOTAL,
                Total: gtAll,
                Passed: gtAll - gtFailed,
                Failed: gtFailed});

    content += '</table>';
    dis.innerHTML = content;
  }

utest.run = function(area, name, fun) {
  if (!_testingStarted)
  {
    _testingStarted = true;
    setTimeout( updateHeader, 1000);
  }

  if (published.strEmpty(area)) area = "Unspecified";

  var did = null;
  if (area in _areaIDs)
    did = _areaIDs[area];
  else
  {
    did = utest.DIV_AREA_PREFIX + published.genAutoincKey("UtestingArea");
    _areaIDs[area] = did;
  }

  var divArea = published.id(did);
  if (!divArea)
  {
    divArea = document.createElement("div");
    divArea.id = did;
    divArea.className = utest.CSS_CLASS_AREA;
    document.body.appendChild(divArea);
    divArea.innerHTML = "<h2>"+area+"</h2>";

    divArea.STAT_NAME = area;
    divArea.STAT_TOTAL_TESTS = 0;
    divArea.STAT_FAILED_TESTS = 0;
  }

  var error = "";
  try
  {
    _testOut = [];
    _testNumber++;
    divArea.STAT_TOTAL_TESTS += 1;
    fun();
  }
  catch(err)
  {
    divArea.STAT_FAILED_TESTS += 1;
    error = err;
  }
  var isError = !published.strEmpty(error);

  var dtid = utest.DIV_TEST_PREFIX + published.genAutoincKey("UtestingCase");
  var divTest = document.createElement("div");
  divTest.id = dtid;
  divTest.className = isError ? utest.CSS_CLASS_ERROR : utest.CSS_CLASS_OK;

  var content = "<div class='"+utest.CSS_CLASS_TESTTITLE+"'><span class='" + utest.CSS_CLASS_TESTNUMBER + "'>" + _testNumber + "</span>&nbsp;"
    + name + "&nbsp;&nbsp;:&nbsp;&nbsp;" + (isError ? "FAILED" : "PASSED")+"</div>";

  if (isError)
    content += "&nbsp;&nbsp;Error: " + error;


  if (_testOut.length>0)
  {
    content += "<pre>";

    for(var msg in _testOut)
      content += " "+_testOut[msg];

    content += "</pre>";
  }

  divTest.innerHTML = content;
  divArea.appendChild(divTest);
};

//Returns true to indicate that testing has been activated
utest.testingStarted = function(){
  return _testingStarted;
};

//Write log message in new line
utest.log = function(msg){
  _testOut.push("  &gt; "+msg+'\n');
};

//Writes log message inline
utest.logi = function(msg){
  _testOut.push(msg);
};

utest.assertTrue = function(assertion, msg){
  if (published.strEmpty(msg)) msg = "Assertion not true";
  if (!assertion) throw msg;
};

utest.assertFalse = function(assertion, msg){
  if (published.strEmpty(msg)) msg = "Assertion not false";
  if (assertion) throw msg;
};


published.UTest = utest;