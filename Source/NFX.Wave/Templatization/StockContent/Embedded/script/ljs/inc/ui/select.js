var fSelectSeed = 0;
//{
//  DIV: id | node
//  data: JSON | {} | []
//  mode: 0(single) | 1(multi)
//  autocomplete: true | false
//  formName: string
//}
published.Select = function (init, choiceUI) {
  if (!WAVE.exists(init)) throw "Select.ctor(init=null)";
  if (WAVE.strEmpty(init.DIV)) throw "Select.ctor(init.DIV=null)";

  var fDIV = WAVE.isString(init.DIV)
                ? WAVE.id(init.DIV)
                : init.DIV;
  if (fDIV === null) throw "Select.ctor(DIV=null)";

  var select = this,
      fChoiceUI = WAVE.isFunction(choiceUI)
                      ? choiceUI
                      : published.Choice,
      fChoiceBuilder = WAVE.isFunction(init.choiceBuilder) ? init.choiceBuilder : null,
      fData = null,
      fFormName = WAVE.strDefault(init.formName),
      fElemContainer = null,
      fElemCombobox = null,
      fElemDisplay = null,
      fElemArrow = null,
      fElemAutocomplete = null,
      fElemDropdown = null,
      fElemSelection = null,
      fElemAutocompleteContainer = null,
      fElemChoicesContainer = null,
      fDropdownShow = false,
      fAutocomplete = WAVE.strAsBool(init.autocomlete, true),
      fFilterTimer = null,
      fDisable = WAVE.strAsBool(init.disable),
      fMode = __parseMode(init.mode),
      fChoice = null,
      fSelectedChoices = {},
      fSelectedIds = [],
      fMatch = WAVE.isFunction(init.match) ? init.match : __match,
      fFilter = WAVE.isFunction(init.filter) ? init.filter : __filter,
      fUpdateData = WAVE.isFunction(init.updateData) ? init.updateData : __resetData,
      fButtonsNotFireFilter = [9, 16, 17, 18, 19, 20, 27, 37, 38, 39, 40],
      fIdSeed = "select_" + fSelectSeed++,
      fIds = {
        container : __IdFor("container"),
        combobox: __IdFor("combobox"),
        display: __IdFor("display"),
        arrow: __IdFor("select"),
        autocomlete: __IdFor("autocomplete"),
        dropdown: __IdFor("dropdown"),
        selection: __IdFor("selection"),
        autoContainer: __IdFor("autoContainer"),
        choicesContainer: __IdFor("choicesContainer")
      };

  WAVE.extend(select, WAVE.EventManager);

  function __IdFor(what) { return fIdSeed + "_" + what; }

  function __createElemet(tag, id, className) {
    var element = document.createElement(tag);
    element.id = id;
    if(!WAVE.strEmpty(className))
      element.className = className;
    return element;
  }

  function __parseMode(val) {
    return published.SELECT_MODES.Multi === val
                    ? published.SELECT_MODES.Multi
                    : published.SELECT_MODES.Single;
  }

  function __resetTimer() {
    if (fFilterTimer !== null) {
      clearTimeout(fFilterTimer);
      fFilterTimer = null;
    }
  }

  function __ensureInitData() {
    var data = init.data;
    if (typeof(data) !== tUNDEFINED && data !== null) {
      if (WAVE.isString(data)) {
        var res = WAVE.tryParseJSON(data);
        if (res.ok)
          fData = res.obj;
        else
          throw "Select.ctor(init.data invalid JSON)";
      } else
        fData = init.data;
      return;
    }
  }

  function __dropdownHide() {
    var args = {
      phase: published.EVT_PHASE_BEFORE,
      sender: select,
      abort: false,
      refresh: true
    };
    select.eventInvoke(published.EVT_SELECT_CLOSE, args);
    if (args.abort === true) return;

    __resetTimer();

    if (args.refresh === true) {
      fFilter([fChoice], "");
      fElemAutocomplete.value = "";
    }

    fDropdownShow = false;
    WAVE.addClass(fElemDropdown, published.CLS_SELECT_DROPDOWN_HIDDEN);
    WAVE.removeClass(fElemDropdown, published.CLS_SELECT_DROPDOWN_SHOWN);
    WAVE.removeEventHandler(window, "click", __outsideClickHandler);

    args.result = false;
    args.phase = published.EVT_PHASE_AFTER;
    delete args.abort;
    select.eventInvoke(published.EVT_SELECT_CLOSE, args);
  }

  function __dropdownShow() {
    var args = {
      phase: published.EVT_PHASE_BEFORE,
      sender: select,
      abort: false
    };
    select.eventInvoke(published.EVT_SELECT_OPEN, args);
    if (args.abort === true) return;

    fDropdownShow = true;
    WAVE.addClass(fElemDropdown, published.CLS_SELECT_DROPDOWN_SHOWN);
    WAVE.removeClass(fElemDropdown, published.CLS_SELECT_DROPDOWN_HIDDEN);
    WAVE.addEventHandler(window, "click", __outsideClickHandler);

    args.result = true;
    args.phase = published.EVT_PHASE_AFTER;
    delete args.abort;
    select.eventInvoke(published.EVT_SELECT_OPEN, args);
  }

  function __toggleDropdown(val) {
    if(typeof(val) === tUNDEFINED || val === null)
      val = !fDropdownShow;

    if (val === true)
      __dropdownShow();
    else
      __dropdownHide();
  }

  function __outsideClickHandler(e) {
    var target = e.target;
    if (!WAVE.isParentOf(fElemContainer, target)) {
      __dropdownHide();
    }
  }

  function __match(choice, term) {
    if (choice.value().toUpperCase().indexOf(term.toUpperCase()) > -1)
      return true;
    else
      return false;
  }

  function __filter(choices, term) {
    var arr = choices;
    for(var i = 0; i < arr.length; i++) {
      var choice = arr[i];
      if (choice === null) continue;
      var children = choice.children();

      if (children.length === 0 && choice.value() === null) continue;

      if (children.length > 0)
        __filter(children, term);
      else
        choice.visible(__match(choice, term));
    }
  }

  function __choiceUnselected(choice) {
    var args = {
      phase: published.EVT_PHASE_BEFORE,
      sender: select,
      abort: false
    };
    select.eventInvoke(published.EVT_SELECT_UNSELECT, args);
    if (args.abort === true) return;

    if (fMode === published.SELECT_MODES.Single) {
      fSelectedChoices = {};
      fSelectedIds = [];
    } else {
      delete fSelectedChoices[choice.id()];
      var idx = fSelectedIds.indexOf(choice.id());
      if (idx > -1)
        fSelectedIds.splice(idx, 1);
    }
    choice.destroyView();
    fElemSelection.value = select.value();

    args.result = fElemSelection.value;
    args.phase = published.EVT_PHASE_AFTER;
    delete args.abort;
    select.eventInvoke(published.EVT_SELECT_UNSELECT, args);
  }

  function __choiceSelected(choice) {
    var args = {
      phase: published.EVT_PHASE_BEFORE,
      sender: select,
      abort: false
    };
    select.eventInvoke(published.EVT_SELECT_SELECT, args);
    if (args.abort === true) return;

    if (fMode === published.SELECT_MODES.Single) {
      if (fSelectedIds.length > 0 && fSelectedIds[0] !== choice.id()) {
        var prev = fSelectedChoices[fSelectedIds[0]];
        prev.isSelected(false);
        prev.destroyView();
        fSelectedChoices = {};
        fSelectedIds = [];
      }
    }

    fSelectedChoices[choice.id()] = choice;
    fSelectedIds.push(choice.id());
    choice.renderSelection(fElemDisplay, function() {
      choice.isSelected(false);
      __choiceUnselected(choice);
    });
    fElemSelection.value = select.value();

    args.result = fElemSelection.value;
    args.phase = published.EVT_PHASE_AFTER;
    delete args.abort;
    select.eventInvoke(published.EVT_SELECT_SELECT, args);
  }

  function __choiceClickHandler(object, args) {
    if (args.phase === published.EVT_PHASE_BEFORE) return;

    var selected = args.sender.isSelected();
    if (fMode === published.SELECT_MODES.Single)
    {
      if (!selected) {
        if (!args.sender.isSelected(true)) return;
        __toggleDropdown();
        __choiceSelected(args.sender);
      }
    } else {
      if (selected) {
        if (args.sender.isSelected(false))
          __choiceUnselected(args.sender);
      }
      else
        if (args.sender.isSelected(true))
          __choiceSelected(args.sender);
    }
  }

  function __destroyChoice() {
    fChoice.eventUnbind(published.EVT_SELECT_CHOICE_CLICK, __choiceClickHandler);
    fChoice.destroy();
    fChoice = null;
  }

  function __resetData(data) {
    var empty = typeof(data) === tUNDEFINED || data === null;
    if (empty && fChoice !== null) {
      __destroyChoice();
      return;
    }

    fSelectedIds = [];
    fSelectedChoices = {};

    if (fChoice !== null)
      __destroyChoice();

    if (!empty) {
      fChoice = new fChoiceUI({data: data, DIV: fElemChoicesContainer, builder: fChoiceBuilder, disable: fDisable});
      fChoice.eventBind(published.EVT_SELECT_CHOICE_CLICK, __choiceClickHandler);
    }
  }

  function __tv(choice, val) {
    var ch = choice.children(),
        l = ch.length;
    if (l === 0) {
      if (choice.trySetValue(val)) {
        __choiceSelected(choice);
        return true;
      }
    } else {
      var result = false;
      for(var i = 0; i < l; i++) {
        var s = __tv(ch[i], val);
        result = result && s;
      }
      return result;
    }
  }

  function __setValue(val) {
    if (fChoice === null) return false;

    fChoice.resetSelection();
    for(var j = 0, k = fSelectedIds.length; j < k; j++) {
        fSelectedChoices[fSelectedIds[j]].destroyView();
    }
    fSelectedIds = [];
    fSelectedChoices = {};
    if (fMode === published.SELECT_MODES.Single) {
      return __tv(fChoice, val);
    } else {
      var vs = val.split(';');
      var res = false, tr;
      for(var i = 0, l = vs.length; i < l; i++) {
        tr = __tv(fChoice, vs[i]);
        res = res && tr;
      }
      return res;
    }
  }

  function __build() {
    var cls = fMode === published.SELECT_MODES.Multi ?
              published.CLS_SELECT_MODE_MULTI :
              published.CLS_SELECT_MODE_SINGLE;

    fElemContainer = __createElemet("div", fIds.container, published.CLS_SELECT_CONTAINER + " " + cls);
    fDIV.appendChild(fElemContainer);

    fElemSelection = __createElemet("input", fIds.selection);
    fElemSelection.type = "hidden";
    fElemContainer.appendChild(fElemSelection);
    if (!WAVE.strEmpty(fFormName))
      fElemSelection.name = fFormName;

    fElemCombobox = __createElemet("div", fIds.combobox, published.CLS_SELECT_COMBOBOX);
    fElemContainer.appendChild(fElemCombobox);

    fElemDisplay = __createElemet("div", fIds.display, published.CLS_SELECT_DISPLAY);
    fElemCombobox.appendChild(fElemDisplay);

    fElemArrow = document.createElement("div");
    fElemArrow.className = published.CLS_SELECT_ARROW;
    fElemCombobox.appendChild(fElemArrow);
    WAVE.addEventHandler(fElemArrow, "click", function() { __toggleDropdown(); });

    fElemDropdown = __createElemet("div", fIds.dropdown, published.CLS_SELECT_DROPDOWN + " " + published.CLS_SELECT_DROPDOWN_HIDDEN);
    fElemDropdown.style.zIndex = CURTAIN_ZORDER + fCurtains.length + 1;
    fElemContainer.appendChild(fElemDropdown);

    fElemAutocompleteContainer = __createElemet("div", fIds.autoContainer, published.CLS_SELECT_AUTO_CONTAINER);
    fElemDropdown.appendChild(fElemAutocompleteContainer);

    fElemChoicesContainer = __createElemet("div", fIds.choicesContainer, published.CLS_SELECT_CHOICES_CONTAINER);
    fElemDropdown.appendChild(fElemChoicesContainer);

    if (fAutocomplete) {
      fElemAutocomplete = __createElemet("input", fIds.autocomlete, published.CLS_SELECT_AUTOCOMPETE);
      fElemAutocomplete.placeholder = "Start enter..";
      if (fMode === published.SELECT_MODES.Single)
        fElemAutocompleteContainer.appendChild(fElemAutocomplete);
      else
        fElemCombobox.appendChild(fElemAutocomplete);

      WAVE.addEventHandler(fElemAutocomplete, "keyup", function(e) {
        if (fButtonsNotFireFilter.indexOf(e.keyCode) > -1)
          return;

        __resetTimer();
        fFilterTimer = setTimeout(function() {
          fFilter([fChoice], fElemAutocomplete.value);
          if (!fDropdownShow)
            __dropdownShow();
        }, 800);
      });
      WAVE.addEventHandler(fElemAutocomplete, "click", __dropdownShow);
    }
    __resetData(fData);
  }

  select.selected = function() {
    return fSelectedChoices;
  };

  select.mode = function() { return fMode; }

  select.value = function(val) {
    if(typeof(val) === tUNDEFINED) {
      var result = "";
      for(var v in fSelectedChoices)
        result += fSelectedChoices[v].key() + ";";
      return result.slice(0, -1);
    } else
      __setValue(val.toString());
  };

  select.data = function(val) {
    if (typeof(val) === tUNDEFINED)
      return val;

    fData = val;
    __resetData(fData);
  };

  select.updateData = function(val) {
    fUpdateData(val);
  };

  select.filter = function(term) {
    fFilter([fChoice], typeof(term) === tUNDEFINED ? fElemAutocomplete.value : term);
  };

  select.hasChoice = function() { return fChoice !== null; };

  select.latchAllEvents = function(delta) {
    select.eventInvocationSuspendCount += delta;
    if(fChoice !== null) fChoice.latchAllEvents(delta);
  };

  __ensureInitData();
  __build();

  return select;
};//Select

published.CLS_SELECT_CHOICE = "wvChoice";
published.CLS_SELECT_CHOICE_DISABLED = "wvChoiceDisabled";
published.CLS_SELECT_CHOICE_SELECTED = "wvSelected";
published.CLS_SELECT_CHOICE_PARENT = "wvChoiceParent";
published.CLS_SELECT_CHOICE_PARENT_LABELED = "wvChoiceParentLabeled";
published.CLS_SELECT_CHOICE_LABEL = "wvChoiceLabel";
published.CLS_SELECT_CHOICE_VALUE_BOX = "wvChoiceValueBox";
published.CLS_SELECT_CHOICE_UNSELECT = "wvChoiseUnselect";

published.EVT_SELECT_CHOICE_SELECTED = "wv-choice-selected";
published.EVT_SELECT_CHOICE_UNSELECTED = "wv-choice-unselected";
published.EVT_SELECT_CHOICE_CLICK = "wv-choise-choice-click";

var fChoiceSeed = 0;
published.Choice = function(init) {

  var choice = this,
      fDIV = init.DIV,
      fChildren = [],
      fDisable = WAVE.strAsBool(init.disable, false),
      fVisible = true,
      fIsSelected = false,
      fRendered = false,
      fData = init.data,
      fKey = WAVE.strEmpty(init.key) ? null : init.key,
      fValue = WAVE.strEmpty(init.value) ? null : init.value,
      fElemValueBox = null,
      fElemChoice = null,
      fElemUnselect = null,
      fStlDisplay = null,
      fCLS = WAVE.strDefault(init.cls),
      fOnCloseClick = null,
      fOnClick = WAVE.isFunction(init.onclick) ? init.onclick : __click,
      fBuilder = init.builder,
      fId = "wv_choice_" + fChoiceSeed++,
      fIds = {
        unselect: fId + "_unselect",
        valueBox: fId + "_value"
      };

  WAVE.extend(choice, WAVE.EventManager);

  function __select(tryValue) {
    if (fIsSelected) return false;

    var args = {
      phase: published.EVT_PHASE_BEFORE,
      sender: choice,
      abort: false
    };
    choice.eventInvoke(published.EVT_SELECT_CHOICE_SELECTED, args);
    if (args.abort === true) return false;

    fIsSelected = typeof(tryValue) === tUNDEFINED ? true : fKey === tryValue;
    if (fIsSelected)
      WAVE.addClass(fElemChoice, published.CLS_SELECT_CHOICE_SELECTED);

    args.result = fIsSelected;
    args.phase = published.EVT_PHASE_AFTER;
    delete args.abort;
    choice.eventInvoke(published.EVT_SELECT_CHOICE_SELECTED, args);

    return fIsSelected;
  }

  function __unselect() {
    if (!fIsSelected) return false;

    var args = {
      phase: published.EVT_PHASE_BEFORE,
      sender: choice,
      abort: false
    };
    choice.eventInvoke(published.EVT_SELECT_CHOICE_UNSELECTED, args);
    if (args.abort === true) return false;

    fIsSelected = false;
    WAVE.removeClass(fElemChoice, published.CLS_SELECT_CHOICE_SELECTED);

    args.result = false;
    args.phase = published.EVT_PHASE_AFTER;
    delete args.abort;
    choice.eventInvoke(published.EVT_SELECT_CHOICE_UNSELECTED, args);

    return true;
  }

  function __toggleSelection(val, tryValue) {
    var result;
    if (fChildren.length > 0) {
      for(var i = 0, l = fChildren.length; i < l; i++) {
        if (!WAVE.strEmpty(tryValue))
          result = result || fChildren[i].trySetValue(tryValue);
        else
          result = result && fChildren[i].isSelected(val);
      }
      return result;
    }
    else {
      if (val === true || !WAVE.strEmpty(tryValue))
        return __select(tryValue);
      else
        return __unselect();
    }
  }

  function __click(e, o) {
    if (fDisable === true)
      return;

    var args = {
      phase: published.EVT_PHASE_BEFORE,
      sender: choice,
      abort: false
    };
    choice.eventInvoke(published.EVT_SELECT_CHOICE_CLICK, args);
    if (args.abort === true) return;

    args.phase = published.EVT_PHASE_AFTER;
    delete args.abort;
    choice.eventInvoke(published.EVT_SELECT_CHOICE_CLICK, args);
  }

  function __hide() {
    if (!fVisible) return;

    fVisible = false;
    if (fStlDisplay === null)
      fStlDisplay = WAVE.styleOf(fElemChoice, "display");
    fElemChoice.style.display = "none";
  }

  function __show() {
    if (fVisible) return;

    fVisible = true;
    fElemChoice.style.display = fStlDisplay === null ? "block" : fStlDisplay;
  }

  function __toggleVisibility(val) {
    if (typeof(val) === tUNDEFINED || val === null)
      val = !fVisible;

    if (val === true)
      return __show();
    else
      return __hide();
  }

  function __selectedEventHandler(object, args) {
    choice.eventInvoke(published.EVT_SELECT_CHOICE_SELECTED, args);
  }

  function __unselectedEventHandler(object, args) {
    choice.eventInvoke(published.EVT_SELECT_CHOICE_UNSELECTED, args);
  }

  function __clickEventHandler(object, args) {
    choice.eventInvoke(published.EVT_SELECT_CHOICE_CLICK, args);
  }

  function __addChild(child) {
    child.eventBind(published.EVT_SELECT_CHOICE_SELECTED, __selectedEventHandler);
    child.eventBind(published.EVT_SELECT_CHOICE_UNSELECTED, __unselectedEventHandler);
    child.eventBind(published.EVT_SELECT_CHOICE_CLICK, __clickEventHandler);
    fChildren.push(child);
  }

  function __createChild(cData, cDiv) {
    var child = new published.Choice({data: cData, DIV: cDiv, builder: fBuilder});
    __addChild(child);
  }

  function __onClickInvoke(e) { fOnClick(e, choice); }

  function __initElemChoice() {
    fElemChoice = document.createElement("div");
    fElemChoice.id = fId;
    fDIV.appendChild(fElemChoice);
    WAVE.addEventHandler(fElemChoice, "click", __onClickInvoke);
    if (!fDisable)
      fElemChoice.className = published.CLS_SELECT_CHOICE;
    else
      fElemChoice.className = published.CLS_SELECT_CHOICE_DISABLED;
    if (fCLS !== null)
      WAVE.addClass(fElemChoice, fCLS);
  }

  function __buildAsKeyValue(key, value) {
    __initElemChoice();
    fElemChoice.innerHTML = value;
    fValue = value;
    fKey = key;
  }

  function __buildAsArray(arrValue) {
    fElemChoice = document.createElement("div");
    fElemChoice.id = fId;
    fElemChoice.className = published.CLS_SELECT_CHOICE_PARENT;
    fDIV.appendChild(fElemChoice);

    for(var i = 0; i < arrValue.length; i++) {
      var value = arrValue[i];
      __createChild(value, fElemChoice);
    }
  }

  function __buildAsObject(objValue) {
    var keys = Object.keys(objValue),
        key,
        value,
        label;

    if (keys.length === 1) {
      key = keys[0];
      value = objValue[key];
      if (WAVE.isString(value) || !WAVE.isMapOrArray(value)) {
        __buildAsKeyValue(key, value.toString());
      } else {
        fElemChoice = document.createElement("div");
        fElemChoice.id = fId;
        fElemChoice.className = published.CLS_SELECT_CHOICE_PARENT + " " +
                                published.CLS_SELECT_CHOICE_PARENT_LABELED;
        fDIV.appendChild(fElemChoice);

        label = document.createElement("div");
        label.className = published.CLS_SELECT_CHOICE_LABEL;
        label.innerHTML = key;
        fElemChoice.appendChild(label);

        __createChild(value, fElemChoice);
      }
    } else {
      fElemChoice = document.createElement("div");
      fElemChoice.id = fId;
      fElemChoice.className = published.CLS_SELECT_CHOICE_PARENT + " " +
                              published.CLS_SELECT_CHOICE_PARENT_LABELED;
      fDIV.appendChild(fElemChoice);

      for(key in objValue) {
        label = document.createElement("div");
        label.className = published.CLS_SELECT_CHOICE_LABEL;
        label.innerHTML = key;
        fElemChoice.appendChild(label);

        value = objValue[key];
        __createChild(value, fElemChoice);
      }
    }
  }

  function __build(data, DIV) {
    if (typeof(data) === tUNDEFINED || data === null)
      return;

    if (WAVE.isString(data))      __buildAsKeyValue(data, data);
    else if (WAVE.isArray(data))  __buildAsArray(data);
    else if (WAVE.isObject(data)) __buildAsObject(data);
    else __buildAsKeyValue(data.toString(), data.toString());
  }

  function __destroyView() {
    WAVE.removeEventHandler(fElemUnselect, "click", fOnCloseClick);
    WAVE.removeElem(fIds.unselect);
    WAVE.removeElem(fIds.valueBox);
    fOnCloseClick = null;
    fRendered = false;
  }

  function __destroy() {
    WAVE.removeEventHandler(fElemChoice, "click", __onClickInvoke);
    WAVE.removeElem(fId);
    choice.eventUnbind(published.EVT_SELECT_CHOICE_SELECTED, __selectedEventHandler);
    choice.eventUnbind(published.EVT_SELECT_CHOICE_UNSELECTED, __unselectedEventHandler);
    choice.eventUnbind(published.EVT_SELECT_CHOICE_CLICK, __clickEventHandler);
    for(var i = 0; i < fChildren.length; i++)
      fChildren[i].destroy();
  }

  choice.disable = function(val) {
    if (typeof(val) === tUNDEFINED || val === null) {
      if (fDisable === null)
        fDisable = fChildren.length > 0;
      return fDisable;
    }
    fDisable = WAVE.strAsBool(val, true);
      //TODO fire selectable change
  };

  choice.isSelected = function(val) {
    if (typeof(val) === tUNDEFINED || val === null)
      return fIsSelected;
    return __toggleSelection(val);
  };

  choice.trySetValue = function(val) {
    if (typeof(val) === tUNDEFINED || val === null) return false;

    return __toggleSelection(null, val);
  }

  choice.resetSelection = function() {//TODO add val
    if (fChildren.length === 0)
      __toggleSelection(false);
    else
      for(var i = 0, l = fChildren.length; i < l; i++)
        fChildren[i].resetSelection();
  }

  choice.visible = function(val) {
    if (typeof(val) === tUNDEFINED || val === null)
      return fVisible;

    return __toggleVisibility(val);
  };

  choice.disable = function() { };

  choice.id = function() { return fId; };
  choice.key = function() { return fKey; };
  choice.value = function() { return fValue; };
  choice.children = function() { return fChildren; };
  choice.elem = function() { return fElemChoice; };

  choice.addChild = function(child) {
    __addChild(child);
  };

  choice.renderSelection = function(div, onCloseClick) {
    if (!fIsSelected ||
        fRendered === true ||
        typeof(div) === tUNDEFINED ||
        div === null)
      return;

    var html = "<div>@value@</div>" +
                "<div class='@clsUnselect@' id='@idUnselect@'>";
    var view = WAVE.strHTMLTemplate(html, {
      value : fValue,
      clsUnselect : published.CLS_SELECT_CHOICE_UNSELECT,
      idUnselect : fIds.unselect
    });
    fElemValueBox = document.createElement("div");
    fElemValueBox.className = published.CLS_SELECT_CHOICE_VALUE_BOX;
    fElemValueBox.innerHTML = view;
    fElemValueBox.id = fIds.valueBox;

    div.appendChild(fElemValueBox);

    fElemUnselect = WAVE.id(fIds.unselect);
    fOnCloseClick = function() { onCloseClick(choice); };
    fRendered = true;
    WAVE.addEventHandler(fElemUnselect, "click", fOnCloseClick);
  };

  choice.destroyView = function() {
    __destroyView();
  };

  choice.destroy = function() {
    __destroyView();
    __destroy();
  };

  choice.latchAllEvents = function(delta) {
    choice.eventInvocationSuspendCount += delta;
    for(var i = 0, l = fChildren.length; i < l; i++)
      fChildren[i].eventInvocationSuspendCount += delta
  };

  if (WAVE.isFunction(fBuilder)) {
    __initElemChoice();
    fBuilder(choice, fData, fElemChoice);
  }
  else
    __build(fData, fDIV);

  return choice;
};//Choice