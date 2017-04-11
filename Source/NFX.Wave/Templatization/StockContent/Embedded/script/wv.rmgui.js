WAVE.RMGUI = (function(){

    var tUNDEFINED = "undefined";

    var published = {
        CLS_ERROR: 'wvError',
        CLS_REQ: 'wvRequired',
        CLS_MOD: 'wvModified',
        CLS_PUZZLE: 'wvPuzzle',
        CLS_COMBO_ARROW: 'wvComboArrow',
        CLS_COMBO_WRAP: 'wvComboWrap'
    };

        var AIKEY = "__wv.rmgui";
        var fErroredInput = null;

        function renderTestControl(root, ctx) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('div');
          Ø1.setAttribute('div', ctx.about);
          var Ø2 = WAVE.ce('input');
          Ø2.setAttribute('type', 'text');
          Ø2.setAttribute('name', ctx.fName);
          Ø2.setAttribute('value', ctx.fValue);
          Ø1.appendChild(Ø2);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function buildTestControl(fldView){
          var divRoot = fldView.DIV();
          WAVE.removeChildren(divRoot);
          renderTestControl(divRoot, {
            about: fldView.field().about(),
            fName: fldView.field().name(),
            fValue: fldView.field().value()
          });
        }

        function genIDSeed(fldView){
          var id = WAVE.genAutoincKey(AIKEY);
          return "_"+fldView.recView().ID()+"_"+id;
        }

        function textChangeHandler(e) {
          var target = e.target,
              val = target.value,
              field = target.__fieldView.field();

          try { field.setGUIValue(val); }
          catch(e)
          {
            WAVE.GUI.toast(WAVE.strHTMLTemplate("Wrong value for field '@about@'. Please re-enter or undo", {about: field.about()}), "error");
          }
        }

        function renderTextBox(root, ctx) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('div');
          var Ø2 = WAVE.ce('label');
          Ø2.innerText = ctx.about;
          Ø2.setAttribute('for', ctx.tbid);
          Ø2.setAttribute('class', ctx.cls);
          Ø1.appendChild(Ø2);
          if(ctx.error) {
            var Ø3 = WAVE.ce('div');
            Ø3.innerText = ctx.error;
            Ø3.setAttribute('class', ctx.ec);
            Ø1.appendChild(Ø3);
          }
          var Ø4 = WAVE.ce('input');
          Ø4.setAttribute('id', ctx.id);
          Ø4.setAttribute('type', ctx.tp);
          Ø4.setAttribute('name', ctx.name);
          Ø4.disabled = ctx.disabled;
          Ø4.readonly = ctx.readonly;
          Ø4.setAttribute('value', ctx.value);
          Ø4.setAttribute('placeholder', ctx.placeholder);
          Ø4.required = ctx.required;
          Ø4.__fieldView = ctx.fw;
          Ø4.addEventListener('change', ctx.handler, false);
          Ø1.appendChild(Ø4);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function buildTextBox(fldView){
          var field = fldView.field(),
              divRoot = fldView.DIV(),
              ids = genIDSeed(fldView),
              idInput = "tb"+ids,
              ve = field.validationError(),
              fk = field.kind(),
              itp = field.password() ? "password" : fk===WAVE.RecordModel.KIND_SCREENNAME ? "text" : fk;

          WAVE.removeChildren(divRoot);
          renderTextBox(divRoot, {
            tbid: idInput,
            about: field.about(),
            cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : ""),

            ec: published.CLS_ERROR,
            error: ve,

            id: idInput,
            tp: itp,
            name: field.name(),
            disabled: field.isEnabled() ? "" : "disabled",
            maxlength: field.size() <= 0 ? "" : "maxlength="+field.size(),
            readonly: field.readonly() ? "readonly" : "",
            value: field.isNull()? "" : field.displayValue(),
            placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
            required: field.required() ? "required" : "",
            handler: textChangeHandler,
            fw: fldView
          });
        }

        function renderTextArea(root, ctx) {
        var Ør = arguments[0];
        if (WAVE.isString(Ør))
          Ør = WAVE.id(Ør);
        var Ø1 = WAVE.ce('div');
        var Ø2 = WAVE.ce('label');
        Ø2.innerText = ctx.about;
        Ø2.setAttribute('for', ctx.tbid);
        Ø2.setAttribute('class', ctx.cls);
        Ø1.appendChild(Ø2);
        if (ctx.error!==null) {
          var Ø3 = WAVE.ce('div');
          Ø3.innerText = ctx.error;
          Ø3.setAttribute('class', ctx.ec);
          Ø1.appendChild(Ø3);
        }
        var Ø4 = WAVE.ce('textarea');
        Ø4.innerText = ctx.value;
        Ø4.setAttribute('id', ctx.id);
        Ø4.setAttribute('name', ctx.name);
        Ø4.disabled = ctx.disabled;
        Ø4.setAttribute('maxlength', ctx.maxlength);
        Ø4.readonly = ctx.readonly;
        Ø4.setAttribute('value', ctx.value);
        Ø4.setAttribute('placeholder', ctx.placeholder);
        Ø4.required = ctx.required;
        Ø4.__fieldView = ctx.fw;
        Ø4.addEventListener('change', ctx.handler, false);
        Ø1.appendChild(Ø4);
        if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
        return Ø1;
        }

        function buildTextArea(fldView){
          var field = fldView.field(),
              divRoot = fldView.DIV(),
              ids = genIDSeed(fldView),
              idInput = "tb"+ids,
              ve = field.validationError();

          WAVE.removeChildren(divRoot);

          renderTextArea(divRoot, {
            tbid: idInput,
            about: field.about(),
            cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : ""),

            ec: published.CLS_ERROR,
            error: ve,

            id: idInput,
            name: field.name(),
            disabled: field.isEnabled() ? "" : "disabled",
            maxlength: field.size() <= 0 ? "" : "maxlength="+field.size(),
            readonly: field.readonly() ? "readonly" : "",
            value: field.isNull()? "" : field.displayValue(),
            placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
            required: field.required() ? "required" : "",
            fw: fldView,
            handler: textChangeHandler
          });
        }

        function renderRadioGroup(root, ctx) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('fieldset');
          var Ø2 = WAVE.ce('legend');
          Ø2.innerText = ctx.about;
          Ø2.setAttribute('class', ctx.cls);
          Ø1.appendChild(Ø2);
          if (ctx.error) {
            var Ø3 = WAVE.ce('div');
            Ø3.innerText = ctx.error;
            Ø3.setAttribute('class', ctx.ec);
            Ø1.appendChild(Ø3);
          }
          for(var i in ctx.keys) {
             var key = ctx.keys[i], keyDesc = ctx.dict[key], idInput = 'rbt'+ctx.ids+'_'+i; 
            var Ø4 = WAVE.ce('input');
            Ø4.setAttribute('id', idInput);
            Ø4.setAttribute('name', ctx.name);
            Ø4.setAttribute('type', 'radio');
            Ø4.disabled = ctx.disabled;
            Ø4.readonly = ctx.readonly;
            Ø4.setAttribute('value', key);
            Ø4.required = ctx.required;
            Ø4.checked = WAVE.strSame(key, ctx.value) ? 'checked' : '';
            Ø4.__fieldView = ctx.fw;
            Ø4.addEventListener('change', ctx.onchange, false);
            Ø1.appendChild(Ø4);
            var Ø5 = WAVE.ce('label');
            Ø5.innerText = keyDesc;
            Ø5.setAttribute('for', idInput);
            Ø1.appendChild(Ø5);
          }
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function radioGroupChangeHandler(e) {
          var target = e.target,
              val = target.value,
              fld = target.__fieldView.field();

          if (!fld.readonly()) fld.value(val, true);//from GUI
          else rebuildControl(target.__fieldView);
        }

        function buildRadioGroup(fldView){
          var field = fldView.field(),
              divRoot = fldView.DIV(),
              dict = field.lookupDict(),
              keys = Object.keys(dict),
              ve = field.validationError(),
              ids = genIDSeed(fldView);

          WAVE.removeChildren(divRoot);

          renderRadioGroup(divRoot, {
            about: field.about(),
            cls: (field.required() ? published.CLS_REQ : "") + " " + (field.isGUIModified() ? published.CLS_MOD : ""),

            ec: published.CLS_ERROR,
            error: ve,

            keys: keys,
            dict: dict,
            ids: ids,
            name: field.name(),
            disabled: field.isEnabled() ? "" : "disabled",
            readonly: field.readonly() ? "readonly" : "",
            required: field.required() ? "required" : "",
            value: field.value(),
            fw: fldView,
            onchange: radioGroupChangeHandler
          });
        }

        function renderCheck(root, ctx) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('div');
          if(ctx.error) {
            var Ø2 = WAVE.ce('div');
            Ø2.innerText = ctx.error;
            Ø2.setAttribute('class', ctx.ec);
            Ø1.appendChild(Ø2);
          }
          var Ø3 = WAVE.ce('input');
          Ø3.setAttribute('id', 'h_'+ctx.id);
          Ø3.setAttribute('type', 'hidden');
          Ø3.setAttribute('value', ctx.val);
          Ø3.setAttribute('name', ctx.name);
          Ø1.appendChild(Ø3);
          var Ø4 = WAVE.ce('label');
          Ø4.innerText = ctx.about;
          Ø4.setAttribute('for', ctx.id);
          Ø4.setAttribute('class', ctx.cls);
          Ø1.appendChild(Ø4);
          var Ø5 = WAVE.ce('input');
          Ø5.setAttribute('id', ctx.id);
          Ø5.setAttribute('type', 'checkbox');
          Ø5.disabled = ctx.disabled;
          Ø5.readonly = ctx.readonly;
          Ø5.required = ctx.required;
          Ø5.checked = ctx.checked;
          Ø5.__fieldView = ctx.fw;
          Ø5.__hiddenId = 'h_'+ctx.id;
          Ø5.addEventListener('change', ctx.handler, false);
          Ø1.appendChild(Ø5);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function checkChangedHandler(e) {
          var target = e.target,
              val = target.checked,
              fld = target.__fieldView.field();

          WAVE.id(target.__hiddenId).value = val ? "true" : "false";
          if (!fld.readonly()) fld.value(val, true);
          else rebuildControl(target.__fieldView);
        }

        function buildCheck(fldView){
          var field = fldView.field(),
              divRoot = fldView.DIV(),
              ids = genIDSeed(fldView),
              idInput = "chk"+ids,
              ve = field.validationError();

          WAVE.removeChildren(divRoot);
          renderCheck(divRoot, {
            ec: published.CLS_ERROR,
            error: ve,

            id: idInput,
            name: field.name(),
            val: WAVE.strAsBool(field.value()) ? "true" : "false",

            about: field.about(),
            cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : ""),

            disabled: field.isEnabled() ? "" : "disabled",
            readonly: field.readonly() ? "readonly" : "",
            required: field.required() ? "required" : "",
            checked: WAVE.strAsBool(field.value()) ? "checked" : "",
            fw: fldView,
            handler: checkChangedHandler
          });
        }

        function renderComboBox(root, ctx) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('div');
          var Ø2 = WAVE.ce('label');
          Ø2.innerText = ctx.about;
          Ø2.setAttribute('for', ctx.id);
          Ø2.setAttribute('class', ctx.cls);
          Ø1.appendChild(Ø2);
          if (ctx.error) {
            var Ø3 = WAVE.ce('div');
            Ø3.innerText = ctx.error;
            Ø3.setAttribute('class', ctx.ec);
            Ø1.appendChild(Ø3);
          }
          var Ø4 = WAVE.ce('div');
          Ø4.setAttribute('class', ctx.wrapCls);
          var Ø5 = WAVE.ce('select');
          Ø5.setAttribute('id', ctx.id);
          Ø5.setAttribute('name', ctx.name);
          Ø5.disabled = ctx.disabled;
          Ø5.readonly = ctx.readonly;
          Ø5.setAttribute('value', ctx.value);
          Ø5.setAttribute('placeholder', ctx.placeholder);
          Ø5.required = ctx.required;
          Ø5.__fieldView = ctx.fw;
          Ø5.addEventListener('change', ctx.handler, false);
          var Ø6 = WAVE.ce('option');
          Ø6.setAttribute('value', '');
          Ø5.appendChild(Ø6);
          for(var i in ctx.keys) {
            var val = ctx.keys[i]; 
            var Ø7 = WAVE.ce('option');
            Ø7.innerText = ctx.dict[val];
            Ø7.setAttribute('value', val);
            Ø7.selected = WAVE.strSame(val, ctx.value) ? 'selected' : '';
            Ø5.appendChild(Ø7);
          }
          Ø4.appendChild(Ø5);
          var Ø8 = WAVE.ce('div');
          Ø8.setAttribute('id', ctx.arId);
          Ø8.setAttribute('class', ctx.arCls);
          Ø4.appendChild(Ø8);
          Ø1.appendChild(Ø4);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function comboChangedHandler(e) {
          var target = e.target,
              view = target.__fieldView,
              field = target.__fieldView.field(),
              fv = field.value();

          if (field.readonly()) {
            if (target.value !== fv) target.value = fv;
            return;
          }

          var val = target.value;
          field.value(val, true);//from GUI
        }

        function buildComboBox(fldView) {
          var field = fldView.field(),
              divRoot = fldView.DIV(),
              ids = genIDSeed(fldView),
              idInput = "cbo" + ids,
              arrowId = "lbl" + idInput,
              ve = field.validationError();

          WAVE.removeChildren(divRoot);
          renderComboBox(divRoot, {
            id: idInput,

            about: field.about(),
            cls: (field.required() ? published.CLS_REQ : "") + " " + (field.isGUIModified() ? published.CLS_MOD : ""),

            ec: published.CLS_ERROR,
            error: ve,

            name: field.name(),
            value: field.value(),
            disabled: field.isEnabled() ? "" : "disabled",
            readonly: field.readonly() ? "readonly" : "",
            placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
            required: field.required() ? "required" : "",
            wrapCls: published.CLS_COMBO_WRAP,
            fw: fldView,
            handler: comboChangedHandler,

            dict: field.lookupDict(),
            keys: Object.keys(field.lookupDict()),

            arId: arrowId,
            arCls: published.CLS_COMBO_ARROW
          });
        }

        function renderHidden(root, ctx) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('input');
          Ø1.setAttribute('type', 'hidden');
          Ø1.setAttribute('name', ctx.name);
          Ø1.setAttribute('value', ctx.value);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function buildHidden(fldView) {
          var divRoot = fldView.DIV(),
              field = fldView.field();
          WAVE.removeChildren(divRoot);
          renderHidden(divRoot, {
            name: field.name(),
            value: field.displayValue()
          });
        }

        function renderCustomControlContainer(root, lblDivId, cntDivId) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('div');
          var Ø2 = WAVE.ce('div');
          Ø2.setAttribute('id', lblDivId);
          Ø1.appendChild(Ø2);
          var Ø3 = WAVE.ce('div');
          Ø3.setAttribute('id', cntDivId);
          Ø1.appendChild(Ø3);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function renderCustomControlContent(root, ctx) {
          WAVE.removeChildren(root);
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('nfx');
          var Ø2 = WAVE.ce('div');
          Ø2.innerText = ctx.error;
          Ø2.setAttribute('class', ctx.ec);
          Ø1.appendChild(Ø2);
          var Ø3 = WAVE.ce('label');
          Ø3.innerText = ctx.about;
          Ø3.setAttribute('class', ctx.cls);
          Ø1.appendChild(Ø3);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function buildChainSelector(fldView) {
          var field = fldView.field(),
              divRoot = fldView.DIV(),
              genIdKey = "@#$CHAIN_SELECTOR_GEN_ID$#@",
              ids = WAVE.get(fldView, genIdKey, null);

          if (ids === null) {
            ids = genIDSeed(fldView);
            fldView[genIdKey] = ids;
          }

          var idCSDiv = "divCS_" + ids,
              idLabelDiv = "labelCont_" + ids,
              labelCont = WAVE.id(idLabelDiv),
              editorCont = WAVE.id(idCSDiv),
              ve = field.validationError();

          if (labelCont === null || editorCont === null) {
            renderCustomControlContainer(divRoot, idLabelDiv, idCSDiv);
            labelCont = WAVE.id(idLabelDiv);
            editorCont = WAVE.id(idCSDiv);
          }
          renderCustomControlContent(labelCont, {
            ec: published.CLS_ERROR,
            error: ve,

            about: field.about(),
            cls: (field.required() ? published.CLS_REQ : "") + " " + (field.isGUIModified() ? published.CLS_MOD : "")
          });

          var dict = field.lookupDict(),
              values = WAVE.exists(dict) ? dict.values : [],
              path = field.isNull() ? "" : field.value(),
              selector = new WAVE.GUI.ChainSelector({
                DIV: editorCont,
                outputFormFieldName: field.name(),
                classes: { divArrowCls: published.CLS_COMBO_ARROW, divComboWrapCls: published.CLS_COMBO_WRAP },
                path: path,
                disable: field.isEnabled() ? false : true,
                readonly: field.readonly(),
                values: values
              });

          selector.eventBind(WAVE.GUI.EVT_CHAIN_SELECTOR_UPDATED,
                             function (e, d) {
                               field.value(d, true);//from GUI
                             });
        }

        function buildPSEditor(fldView){
          var field = fldView.field(),
             divRoot = fldView.DIV(),
             genIdKey = "@#$PS_EDITOR_GEN_ID$#@",
             ids = WAVE.get(fldView, genIdKey, null);

          if (ids === null) {
            ids = genIDSeed(fldView);
            fldView[genIdKey] = ids;
          }

          var idPSDiv = "divps_"+ids,
              idLabelDiv =  "labelCont_"+ids,
              labelCont = WAVE.id(idLabelDiv),
              editorCont = WAVE.id(idPSDiv),
              ve = field.validationError();

          if (labelCont === null || editorCont === null){
            renderCustomControlContainer(divRoot, idLabelDiv, idPSDiv);
            labelCont = WAVE.id(idLabelDiv);
            editorCont = WAVE.id(idPSDiv);
          }
          renderCustomControlContent(labelCont, {
            ec: published.CLS_ERROR,
            error: ve,

            about: field.about(),
            cls: (field.required() ? published.CLS_REQ : "") + " " + (field.isGUIModified() ? published.CLS_MOD : "")
          });

          var json = field.isNull()? "" : field.value(),
              ekey = "@#$PS_EDITOR$#@",
              editor = WAVE.get(fldView, ekey, null);
          if (editor===null)
          {
            editor =  new WAVE.GUI.PropSetEditor({
              DIV: editorCont,
              outputFormFieldName: field.name(),
              langs: WAVE.LOCALIZER.allLanguageISOs(),
              content: json,
              disable: field.isEnabled() ? false : true,
              readonly: field.readonly(),
              title: field.about()
            });

            fldView[ekey] = editor;

            editor.eventBind(WAVE.GUI.EVT_PS_EDITOR_UPDATED,
            function (e, d){
              field.value(d, true);//from GUI
            });
          }
        }

        function renderPuzzleContainer(root, ctx) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('div');
          var Ø2 = WAVE.ce('div');
          Ø2.innerText = ctx.error;
          Ø2.setAttribute('class', ctx.ec);
          Ø1.appendChild(Ø2);
          var Ø3 = WAVE.ce('div');
          Ø3.setAttribute('id', ctx.id);
          Ø3.setAttribute('class', ctx.cls);
          Ø1.appendChild(Ø3);
          var Ø4 = WAVE.ce('input');
          Ø4.setAttribute('id', ctx.hId);
          Ø4.setAttribute('type', 'hidden');
          Ø4.setAttribute('name', ctx.name);
          Ø4.setAttribute('value', '');
          Ø1.appendChild(Ø4);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function buildPuzzle(fldView){
          if (fldView.PUZZLE) return;

          var field = fldView.field(),
              divRoot = fldView.DIV(),
              ids = genIDSeed(fldView),
              idInput = "divPuzzle"+ids,
              idHiddenInput = "hiddenPuzzle"+ids,
              ve = field.validationError();

          renderPuzzleContainer(divRoot, {
            ec: published.CLS_ERROR,
            error: ve,

            id: idInput,
            cls: published.CLS_PUZZLE,
            hId: idHiddenInput,
            name: field.name()
          });

          var fv = field.value();
          var pk = new WAVE.GUI.PuzzleKeypad(
            {
              DIV: WAVE.id(idInput),
              Image: WAVE.strDefault(fv.Image, ""),
              Help:  WAVE.strDefault(fv.Help, ""),
              Question: WAVE.strDefault(fv.Question, "")
            });

          var hidden = WAVE.id(idHiddenInput);
          hidden.value = JSON.stringify(field.value());

          pk.eventBind(WAVE.GUI.EVT_PUZZLE_KEYPAD_CHANGE,
            function (kpad) {
              field.value().Answer = kpad.value();
              hidden.value = JSON.stringify(field.value());
            });

          fldView.PUZZLE = pk;
        }

        function renderErrorRec(root, ctx) {
          var Ør = arguments[0];
          if (WAVE.isString(Ør))
            Ør = WAVE.id(Ør);
          var Ø1 = WAVE.ce('div');
          Ø1.innerText = ctx.error;
          Ø1.setAttribute('class', ctx.ec);
          if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
          return Ø1;
        }

        function buildErrorRec(fldView, summary){
          var record = fldView.record(),
              divRoot = fldView.DIV();

          if (summary)
          {
            var allErrors = record.allValidationErrors();
            for(var i in allErrors)
            {
              var err = allErrors[i];
              if (err !== null) renderErrorRec(divRoot, { ec: published.CLS_ERROR, error: err });
            }
          }
          else
          {
            var rve = record.validationError();
            if (rve !== null) renderErrorRec(divRoot, { ec: published.CLS_ERROR, error: rve });
          }
        }

        function rebuildControl(fldView){
          var ct = published.getControlType(fldView);

          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_CHECK))    { ensureField(fldView, ct); buildCheck(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_CHAIN_SELECTOR)) { ensureField(fldView, ct); buildChainSelector(fldView); }
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_CHECK))    { ensureField(fldView, ct); buildCheck(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_RADIO))    { ensureField(fldView, ct); buildRadioGroup(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_NLS))      { ensureField(fldView, ct); buildPSEditor(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_COMBO))    { ensureField(fldView, ct); buildComboBox(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_PUZZLE))   { ensureField(fldView, ct); buildPuzzle(fldView);}
          else
          if (
               WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_TEXTAREA) ||
               WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_SCRIPT)
             )                                                    { ensureField(fldView, ct); buildTextArea(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_HIDDEN))   { ensureField(fldView, ct); buildHidden(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_ERROR_REC)) buildErrorRec(fldView, false);
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_ERROR_SUMMARY)) buildErrorRec(fldView, true);
          else
          {
            ensureField(fldView, ct);
            buildTextBox(fldView);
          }
        }

        function ensureField(fldView, ct) {
          if (fldView.field()===null)
           throw "The control type '"+ct+"' requires the field binding, but was bound to record";
        }


    //Gets the appropriate(for this GUI lib) control type if the one is specified in field schema, or infers one from field definition
    published.getControlType = function(fldView){
      return fldView.getOrInferControlType();
    };

    //Builds control from scratch
    published.buildFieldViewAnew = function(fldView){
     rebuildControl(fldView);
    };

    //Updates control in response to underlying bound field change
    published.eventNotifyFieldView = function(fldView, evtName, sender, phase){
      rebuildControl(fldView);
    };

    return published;
}());//WAVE.RM.GUI