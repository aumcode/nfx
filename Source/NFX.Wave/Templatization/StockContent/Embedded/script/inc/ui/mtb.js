  // {
  //   "DIV": html container,
  //   "id" : "uniqueId",
  //   "value" : "",
  //   "disabled" : false,
  //   "readonly" : false,
  //   "placeholder" : "some text",
  //   "addLabel" : true | false,
  //   "attrs" : {attr : value}
  // }
    published.MultiLineTextBox = function (init) {
      if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "MultiLineTextBox.ctor(init.DIV)";
      if (WAVE.strEmpty(init.id)) throw "MultiLineTextBox.ctor(init.id)";

      var editor = this;
      WAVE.extend(editor, WAVE.EventManager);
      var fDIV = init.DIV;
      var fId = init.id;
      var fAddLabel = WAVE.strAsBool(init.addLabel, true);
      var fShadowDivId = "shdw_div_" + fId;
      var fPlaceholder = WAVE.strDefault(init.placeholder);
      var fReadOnly = WAVE.strAsBool(init.readonly, false);
      var fDisabled = WAVE.strAsBool(init.disabled, false);
      var fValue = WAVE.strDefault(init.value);
      var fAttrs = WAVE.tryParseJSON(init.attrs).obj;

      var fTemplate =
        "<div id='@shdwDivId@' style='position: fixed;visibility: hidden;height: auto;width: auto;'></div>" +
        (fAddLabel ? "<label for='@id@'>@placeholder@</label>" : "") +
        "<textarea id='@id@' @disabled@ @readonly@ style='resize: none' placeholder='@placeholder@' title='@placeholder@'></textarea>";

      fDIV.innerHTML = WAVE.strHTMLTemplate(fTemplate, {
        shdwDivId: fShadowDivId,
        id : fId,
        disabled: fDisabled ? "disabled" : "",
        readonly: fReadOnly ? "readonly" : "",
        placeholder : fPlaceholder
      });

      var dI = WAVE.id(fId);
      var shadowDiv = WAVE.id(fShadowDivId);

      dI.onresize = function () { shadowDiv.style.maxWidth = WAVE.styleOf(dI, 'width'); };
      dI.style.overflow = 'hidden';
      for(var n in fAttrs){
        dI.setAttribute(n, fAttrs[n]);
      }

      shadowDiv.innerText = dI.value = fValue;
      shadowDiv.style.maxWidth = WAVE.styleOf(dI, 'width');
      shadowDiv.style.font = WAVE.styleOf(dI, 'font');
      shadowDiv.style.minHeight = WAVE.styleOf(dI, 'height');
      shadowDiv.style.paddingTop = WAVE.styleOf(dI, 'padding-top');
      shadowDiv.style.paddingRight = WAVE.styleOf(dI, 'padding-right');
      shadowDiv.style.paddingBottom = WAVE.styleOf(dI, 'padding-bottom');
      shadowDiv.style.paddingLeft = WAVE.styleOf(dI, 'padding-left');
      shadowDiv.style.wordWrap = WAVE.styleOf(dI, 'word-wrap');
      shadowDiv.style.wordSpacing = WAVE.styleOf(dI, 'word-spacing');
      shadowDiv.style.whiteSpace = WAVE.styleOf(dI, 'white-space');
      shadowDiv.style.borderSpacing = WAVE.styleOf(dI, 'border-spacing');
      shadowDiv.style.letterSpacing = WAVE.styleOf(dI, 'letter-spacing');
      shadowDiv.style.lineHeight = WAVE.styleOf(dI, 'line-height');
      shadowDiv.style.boxSizing = WAVE.styleOf(dI, 'box-sizing');
      shadowDiv.style.border = WAVE.styleOf(dI, 'border');
      shadowDiv.style.borderWidth = WAVE.styleOf(dI, 'border-width');
      shadowDiv.style.borderRadius = WAVE.styleOf(dI, 'border-radius');
      shadowDiv.style.textIndent = WAVE.styleOf(dI, 'text-indent');

      var setAreaHeight = function (control) {
        shadowDiv.innerText = WAVE.strEmpty(control.value) ? 'W' : control.value.replace(/\r\n|\r|\n/g, "\n ");
        control.style.height = shadowDiv.clientHeight + "px";
      };

      dI.onchange = function (e) {
        setAreaHeight(this);
        shadowDiv.style.maxWidth = WAVE.styleOf(dI, 'width');
        editor.eventInvoke(published.EVT_MULTI_LINE_TEXT_UPDATED, { phase: published.EVT_PHASE_AFTER, target: e.target, value: this.value });
      };
      dI.onkeydown = dI.onkeyup = function (e) {
        setAreaHeight(this);
      };

      setAreaHeight(dI);
      return editor;
    };//MultiLineTextBox