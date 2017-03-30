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
          /***
            div{
              div=?ctx.about
              input{
                type=text
                name=?ctx.fName
                value=?ctx.fValue
              }
            }
          ***/
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
          /***
            div{
              label=?ctx.about {
                for=?ctx.tbid
                class=?ctx.cls
              }
              "?if(ctx.error)"{
                div=?ctx.error{
                  class=?ctx.ec
                }
              }
              input{
                id=?ctx.id
                type=?ctx.tp
                name=?ctx.name
                disabled=?ctx.disabled
                readonly=?ctx.readonly
                value=?ctx.value
                placeholder=?ctx.placeholder
                required=?ctx.required
                __fieldView=?ctx.fw
                on-change=ctx.handler
              }
            }
          ***/
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
        /***
          div{
            label=?ctx.about {
              for=?ctx.tbid
              class=?ctx.cls
            }
            "?if (ctx.error!==null)" {
              div=?ctx.error {
                class=?ctx.ec
              }
            }
            textarea=?ctx.value {
              id=?ctx.id
              name=?ctx.name
              disabled=?ctx.disabled
              maxlength=?ctx.maxlength
              readonly=?ctx.readonly
              value=?ctx.value
              placeholder=?ctx.placeholder
              required=?ctx.required
              __fieldView=?ctx.fw
              on-change=ctx.handler
            }
          }
        ***/
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
          /***
            fieldset {
              legend=?ctx.about {
                class=?ctx.cls
              }
              "?if (ctx.error)" {
                div=?ctx.error {
                  class=?ctx.ec
                }
              }
              "?for(var i in ctx.keys)" {
                "? var key = ctx.keys[i], keyDesc = ctx.dict[key], idInput = 'rbt'+ctx.ids+'_'+i;" {}
                input {
                  id=?idInput
                  name=?ctx.name
                  type=radio
                  disabled=?ctx.disabled
                  readonly=?ctx.readonly
                  value=?key
                  required=?ctx.required
                  checked="?WAVE.strSame(key, ctx.value) ? 'checked' : ''"
                  __fieldView=?ctx.fw
                  on-change=ctx.onchange
                }
                label=?keyDesc {
                  for=?idInput
                }
              }
            }
          ***/
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
          /***
            div {
              "?if(ctx.error)" {
                div=?ctx.error {
                  class=?ctx.ec
                }
              }
              input {
                id="?'h_'+ctx.id"
                type=hidden
                value=?ctx.val
                name=?ctx.name
              }
              label=?ctx.about {
                for=?ctx.id
                class=?ctx.cls
              }
              input {
                id=?ctx.id
                type=checkbox
                disabled=?ctx.disabled
                readonly=?ctx.readonly
                required=?ctx.required
                checked=?ctx.checked
                __fieldView=?ctx.fw
                __hiddenId="?'h_'+ctx.id"
                on-change=ctx.handler
              }
            }
          ***/
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
          /***
          div {
            label=?ctx.about {
              for=?ctx.id
              class=?ctx.cls
            }
            "?if (ctx.error)" {
              div=?ctx.error {
                class=?ctx.ec
              }
            }
            div {
              class=?ctx.wrapCls
              select {
                id=?ctx.id
                name=?ctx.name
                disabled=?ctx.disabled
                readonly=?ctx.readonly
                value=?ctx.value
                placeholder=?ctx.placeholder
                required=?ctx.required
                __fieldView=?ctx.fw
                on-change=ctx.handler
                option {
                  value=''
                }
                "?for(var i in ctx.keys)" {
                  "?var val = ctx.keys[i];" {}
                  option=?ctx.dict[val] {
                    value=?val
                    selected="?WAVE.strSame(val, ctx.value) ? 'selected' : ''"
                  }
                }
              }
              div {
                id=?ctx.arId
                class=?ctx.arCls
              }
            }
          }
          ***/
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
            value: field.value(),
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
          /***
            input {
              type=hidden
              name=?ctx.name
              value=?ctx.value
            }
          ***/
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

        function buildChainSelector(fldView) {
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var genIdKey = "@#$CHAIN_SELECTOR_GEN_ID$#@";
          var ids = WAVE.get(fldView, genIdKey, null);
          if (ids === null) {
            ids = genIDSeed(fldView);
            fldView[genIdKey] = ids;
          }
          var idCSDiv = "divCS_" + ids;
          var idLabelDiv = "labelCont_" + ids;

          var labelCont = WAVE.id(idLabelDiv);
          var editorCont = WAVE.id(idCSDiv);
          if (labelCont === null || editorCont === null) {
            divRoot.innerHTML = WAVE.strHTMLTemplate("<div id='@idLabelDiv@'></div><div id='@idCSDiv@'></div>",
                                                     {
                                                       idLabelDiv: idLabelDiv,
                                                       idCSDiv: idCSDiv
                                                     });
            labelCont = WAVE.id(idLabelDiv);
            editorCont = WAVE.id(idCSDiv);
          }

          var html = "";
          var ve = field.validationError();
          if (ve !== null) html += WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", { ec: published.CLS_ERROR, error: ve });
          html += WAVE.strHTMLTemplate("<label class='@cls@'>@about@</label>",
                                      {
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") + " " + (field.isGUIModified() ? published.CLS_MOD : "")
                                      });

          labelCont.innerHTML = html;

          var dict = field.lookupDict();
          var values = WAVE.exists(dict) ? dict.values : [];
          var path = field.isNull() ? "" : field.value();

          var selector = new WAVE.GUI.ChainSelector({
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
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var genIdKey = "@#$PS_EDITOR_GEN_ID$#@";
          var ids = WAVE.get(fldView, genIdKey, null);
          if (ids === null) {
            ids = genIDSeed(fldView);
            fldView[genIdKey] = ids;
          }
          var idPSDiv = "divps_"+ids;
          var idLabelDiv =  "labelCont_"+ids;

          var labelCont = WAVE.id(idLabelDiv);
          var editorCont = WAVE.id(idPSDiv);
          if (labelCont === null || editorCont === null){
             divRoot.innerHTML = WAVE.strHTMLTemplate("<div id='@idLabelDiv@'></div><div id='@idPSDiv@'></div>", {idLabelDiv:idLabelDiv, idPSDiv:idPSDiv});
             labelCont = WAVE.id(idLabelDiv);
             editorCont = WAVE.id(idPSDiv);
          }

          var html = "";
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});
          html+= WAVE.strHTMLTemplate("<label class='@cls@'>@about@</label>",
                                      {
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });

          labelCont.innerHTML = html;

          var json = field.isNull()? "" : field.value();
          var ekey = "@#$PS_EDITOR$#@";
          var editor = WAVE.get(fldView, ekey, null);
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

        function buildPuzzle(fldView){

          if (fldView.PUZZLE) return;

          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "divPuzzle"+ids;
          var idHiddenInput = "hiddenPuzzle"+ids;

          var html = "";
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          html+= WAVE.strTemplate("<div id='@id@' class='@cls@'></div>"+
                                  "<input id='@idh@' type='hidden' name=@fname@ value=''></input>",
                                      {
                                        id: idInput,
                                        idh: idHiddenInput,
                                        fname: field.name(),
                                        cls: published.CLS_PUZZLE
                                      });

          divRoot.innerHTML = html;

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

          pk.eventBind(WAVE.GUI.EVT_PUZZLE_KEYPAD_CHANGE, function(kpad) {
                          field.value().Answer = kpad.value();
                          hidden.value = JSON.stringify(field.value());
                       });

          fldView.PUZZLE = pk;
        }

        function renderErrorRec(root, ctx) {
          /***
            div=?ctx.error {
              class=?ctx.ec
            }
          ***/
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