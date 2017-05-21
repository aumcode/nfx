var fObjectEditorIdSeed = 0;
//{
//  DIV: div,
//  content: {}
//  type : {
//    id: { type: "text" },
//    nls: {
//      type: "nls",
//      options: {n: {}}
//    }
//  }
//}
published.ObjectEditor = function(init) {
  if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "ObjectEditor.ctor(init.DIV)";

  var editor = this;
  WAVE.extend(editor, WAVE.EventManager);

  fObjectEditorIdSeed++;
  var fSeed = "wvObjEditor_" + fObjectEditorIdSeed;

  var fDIV = init.DIV;
  var fMeta = WAVE.get(init, "meta", {});
  var fType = WAVE.strDefault(fMeta.type, "text");
  var fSchema = WAVE.get(fMeta, "schema", {});
  var fPlh = WAVE.strDefault(fMeta.plh);
  var fSets = WAVE.mergeArrays([], fMeta.sets, function(a, b) { return a.toLowerCase() === b.toLowerCase(); }, function(a) { return a.toLowerCase(); });
  var fDflt = fMeta.dflt;
  var fContent = WAVE.get(init, "content", typeof(fDflt) !== tUNDEFINED ? fDflt : {});

  var fReadOnly = WAVE.strAsBool(init.readonly, false);
  var fDisabled = WAVE.strAsBool(init.disabled, false);
  var fCanRaw = WAVE.strAsBool(init.canRaw, false) && !fReadOnly && !fDisabled;
  var fCanDel = WAVE.strAsBool(init.canDel, true) && !fReadOnly && !fDisabled;
  var fCanAdd = WAVE.strAsBool(init.canAdd, true) && !fReadOnly && !fDisabled;
  var fMaxLength = WAVE.intValidPositive(fMeta.maxLength) ? fMeta.maxLength : 2;
  var fOutputFormFieldName = WAVE.strDefault(init.outputFormFieldName, "");

  var fCurrentLocal = WAVE.strDefault(init.local, "eng");
  var fLocalizerSchema = "ObjectEditor";
  var fLocalizerField = "objEd";
  var fValidators = [];
  var fEditors = [];

  var fControlsIds = {
    editorContainerId : "edit_cont_" + fSeed,
    buttonsContainerId : "edit_buttons_" + fSeed,
    inputId : "edit_input_" + fSeed,
    outputHiddenId: "edit_conf_out_" + fSeed,
    btnRawId: "edit_raw_" + fSeed,
    btnDelId: "edit_del_" + fSeed
  };

  var fTexts = WAVE.get(init, "texts", {});
    fTexts.addTitle = getText("add-title", "Add New Language");
    fTexts.addBody = getText("add-body", "");
    fTexts.rawTitle= getText("raw-title", "Raw Edit");
    fTexts.rawBody = getText("raw-body");
    fTexts.clearConfirmTitle = getText("clear-confirm-title", "Confirm Clear");
    fTexts.clearConfirmBody = getText("clear-confirm-body", "Clear all language data?");
    fTexts.delConfirmTitle = getText("del-confirm-title", "Confirm Delete");
    fTexts.resetConfirmTitle = getText("reset-confirm-title", "Confirm Reset");
    fTexts.delConfirmBody = getText("del-confirm-body");
    fTexts.resetConfirmBody = getText("reset-confirm-body");
    fTexts.btnDel = getText("btn-del", "Delete");
    fTexts.btnAdd = getText("btn-add", "Add");
    fTexts.btnRaw = getText("btn-raw", "Edit Raw");
    fTexts.btnClear = getText("btn-clear", "Clear");
    fTexts.btnReset = getText("btn-reset", "Reset");
    fTexts.required = getText("err-meg", "Required");

  var Control = function(name, nodeDIV) {
    var meta = fSchema[name];
    var DIV = WAVE.get(meta , "DIV", nodeDIV);
    var dflt = meta.dflt;
    var content = WAVE.get(fContent, name, dflt);
    var type = WAVE.strDefault(meta.type, "text");
    var schema = WAVE.get(meta, "schema", {});
    var plh = WAVE.strDefault(meta.plh, "");
    var fIdx = WAVE.intValidPositiveOrZero(meta.index) ? meta.index : -1;
    var sets = WAVE.mergeArrays([], meta.sets, function(a, b) { return a.toLowerCase() === b.toLowerCase(); }, function(a) { return a.toLowerCase(); });
    var items = WAVE.isObject(meta.items) ? meta.items : {};
    var disabled = WAVE.strAsBool(meta.disabled, fDisabled);
    var readonly = WAVE.strAsBool(meta.readonly, fReadOnly);
    var canRaw = WAVE.strAsBool(meta.canRaw, false);
    var canDel = WAVE.strAsBool(meta.canDel, true);
    var canAdd = WAVE.strAsBool(meta.canAdd, true);
    var required = WAVE.strAsBool(meta.required, false);
    var nodeClass = WAVE.strDefault(meta.cls);
    var errMsg = WAVE.strDefault(meta.errMsg, required ? fTexts.required : "");
    var showError = WAVE.isFunction(meta.showError) ? meta.showError : function() { WAVE.id(errorDivId).innerText = errMsg; };
    var hideError = WAVE.isFunction(meta.hideError) ? meta.hideError : function() { WAVE.id(errorDivId).innerText = ""; };
    var validFunction = WAVE.isFunction(meta.validate) ?
      meta.validate :
      function(sender, v, inIndex) {
      var res = true;
      if (required) {
        if (type === "text" || type === "textarea" || type === "radio" || type === "combo") res = !WAVE.strEmpty(v);
        if (type === "array" || type === "multiple") res = WAVE.isArray(v) && v.length > 0;
        if (type === "object" || type === "ps") res = WAVE.isObject(v) && !WAVE.empty(v);
      }
      return {ok: res, value: v, index: inIndex};
    };

    var divId = uName(name, fControlsIds.editorContainerId);
    var inputId = uName(name, fControlsIds.inputId);
    var errorDivId = divId + "_err";
    var ed = null;
    var i, l, nId, radios, key, html, vFunc, arr;

    if (type === "text" || type === "textarea" || type === "combo" || type === "check") {
      vFunc = function() {
        var ctrl = WAVE.id(inputId);
        var vl = type === "check" ? ctrl.checked : ctrl.value;
        return validateField(validFunction, ctrl, vl, fIdx, errorDivId, errMsg, showError, hideError);
      };
    }
    if (type === "array" || type === "object" || type === "ps") {
      vFunc = function() {
        return validateField(validFunction, ed, ed.jsonContent(), fIdx, errorDivId, errMsg, showError, hideError);
      };
    }
    if (type === "radio" || type === "multiple") {
      vFunc = function() {
        radios = document.getElementsByName(inputId);
        l = radios.length;
        arr = [];
        for (i = 0; i < l; i++) {
          if (radios[i].checked) {
            arr.push(radios[i].value);
          }
        }
        return validateField(validFunction, radios, type === "multiple" ? arr : (arr.length > 0 ? arr[0] : null), fIdx, errorDivId, errMsg, showError, hideError);
      };
    }
    fValidators.push(vFunc);

    var strHtml =
      "<div class='@nodeCont@'>" +
        (WAVE.strEmpty(plh) ? "" : "<label class='@lblCls@' for='@labelFor@'>@plh@</label>") +
        "<div class='@errorDivCls@' id='@errorDivId@'></div>" +
        "<div id='@divId@' class='@ccCls@'></div>" +
      "</div>";
    var strDisabled = disabled ? "disabled" : "";
    var strReadonly = readonly ? "readonly" : "";

    DIV.insertAdjacentHTML("beforeend", WAVE.strHTMLTemplate(strHtml, {
      nodeCont: published.CLS_OBJECT_EDITOR_NODE_CONTAIER + " " + nodeClass,
      labelFor: inputId,
      lblCls: required ? "wvRequired" : "",
      plh: plh,
      errorDivCls: "wvError",
      errorDivId: errorDivId,
      divId: divId,
      ccCls: type === "array" ?
        published.CLS_OBJECT_EDITOR_ARRAY_CONTAIER :
        ((type === "object" || type === "ps") ?
          published.CLS_OBJECT_EDITOR_OBJECT_CONTAIER :
          published.CLS_OBJECT_EDITOR_INPUT_CONTAIER)
    }));

    var gDiv = WAVE.id(divId);
    var erDiv = WAVE.id(errorDivId);
    var cinit = {
      DIV : gDiv,
      id : inputId,
      value : WAVE.strDefault(content),
      meta: meta,
      content: content,
      disabled : disabled,
      readonly : readonly,
      addLabel : false,
      placeholder : plh,
      sets: sets,
      canRaw : canRaw,
      canAdd : canAdd,
      canDel : canDel
    };

    function checkAndSave() {
      var res = vFunc();
      if (!res.ok) return;

      fContent = WAVE.isObject(fContent) ? fContent : {};
      var value = res.value;
      fContent[name] = value;
      editor.content(fContent);
    }

    if(type === "textarea"){
      ed = new published.MultiLineTextBox(cinit);
      ed.eventBind(published.EVT_MULTI_LINE_TEXT_UPDATED, function (e, args) {
        checkAndSave();
      });
    } else if(type === "object" || type === "array") {
      ed = new published.ObjectEditor(cinit);
      ed.eventBind(published.EVT_OBJECT_EDITOR_UPDATED, function (e, d) {
        checkAndSave();
      });
    } else if (type === "ps") {
      ed = new published.PropSetEditor(cinit);
      ed.eventBind(published.EVT_PS_EDITOR_UPDATED, function (e, d) {
        checkAndSave();
      });
    } else if(type === "check") {
      gDiv.innerHTML = WAVE.strHTMLTemplate("<input id='@id@' @disabled@ @readonly@ type='checkbox' placeholder='@plh@' title='@plh@' />",
      {
        id: inputId,
        disabled: strDisabled,
        readonly: strReadonly,
        plh: plh,
        value: WAVE.strDefault(content)
      });
      nId = WAVE.id(inputId);
      nId.checked = WAVE.strAsBool(content, false);
      nId.onclick = function change(e) {
        checkAndSave();
      };
    } else if(type === "multiple") {
      html = "";
      for(key in items) {
        nId = inputId + "_+ " + key;
        html += WAVE.strHTMLTemplate("<label for='@id@'>@name@</label>",
        {
          id: nId,
          name: WAVE.strDefault(items[key], key)
        });
        html += WAVE.strHTMLTemplate("<input type='checkbox' @checked@ name='@name@' value='@value@' id='@id@' @disabled@ @readonly@/>",
        {
          checked: ((WAVE.isArray(content) && WAVE.inArray(content, key)) ? "checked='checked'" : ""),
          name: inputId,
          value: key,
          id: nId,
          disabled: strDisabled,
          readonly: strReadonly
        });
      }
      gDiv.innerHTML = html;
      radios = document.getElementsByName(inputId);
      l = radios.length;
      for(i = 0; i < l; i++) {
        radios[i].onclick = checkAndSave;
      }
    } else if (type === "radio") {
      html = "";
      for(key in items){
        nId = inputId + "_+ " + key;
        html += WAVE.strHTMLTemplate("<label for='@id@'>@name@</label>",
        {
          id: nId,
          name: WAVE.strDefault(items[key], key)
        });
        html += WAVE.strHTMLTemplate("<input type='radio' @checked@ name='@name@' value='@value@' id='@id@' @disabled@ @readonly@/>",
        {
          checked: content === key ? "checked='checked'" : "",
          name: inputId,
          value: key,
          id: nId,
          disabled: strDisabled,
          readonly: strReadonly
        });
      }
      gDiv.innerHTML = html;
      radios = document.getElementsByName(inputId);
      l = radios.length;
      for(i = 0; i < l; i++) {
        radios[i].onclick = checkAndSave;
      }
    }
    else if(type === "combo") {
      html = "<select id='@id@' @disabled@ @readonly@>";
      if(WAVE.strEmpty(dflt)) {
        html += "<option value=''></option>";
      }
      for(key in items) {
        html += WAVE.strHTMLTemplate("<option @selected@ value='@value@'>@text@</option>",
        {
          selected: content === key ? "selected" : "",
          value: key,
          text: WAVE.strDefault(items[key], key)
        });
      }
      html += "</select>";
      gDiv.innerHTML = WAVE.strHTMLTemplate(html,
      {
        id: inputId,
        name: WAVE.strDefault(plh, key),
        disabled: strDisabled,
        readonly: readonly ? "disabled" : ""
      });
      if (!WAVE.strEmpty(dflt) && WAVE.strEmpty(fContent[name])) {
        fContent[name] = dflt;
      }
      WAVE.id(inputId).onchange = function change(e) {
        checkAndSave();
      };
    } else if(type === "text") {
      gDiv.innerHTML = WAVE.strHTMLTemplate("<input id='@id@' @disabled@ @readonly@ type='text' placeholder='@plh@' title='@plh@' value='@value@'/>",
      {
        id: inputId,
        disabled: strDisabled,
        readonly: strReadonly,
        plh: plh,
        value: WAVE.strDefault(content)
      });
      WAVE.id(inputId).onchange = function change(e) {
        checkAndSave();
      };
    }
    vFunc();
  };

  function getText(n, dflt){
    return localize(fTexts[n], dflt);
  }

  function localize(text, dflt){
    return WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, WAVE.strDefault(text, dflt));
  }

  var fClasses = WAVE.get(init, "btnClasses", {});
    fClasses.addBtnCls = WAVE.strDefault(fClasses.addBtnCls, published.CLS_PS_BTN_ADD);
    fClasses.delBtnCls = WAVE.strDefault(fClasses.delBtnCls, published.CLS_PS_BTN_DEL);
    fClasses.rawBtnCls = WAVE.strDefault(fClasses.rawBtnCls, published.CLS_PS_BTN_RAW);
    fClasses.clearBtnCls = WAVE.strDefault(fClasses.clearBtnCls, published.CLS_PS_BTN_CLEAR);
    fClasses.modalBtnCls = WAVE.strDefault(fClasses.modalBtnCls, "");

  function setHiddenValue(){
      if (fOutputFormFieldName !== "") WAVE.id(fControlsIds.outputHiddenId).value = editor.content();
  }

  this.content = function(value, rebuild) {
    if (typeof (value) === tUNDEFINED)
      return WAVE.empty(fContent) ? "" : JSON.stringify(fContent, null, 2);

    var parsed = WAVE.tryParseJSON(value);
    if (parsed.ok) {
      fContent = parsed.obj;

      setHiddenValue();
      editor.eventInvoke(published.EVT_OBJECT_EDITOR_UPDATED, fContent);
      if(rebuild === true) {
        build();
      }
    }
    return value;
  };

  this.jsonContent = function() {
    return fContent;
  };

  this.maxLength = function() {
    return fMaxLength;
  };

  this.DIV = function() {
    return fDIV;
  };

  this.validate = function() {
    var vals = [];
    var ok = true;
    for(var f in fValidators) {
      var r = fValidators[f]();
      ok = ok && r.ok;
      vals.push(r);
    }
    return {ok: ok, result: vals};
  };

  this.validators = function() {
    return fValidators;
  };

  function validateField(func, control ,value, idx, errorDivId, errMsg, showError, hideError) {
    var res = func(editor, value, idx);
    if (res.ok) {
      hideError();
      editor.eventInvoke(published.EVT_OBJECT_EDITOR_VALIDATION_SUCCESS, { phase: published.EVT_PHASE_AFTER, target: control });
    } else {
      showError();
      editor.eventInvoke(published.EVT_OBJECT_EDITOR_VALIDATION_ERROR, { phase: published.EVT_PHASE_AFTER, target: control });
    }
    return res;
  }

  function uName(filedName, id){
    return filedName + id;
  }

  function build() {
    fValidators = [];
    fDIV.innerHTML = WAVE.strHTMLTemplate("<div id='@contId@'></div><div id='@buttonsId@'></div>",
      {
        contId: fControlsIds.editorContainerId,
        buttonsId: fControlsIds.buttonsContainerId
      });
    var nodeDIV = WAVE.id(fControlsIds.editorContainerId);
    var buttonsDIV = WAVE.id(fControlsIds.buttonsContainerId);
    if (fType === "array") {
      if(WAVE.isArray(fContent)){
        for(var i = 0; i < fContent.length; i++){
          createArrItem(i, nodeDIV);
        }
      }
    } else {
      for(var o in fSchema){
        var ed = new Control(o, nodeDIV);
      }
    }

    if(fType === "array" && fCanAdd && !fReadOnly && !fDisabled && (!WAVE.isArray(fContent) || fContent.length < fMaxLength)){
      var buttonAddId = "add_" + fSeed;
      buttonsDIV.insertAdjacentHTML("beforeend", "<input type='button' value='" + fTexts.btnAdd + "' id='" + buttonAddId + "' class='" + fClasses.addBtnCls + "'/>");
      WAVE.id(buttonAddId).onclick = function () {
        if(!WAVE.isArray(fContent)) fContent = [];
        fContent.push({});
        editor.content(fContent, true);
        editor.validate();
      };
    }

    if (fCanRaw) {
      buttonsDIV.insertAdjacentHTML("beforeend", "<input type='button' value='" + fTexts.btnRaw + "' id='" + fControlsIds.btnRawId + "' class='" + fClasses.addBtnCls + "'/>");
      WAVE.id(fControlsIds.btnRawId).onclick = function () {
        var inputId = "raw" + fSeed;
        published.showConfirmationDialog(
          fTexts.rawTitle,
          "<div>" + fTexts.rawBody + "</div>" +
          "<div><textarea style='width: 70vw; height: 70vh;' class='" + published.CLS_PS_TEXTAREA_RAW_EDITOR + "' type='text' id='" + inputId + "'></textarea></div>",
          [published.DLG_OK, published.DLG_CANCEL],
          function (sender, result) {
            if (result === published.DLG_OK) {
              var val = WAVE.id(inputId).value;
              var res = WAVE.tryParseJSON(val);
              if (res.ok){
                  if (fType !== 'array' && WAVE.checkKeysUnique(res.obj)){
                    published.toast(WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, "JSON is valid, but some keys are duplicated"), "error");
                    return false;
                  }
                  editor.content(val, true);
                }
              else {
                published.toast(WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, "Invalid JSON"), "error");
                return false;
              }
            }
            return true;
          },
          { btnCls: fClasses.modalBtnCls });
        WAVE.id(inputId).value = editor.content();
        WAVE.id(inputId).focus();
      };
    }
  }

  var arrItemTemplate = "<div class='@divCLS@' id='@divId@'></div>" +
    (fCanDel && !fReadOnly && !fDisabled ? "<input type='button' id='@BtnId@' class='@BtnCls@' value='@delText@'/>" : "") +
    "<div class='@arrSep@'></div>";
  function createArrItem(idx, nodeDIV){
    var m = WAVE.memberClone(fMeta);
    m.type = "object";
    for(var nm in m.schema){
      m.schema[nm].index = idx;
    }
    var id = fControlsIds.editorContainerId + "_" + idx;
    var delButtonId = "del" + id;
    nodeDIV.insertAdjacentHTML("beforeend",WAVE.strHTMLTemplate(arrItemTemplate,
    {
      divCLS: published.CLS_OBJECT_EDITOR_ARRAY_ITEM,
      divId: id,
      BtnId: delButtonId,
      BtnCls: fClasses.delBtnCls,
      delText: fTexts.btnDel,
      arrSep: published.CLS_OBJECT_EDITOR_ARRAY_ITEM_SEPARATOR
    }));
    var ed = new published.ObjectEditor({
      DIV: WAVE.id(id),
      content: fContent[idx],
      meta: m,
      disabled : fDisabled,
      readonly : fReadOnly
    });
    ed.eventBind(published.EVT_OBJECT_EDITOR_UPDATED, function (e, data) {
      fContent[idx] = data;
      editor.content(fContent);
    });
    ed.eventBind(published.EVT_OBJECT_EDITOR_VALIDATION_ERROR,function(e, args) {
      editor.eventInvoke(published.EVT_OBJECT_EDITOR_VALIDATION_ERROR, { phase: published.EVT_PHASE_AFTER, target: args.target });
    });
    ed.eventBind(published.EVT_OBJECT_EDITOR_VALIDATION_SUCCESS,function(e, args) {
      editor.eventInvoke(published.EVT_OBJECT_EDITOR_VALIDATION_SUCCESS, { phase: published.EVT_PHASE_AFTER, target: args.target });
    });
    var vls = ed.validators();
    for(var vl in vls) {
      fValidators.push(vls[vl]);
    }
    if (fCanDel && !fReadOnly && !fDisabled) {
      WAVE.id(delButtonId).onclick = function () {
        published.showConfirmationDialog(
          fTexts.delConfirmTitle,
          "<div>"+fTexts.delConfirmBody+"</div>",
          [published.DLG_OK, published.DLG_CANCEL],
          function (sender, result) {
            if (result === published.DLG_OK) {
              fContent.splice(idx, 1);
              editor.content(fContent, true);
            }
            return true;
          },
          { btnCls: fClasses.modalBtnCls });
      };
    }
  }

  build();
};//ObjectEditor