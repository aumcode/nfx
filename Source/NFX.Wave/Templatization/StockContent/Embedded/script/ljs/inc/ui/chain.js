var fChainSelectorSeed = 0;
/*
init : {
  DIV: html container, requred,
  outputFormFieldName : "fieldId",
  classes: css names,
  path: initail path ('Books.General.A'), optional,
  readonly: true/false,
  disabled: true/false,
  values: [
    { id: n1, val: node1, name: 'Some header', descr: 'Some text description' },
    {
      id: n2,
      val: node2,
      name: '...',
      descr: '...',
      children : [ { id: n21, val: node21, descr: '...' }, { id: n22, val: node22, descr: '...' } ]
    },
    ...
  ]
}
*/
published.ChainSelector = function (init) {
  if (!WAVE.exists(init)) throw "ChainSelector.ctor(init=null)";

  var chain = this;
  WAVE.extend(chain, WAVE.EventManager);

  var fId = "wvChainSelector_" + fChainSelectorSeed++;
  var fDiv;
  if (WAVE.has(init, 'DIV')) fDiv = WAVE.isString(init.DIV) ? WAVE.id(init.DIV) : init.DIV;
  var fOutputFormFieldName = WAVE.strDefault(init.outputFormFieldName, null);
  var fDescr = WAVE.strDefault(init.description);
  var fValues = (WAVE.has(init, 'values') && WAVE.exists(init.values) && WAVE.isArray(init.values)) ? init.values : [];
  var fPath = WAVE.strDefault(init.path, '');
  var fReadOnly = WAVE.strAsBool(init.readonly, false);
  var fDisabled = WAVE.strAsBool(init.disabled, false);
  var fCombos = [];

  var fClasses = WAVE.get(init, "classes", {});
  fClasses.divWrapCls = WAVE.strDefault(fClasses.divWrapCls, published.CLS_CHAIN_SELECTOR);
  fClasses.divInfoCls = WAVE.strDefault(fClasses.divInfoCls, published.CLS_CS_DIV_INFO);
  fClasses.divNameCls = WAVE.strDefault(fClasses.divNameCls, published.CLS_CS_DIV_NAME);
  fClasses.divDescrCls = WAVE.strDefault(fClasses.divDescrCls, published.CLS_CS_DIV_DESCR);
  fClasses.divComboWrapCls = WAVE.strDefault(fClasses.divComboWrapCls, published.CLS_CS_DIV_COMBO_WRAP);
  fClasses.divArrowCls = WAVE.strDefault(fClasses.divArrowCls, published.CLS_CS_DIV_ARROW);

  var fSeed = "cs_" + fChainSelectorSeed;
  var fControlIds = {
    divInfoId: "divInfo_" + fSeed,
    divNameId: "divName_" + fSeed,
    divDescrId: "divDescr_" + fSeed,
    divComboWrapId: "divComboWrap_" + fSeed + "_",
    selectBodyId: "selectBodyId" + fSeed + "_",
    divArrowId: "divArrow_" + fSeed + "_",
    outputHiddenId : "hidden_" + fSeed
  };

  function getSelectorByIndex(idx) {
    var sid = fControlIds.selectBodyId + idx.toString();
    return WAVE.id(sid);
  }

  function selectorValueChanged(e) {
    var elm = e.target.parentNode;
    var lvl = fCombos.indexOf(elm);
    var newPath = __path().slice(0, lvl+1);
    __navigate(newPath);
  }

  function addCombo(values, value) {
    if (fDisabled || fReadOnly) return false;

    // construct template
    var lvl = fCombos.length;
    var wid = fControlIds.divComboWrapId + lvl.toString();
    var sid = fControlIds.selectBodyId + lvl.toString();
    var html = WAVE.strHTMLTemplate("<div id='@divComboWrapId@' class='@divComboWrapCls@' @disabled@ @readonly@>" +
                                      "<select id='@selectBodyId@'>" +
                                        "<option value=''></option>", //add blank,
                                    {
                                      divComboWrapId:  wid,
                                      divComboWrapCls: fClasses.divComboWrapCls,
                                      selectBodyId: sid,
                                      disabled: fDisabled ? "" : "disabled",
                                      readonly: fReadOnly ? "readonly" : ""
                                    });

    values.wEach(function (v) {
      html += WAVE.strHTMLTemplate("<option value='@value@' @selected@>@descr@</option>",
                                    {
                                      value:    v.id,
                                      selected: WAVE.strSame(v.id, value) ? "selected" : "",
                                      descr:    v.val
                                    });
    });

    html += WAVE.strHTMLTemplate("</select><div id='@divArrowId@' class='@divArrowCls@'></div></div>",
                                  {
                                    divArrowId:  fControlIds.divArrowId + lvl.toString(),
                                    divArrowCls: fClasses.divArrowCls
                                  });

    var divDescr = WAVE.id(fControlIds.divInfoId);
    var tmp = document.createElement('div');
    tmp.innerHTML = html;
    var divComboWrap = tmp.firstChild;
    fDiv.insertBefore(divComboWrap, divDescr);
    fCombos[lvl] = divComboWrap;

    WAVE.addEventHandler(WAVE.id(sid), "change", selectorValueChanged);

    return divComboWrap;
  }

  function removeComboxes(cnt) {
    if (fDisabled || fReadOnly) return false;

    for (var i=0; i<cnt; i++) {
      var combo = fCombos.pop();
      if (!WAVE.exists(combo)) return;

      var lvl = fCombos.length;
      WAVE.removeEventHandler(getSelectorByIndex(lvl), "change", selectorValueChanged);

      fDiv.removeChild(combo);
    }
  }

  function __navigate(path) {
    if (fDisabled || fReadOnly) return false;
    if (!WAVE.exists(path)) path = [''];
    if (WAVE.isString(path)) path = path.split('.');
    if (!WAVE.isArray(path)) return false;

    var values = WAVE.arrayWalkable(fValues);
    var children = null;
    var part;
    var node;
    var result = true;

    // init
    removeComboxes(fCombos.length - path.length);
    if (fCombos.length <= 0) addCombo(values, '');

    // navigate to tree node
    for (var i in path) {
      part = path[i];

      // special case: empty part resets current combo
      if (WAVE.strSame(part, '')) {
        removeComboxes(fCombos.length-i);
        children = values;
        break;
      }

      // seek for node with same name
      if (values === null) {
        result = false;
        removeComboxes(fCombos.length-i);
        break;
      }
      var n = values.wFirst(function (v) { return WAVE.strSame(v.id, part); });
      if (n === null)
      {
        result = false;
        removeComboxes(fCombos.length-i);
        children = values;
        break;
      }
      node = n;
      children = WAVE.exists(node.children) ? WAVE.arrayWalkable(node.children) : null;

      // add new combo if it does not exist, adjust next combos
      var combo = fCombos[i];
      if (!WAVE.exists(combo)) {
        removeComboxes(fCombos.length-i);
        addCombo(values, part);
      } else {
        var selector = getSelectorByIndex(i);
        if (!WAVE.strSame(selector.value, part)) {
          removeComboxes(fCombos.length-i-1);
          selector.value = part;
        }
      }

      values = children;
    }

    // add empty next combo if needed
    if (WAVE.exists(children)) addCombo(children, '');

    // update info
    var divName = WAVE.id(fControlIds.divNameId);
    var name = WAVE.exists(node) ? WAVE.strDefault(node.name, '') : '';
    divName.innerText = !WAVE.strSame(name, '') ? "Name: " + name : '';
    var descr = WAVE.exists(node) ? WAVE.strDefault(node.descr, '') : '';
    var divDescr = WAVE.id(fControlIds.divDescrId);
    divDescr.innerText = !WAVE.strSame(descr, '') ?"Description: " + descr : '';

    setHiddenValue();
    chain.eventInvoke(published.EVT_CHAIN_SELECTOR_UPDATED, __value());

    return result;
  }

  function __path() {
    var result = [];
    for (var i in fCombos) {
      var combo = fCombos[i];
      var selector = WAVE.id(fControlIds.selectBodyId + i.toString());
      result.push(selector.value);
    }
    return result;
  }

  function __value() {
    var res = __path().join('.');
    if (res.length > 0 && WAVE.strSame(res[res.length - 1], '.'))
      res = res.substring(0, res.length - 1);
    return res;
  }

  function setHiddenValue() {
    if (fOutputFormFieldName !== null) WAVE.id(fControlIds.outputHiddenId).value = chain.value();
  }

  function build() {
    if (!WAVE.exists(fDiv)) return;

    WAVE.addClass(fDiv, fClasses.divWrapCls);

    // build main html
    var html = WAVE.strHTMLTemplate("<input id='@outerId@' type='hidden' name='@outerName@'/>" +
                                    "<div id='@divInfoId@' class='@divInfoCls@'>" +
                                      "<div id='@divNameId@' class='@divNameCls@'></div>" +
                                      "<div id='@divDescrId@' class='@divDescrCls@'></div>" +
                                    "</div>",
                                    {
                                      outerId: fControlIds.outputHiddenId,
                                      outerName: WAVE.strDefault(fOutputFormFieldName, ''),
                                      divInfoId:  fControlIds.divInfoId,
                                      divInfoCls: fClasses.divInfoCls,
                                      divNameId: fControlIds.divNameId,
                                      divNameCls: fClasses.divNameCls,
                                      divDescrId: fControlIds.divDescrId,
                                      divDescrCls: fClasses.divDescrCls
                                    });
    fDiv.innerHTML = html;

    // inject first combobox
    __navigate(fPath);
  }

  // public functions

  this.navigate = function(path) {
    return __navigate(path);
  };

  this.path = function() {
    return __path();
  };

  this.value = function () {
    return __value();
  };

  build();

  return chain;
};//ChainSelector