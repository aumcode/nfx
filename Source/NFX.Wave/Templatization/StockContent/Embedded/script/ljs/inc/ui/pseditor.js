var fPSEditorIDSeed = 0;
// {
//   "DIV": html container, --required
//   "outputFormFieldName" : "fieldId"
//   "sets" :[],
//   "content": "{}",
//   "disabled": false,
//   "readonly": false,
//   "canAdd": true,
//   "canDel": true,
//   "canRaw": true,
//   "options" : {"n": {type : "text", plh: "Name"}, "d": {type : "textarea", plh: "Description"}},
//   "btnClasses" : {
//     "addBtnCls" : "class name",
//     "delBtnCls" : "class name",
//     "rawBtnCls" : "class name",
//     "clearBtnCls" : "class name",
//     "modalBtnCls" : "class name",
//   },
//   "texts" : {
//     "add-title" : "Add New Language",
//     "add-body" : "",
//     "raw-title" : "Raw Edit",
//     "raw-body" : "",
//     "clear-confirm-title" : "Confirm Clear",
//     "clear-confirm-body" : "Clear all language data?",
//     "del-confirm-title" : "Confirm Delete",
//     "del-confirm-body" : "",
//     "btn-del" : "Delete",
//     "btn-add" : "Add",
//     "btn-raw" : "Edit Raw",
//     "btn-clear" : "Clear",
//     "n-plh" : "Name",
//     "d-plh" : "Description",
//   }
// }
published.PropSetEditor = function (init) {
  if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "PropSetEditor.ctor(init.DIV)";

  var editor = this;
  WAVE.extend(editor, WAVE.EventManager);

  fPSEditorIDSeed++;
  var fDIV = init.DIV;
  var fContent = WAVE.memberClone(WAVE.tryParseJSON(init.content).obj);
  var fOutputFormFieldName = WAVE.strDefault(init.outputFormFieldName, null);
  var fReadOnly = WAVE.strAsBool(init.readonly, false);
  var fDisable = WAVE.strAsBool(init.disabled, false);
  var fCanRaw = WAVE.strAsBool(init.canRaw, true);
  var fCanAdd = WAVE.strAsBool(init.canAdd, true);
  var fCanDel = WAVE.strAsBool(init.canDel, true);
  var fSets =  WAVE.mergeArrays(["eng"], init.sets, function(a, b) { return a.toLowerCase() === b.toLowerCase(); }, function(a) { return a.toLowerCase(); });
  var fCurrentLocal = WAVE.strDefault(init.local, "eng");
  var fLocalizerSchema = "PSEditor";
  var fLocalizerField = "ps";
  var fTitle = WAVE.strDefault(init.title);
  var fSchema = WAVE.get(init, "schema", {n: {type: "text",plh: "Name"}, d: {type : "textarea", plh: "Description"}});

  var fClasses = WAVE.get(init, "btnClasses", {});
    fClasses.addBtnCls = WAVE.strDefault(fClasses.addBtnCls, published.CLS_PS_BTN_ADD);
    fClasses.delBtnCls = WAVE.strDefault(fClasses.delBtnCls, published.CLS_PS_BTN_DEL);
    fClasses.rawBtnCls = WAVE.strDefault(fClasses.rawBtnCls, published.CLS_PS_BTN_RAW);
    fClasses.clearBtnCls = WAVE.strDefault(fClasses.clearBtnCls, published.CLS_PS_BTN_CLEAR);
    fClasses.modalBtnCls = WAVE.strDefault(fClasses.modalBtnCls, "");

  var fSeed = "ps_" + fPSEditorIDSeed;
  var fControlIds = {
    btnAddId : "addLang_" + fSeed,
    btnRawId : "rawEdit_" + fSeed,
    btnClearId : "clearBtn_" + fSeed,
    divTextTemplateId : "textTemplate_" + fSeed,
    divEditorsContainerId : "divEditorsContainer_" + fSeed,
    divDeleteButtonId : "deleteButtonId_" + fSeed,
    inputId : "_inputId_" + fSeed,
    divLangNameId : "islLabel_" + fSeed,
    outputHiddenId : "hidden_" + fSeed
  };

  var fTexts = WAVE.get(init, "texts", {});
    fTexts.addTitle = getText("add-title", "Add New Language");
    fTexts.addBody = getText("add-body", "");
    fTexts.rawTitle= getText("raw-title", "Raw Edit");
    fTexts.rawBody = getText("raw-body");
    fTexts.clearConfirmTitle = getText("clear-confirm-title", "Confirm Clear");
    fTexts.clearConfirmBody = getText("clear-confirm-body", "Clear all language data?");
    fTexts.delConfirmTitle = getText("del-confirm-title", "Confirm Delete");
    fTexts.delConfirmBody = getText("del-confirm-body");
    fTexts.btnDel = getText("btn-del", "Delete");
    fTexts.btnAdd = getText("btn-add", "Add");
    fTexts.btnRaw = getText("btn-raw", "Edit Raw");
    fTexts.btnClear = getText("btn-clear", "Clear");

  function getText(n, dflt){
    return localize(fTexts[n], dflt);
  }

  function localize(text, dflt){
    return WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, WAVE.strDefault(text, dflt));
  }

  function getNotAddedLangs() {
    var langs = [];
    for (var l in fSets) {
      if (!WAVE.has(fContent, fSets[l]))
        langs.push(fSets[l]);
    }
    return langs;
  }

  function addMainControls() {
    var addControlTemaplate =
      "<input id='@outerId@' type='hidden' name='@outerName@'/>" +
      "<div id='@divEditorsContainerId@' class='@divEditorsContainerClass@'></div>" +
      ((!fReadOnly && !fDisable) ?
        (fCanAdd ? "<input type='button' id='@btnAddId@' class='@btnAddClass@' value='@add@' />" : "") +
        (fCanRaw ? "<input type='button' id='@btnRawId@' class='@btnRawClass@' value='@raw@' />" : "") +
        (fCanDel ? "<input type='button' id='@btnClearId@' class='@btnClearClass@' value='@clear@' />" : "") : "");

    fDIV.innerHTML = WAVE.strHTMLTemplate(addControlTemaplate,
      {
        outerId: fControlIds.outputHiddenId,
        outerName: WAVE.strDefault(fOutputFormFieldName, ''),
        divEditorsContainerId: fControlIds.divEditorsContainerId,
        divEditorsContainerClass: published.CLS_PS_DIV_LANGS_CONTAINER,
        btnAddId: fControlIds.btnAddId,
        btnAddClass: fClasses.addBtnCls,
        btnRawId: fControlIds.btnRawId,
        btnRawClass: fClasses.rawBtnCls,
        btnClearId: fControlIds.btnClearId,
        btnClearClass: fClasses.clearBtnCls,

        add: fTexts.btnAdd,
        raw: fTexts.btnRaw,
        clear: fTexts.btnClear
      });
    if (!fReadOnly && !fDisable && fCanAdd && getNotAddedLangs().length === 0)
      WAVE.id(fControlIds.btnAddId).disabled = true;

    if (fReadOnly || fDisable) return;

    if (fCanAdd && getNotAddedLangs().length > 0) {
      WAVE.id(fControlIds.btnAddId).onclick = function () {
        var btnAdd = this;
        var ls = getNotAddedLangs();
        var opts = "";
        for (var l in ls) {
          opts += "<option>" + ls[l] + "</option>";
        }

        var inputId = "newIso_" + fSeed;
        published.showConfirmationDialog(
          pfxTitle() + fTexts.addTitle,
          "<div>" + fTexts.addBody + "</div>" +
          "<div><select class='" + published.CLS_PS_LANG_SELECTOR + "' id='" + inputId + "'>" + opts + "</select></div>",
          [published.DLG_OK, published.DLG_CANCEL],
          function (sender, result) {
            if (result === published.DLG_OK) {
              var iso = WAVE.id(inputId).value;
              editor.addLang(iso);
              if (ls.length === 1)
                btnAdd.disabled = true;
            }
            return true;
          },
          { btnCls: fClasses.modalBtnCls });
        WAVE.id(inputId).focus();
      };
    }

    if (fCanRaw) {
      WAVE.id(fControlIds.btnRawId).onclick = function () {
        var inputId = "raw" + fSeed;
        published.showConfirmationDialog(
          pfxTitle() + fTexts.rawTitle,
          "<div>" + fTexts.rawBody + "</div>" +
          "<div><textarea style='width: 70vw; height: 70vh;' class='" + published.CLS_PS_TEXTAREA_RAW_EDITOR + "' type='text' id='" + inputId + "'></textarea></div>",
          [published.DLG_OK, published.DLG_CANCEL],
          function (sender, result) {
            if (result === published.DLG_OK) {
              var val = WAVE.id(inputId).value;
              var res = WAVE.tryParseJSON(val);
              if (res.ok){
                  if (WAVE.checkKeysUnique(res.obj)){
                    published.toast(WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, "JSON is valid, but some keys are duplicated"), "error");
                    return false;
                  }
                  editor.content(val);
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

    if (fCanDel) {
      WAVE.id(fControlIds.btnClearId).onclick = function () {
        published.showConfirmationDialog(
          pfxTitle() + fTexts.clearConfirmTitle,
          "<div>"+fTexts.clearConfirmBody+"</div>",
          [published.DLG_OK, published.DLG_CANCEL],
          function (sender, result) {
            if (result === published.DLG_OK) {
              editor.clear();
            }
            return true;
          },
          { btnCls: fClasses.modalBtnCls });
      };
    }
  }

  function getLangContainerId(iso){
    return  fSeed + "_" + iso;
  }

  function buildLang(iso) {
    var langId = getLangContainerId(iso);
    var langEditors = WAVE.id(fControlIds.divEditorsContainerId);
    var delButtonId = fControlIds.divDeleteButtonId + "_" + iso;
    var labelId = fControlIds.divLangNameId + "_" + iso;
    var inputId = fControlIds.inputId + "_" + iso;
    var controlContainerId = fControlIds.divTextTemplateId + "_" + iso;

    function uName(filedName, id){
      return filedName + id;
    }

    var html =
      "<fieldset id='@langId@'>" +
      "<legend id='@isoLabelId@' class='@langClass@'></legend>" +
      "<div class='@textEditorsContainerClass@' id='@controlContainerId@'>" +
      "</div>" +
      (fCanDel && !fReadOnly && !fDisable ? "<input type='button' id='@deleteButtonId@' class='@delCls@' value='@delText@'/>" : "") +
      "</fieldset>";

    var params = {
        langId: langId,
        controlContainerId: controlContainerId,
        deleteButtonId: delButtonId,
        isoLabelId: labelId,
        langClass: published.CLS_PS_DIV_LANG_NAME,
        labelContainerClass: published.CLS_PS_LABEL_CONTAINER,
        textEditorsContainerClass: published.CLS_PS_INPUT_CONTAINER,
        delCls: fClasses.delBtnCls,
        disabled: fDisable ? "disabled" : "",
        readonly: fReadOnly ? "readonly" : "",

        delText: fTexts.btnDel
      };

    langEditors.insertAdjacentHTML("beforeend", WAVE.strHTMLTemplate(html, params));
    WAVE.id(labelId).innerText = iso;

    var ed = new published.ObjectEditor({
      DIV: WAVE.id(controlContainerId),
      meta: {
        type: "object",
        schema: fSchema
      },
      content: fContent[iso],
      disabled : fDisable,
      readonly : fReadOnly
    });

    ed.eventBind(published.EVT_OBJECT_EDITOR_UPDATED, function (e, d) {
      fContent[iso] = d;
      editor.content(JSON.stringify(fContent), false);
    });

    if (fCanDel && !fReadOnly && !fDisable) {
      WAVE.id(delButtonId).onclick = function () {
        published.showConfirmationDialog(
          pfxTitle() + fTexts.delConfirmTitle + " : " + iso.toUpperCase(),
          "<div>"+fTexts.delConfirmBody+"</div>",
          [published.DLG_OK, published.DLG_CANCEL],
          function (sender, result) {
            if (result === published.DLG_OK) {
              editor.deleteLang(iso);
            }
            return true;
          },
          { btnCls: fClasses.modalBtnCls });
      };
    }
  }

  function buildEditors() {
    if (WAVE.isObject(fContent))
      for (var iso in fContent) buildLang(iso);
  }

  function rebuild() {
    while (fDIV.hasChildNodes()) {
      fDIV.removeChild(fDIV.lastChild);
    }
    addMainControls();
    buildEditors();
    setHiddenValue();
  }

  function setHiddenValue(){
      if (fOutputFormFieldName !== null) WAVE.id(fControlIds.outputHiddenId).value = editor.content();
  }

  addMainControls();

  this.DIV = function() { return fDIV; };

  this.jsonContent = function () { return fContent; };

  this.content = function (value, rebuild) {
    if (typeof (value) === tUNDEFINED)
      return WAVE.empty(fContent) ? "" : JSON.stringify(fContent, null, 2);

    var parsed = WAVE.tryParseJSON(value);
    if (parsed.ok) {
      rebuild = WAVE.strAsBool(rebuild, true);
      if (rebuild) {
        for (var iso in fContent) {
          WAVE.removeElem(getLangContainerId(iso));
        }
      }

      fContent = WAVE.memberClone(parsed.obj);

      if(rebuild) buildEditors();

      setHiddenValue();
      editor.eventInvoke(published.EVT_PS_EDITOR_UPDATED, fContent);
    }
    return value;
  };

  this.title = function(value) {
      if (typeof (value) === tUNDEFINED) return fTitle;
      fTitle = WAVE.strDefault(value);
  };

  function pfxTitle(){
    return WAVE.strEmpty(fTitle) ? "" : (fTitle + ": ");
  }

  this.enable = function (value) {
    if (typeof(value) === tUNDEFINED) return !fDisable;

    fDisable = !(value===true);
    rebuild();
    return value;
  };

  this.readOnly = function (value) {
    if (typeof(value) === tUNDEFINED) return fReadOnly;

    fReadOnly = value===true;
    rebuild();
    return value;
  };

  this.canAdd = function (value) {
    if (typeof(value) === tUNDEFINED) return fCanAdd;

    fCanAdd = value===true;
    rebuild();
    return value;
  };

  this.canDel = function (value) {
    if (typeof(value) === tUNDEFINED) return fCanDel;

    fCanDel = value===true;
    rebuild();
    return value;
  };

  this.canRaw = function (value) {
    if (typeof (value) === tUNDEFINED) return fCanRaw;

    fCanRaw = value===true;
    rebuild();
    return value;
  };

  this.addLang = function (iso) {
    if (WAVE.strEmpty(iso)) return false;

    iso = iso.toLowerCase();
    if (WAVE.has(fContent, iso)) return false;

    fContent[iso] = {};
    buildLang(iso);

    editor.content(JSON.stringify(fContent));
    return true;
  };

  this.deleteLang = function (iso) {
    if (WAVE.strEmpty(iso)) return false;

    iso = iso.toLowerCase();
    if (!WAVE.has(fContent, iso)) return false;

    delete fContent[iso];
    WAVE.removeElem(getLangContainerId(iso));
    if (!fReadOnly && !fDisable) WAVE.id(fControlIds.btnAddId).disabled = false;
    editor.content(JSON.stringify(fContent));
    return true;
  };

  this.clear = function () {
    if (!fReadOnly && !fDisable) WAVE.id(fControlIds.btnAddId).disabled = false;
    editor.content("{}");
  };

  rebuild();
  return editor;
};//PropSetEditor