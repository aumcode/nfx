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
              div=@about@
              input{
                type=text
                name=@fName@
                value=@fValue@
              }
            }
          ***/
        }

        function buildTestControl(fldView){
          var ctx = {
            about: fldView.field().about(),
            fName: fldView.field().name(),
            fValue: fldView.field().value()
          };
          renderTestControl(fldView.DIV(), ctx);
        }

        function genIDSeed(fldView){
          var id = WAVE.genAutoincKey(AIKEY);
          return "_"+fldView.recView().ID()+"_"+id;
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

        function buildTextBox(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "tb"+ids;

          var html = WAVE.strHTMLTemplate("<label for='@tbid@' class='@cls@'>@about@</label>",
                                      {
                                        tbid: idInput,
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          var itp;
          var fk = field.kind();
          if (field.password()) itp = "password";
          else if (fk===WAVE.RecordModel.KIND_SCREENNAME) itp = "text";
          else itp = fk;

          html+= WAVE.strHTMLTemplate("<input id='@id@' type='@tp@' name='@name@' @disabled@ @maxlength@ @readonly@ value='@value@' placeholder='@placeholder@' @required@>",
                                      {
                                        id: idInput,
                                        tp: itp,
                                        name: field.name(),
                                        disabled: field.isEnabled() ? "" : "disabled",
                                        maxlength: field.size() <= 0 ? "" : "maxlength="+field.size(),
                                        readonly: field.readonly() ? "readonly" : "",
                                        value: field.isNull()? "" : field.displayValue(),
                                        placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
                                        required: field.required() ? "required" : ""
                                      });

          divRoot.innerHTML = html;
          WAVE.id(idInput).__fieldView = fldView;
          $("#"+idInput).change(function(){
            var val = this.value;
            try
            {
                this.__fieldView.field().setGUIValue(val);
                fErroredInput = null;
            }
            catch(e)
            {
                WAVE.GUI.toast("Wrong value for field '"+this.__fieldView.field().about()+"'. Please re-enter or undo", "error");
                var self = this;
                if (fErroredInput===self || fErroredInput===null)
                {
                    fErroredInput = self;
                    setTimeout(function()
                           {
                             if (!self.ERRORED) $(self).blur(function(){$(self).change();});
                             self.focus();
                             self.ERRORED = true;
                           }, 50);
                }
            }
          });
        }


        function buildTextArea(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "tb"+ids;

          var html = WAVE.strHTMLTemplate("<label for='@tbid@' class='@cls@'>@about@</label>",
                                      {
                                        tbid: idInput,
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          html+= WAVE.strHTMLTemplate("<textarea id='@id@' name='@name@' @disabled@ @maxlength@ @readonly@ placeholder='@placeholder@' @required@>@value@</textarea>",
                                      {
                                        id: idInput,
                                        name: field.name(),
                                        disabled: field.isEnabled() ? "" : "disabled",
                                        maxlength: field.size() <= 0 ? "" : "maxlength="+field.size(),
                                        readonly: field.readonly() ? "readonly" : "",
                                        value: field.isNull()? "" : field.displayValue(),
                                        placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
                                        required: field.required() ? "required" : ""
                                      });

          divRoot.innerHTML = html;
          WAVE.id(idInput).__fieldView = fldView;
          $("#"+idInput).change(function(){
            var val = this.value;
            try
            {
                this.__fieldView.field().setGUIValue(val);
                fErroredInput = null;
            }
            catch(e)
            {
                WAVE.GUI.toast("Wrong value for field '"+this.__fieldView.field().about()+"'. Please re-enter or undo", "error");
                var self = this;
                if (fErroredInput===self || fErroredInput===null)
                {
                    fErroredInput = self;
                    setTimeout(function()
                           {
                             if (!self.ERRORED) $(self).blur(function(){$(self).change();});
                             self.focus();
                             self.ERRORED = true;
                           }, 50);
                }
            }
          });
        }


        function buildRadioGroup(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var dict = field.lookupDict();
          var keys = Object.keys(dict);

          var html = "<fieldset>";
          html+= WAVE.strHTMLTemplate("<legend class='@cls@'>@about@</legend>",{about: field.about(),
                                                                                cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                                                               });

          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          var ids = genIDSeed(fldView);

          for(var i in Object.keys(dict)){
             var key = keys[i];
             var keyDescr = dict[key];

              var idInput = "rbt"+ids+"_"+i;

              html+= WAVE.strHTMLTemplate("<input id='@id@' type='radio' name='@name@' @disabled@ @readonly@ value='@value@' @required@ @checked@>",
                                          {
                                            id: idInput,
                                            name: field.name(),
                                            disabled: field.isEnabled() ? "" : "disabled",
                                            readonly: field.readonly() ? "readonly" : "",
                                            value: key,
                                            required: field.required() ? "required" : "",
                                            checked: WAVE.strSame(key, field.value()) ? "checked" : ""
                                          });

              html+= WAVE.strHTMLTemplate("<label for='@rbtid@'>@about@</label>",
                                          {
                                            rbtid: idInput,
                                            about: keyDescr
                                          });



          }//for

          html+= "</fieldset>";

          divRoot.innerHTML = html;

          //bind events
          function rbtChange(evt){
            var val = evt.target.value;
            var fld = evt.target.__fieldView.field();
            if (!fld.readonly())
                fld.value(val, true);//from GUI
            else
                rebuildControl(evt.target.__fieldView);
          }

          for(var j in Object.keys(dict)){
           var idInp = "rbt"+ids+"_"+j;
           WAVE.id(idInp).__fieldView = fldView;

           $("#"+idInp).change(rbtChange);
          }
        }

        function buildCheck(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "chk"+ids;

          var html = "";
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});


          html+= WAVE.strHTMLTemplate("<input id='h_@id@' type='hidden'  value='@val@' name='@name@'>",
                                      {
                                        id: idInput,
                                        name: field.name(),
                                        val: WAVE.strAsBool(field.value()) ? "true" : "false"
                                      });


          html+= WAVE.strHTMLTemplate("<label for='@chkid@' class='@cls@'>@about@</label>",
                                      {
                                        chkid: idInput,
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });
          html+= WAVE.strHTMLTemplate("<input id='@id@' type='checkbox' @disabled@ @readonly@  @required@ @checked@>",
                                      {
                                        id: idInput,
                                        disabled: field.isEnabled() ? "" : "disabled",
                                        readonly: field.readonly() ? "readonly" : "",
                                        required: field.required() ? "required" : "",
                                        checked: WAVE.strAsBool(field.value()) ? "checked" : ""
                                      });

          divRoot.innerHTML = html;
          WAVE.id(idInput).__fieldView = fldView;
          $("#"+idInput).change(function(){
               var val = this.checked;
               $("#h_"+idInput).val( val ? "true" : "false");
               var fld = this.__fieldView.field();
               if (!fld.readonly())
                 fld.value(val, true);//from GUI
               else
                 rebuildControl(this.__fieldView);
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

        function buildComboBox(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "cbo"+ids;
          var arrowId = "lbl"+idInput;

          var html = WAVE.strHTMLTemplate("<label for='@cbid@' class='@cls@'>@about@</label>",
                                      {
                                        cbid: idInput,
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });

          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          html+= WAVE.strHTMLTemplate("<div class='@comboWrapCls@'><select id='@id@' name='@name@' @disabled@ @readonly@ value='@value@' placeholder='@placeholder@' @required@>",
                                      {
                                        id: idInput,
                                        name: field.name(),
                                        disabled: field.isEnabled() ? "" : "disabled",
                                        readonly: field.readonly() ? "readonly" : "",
                                        value: field.value(),
                                        placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
                                        required: field.required() ? "required" : "",
                                        comboWrapCls: published.CLS_COMBO_WRAP
                                      });

          var dict = field.lookupDict();
          var keys = Object.keys(dict);
          html += "<option value=''></option>";//add Blank
          for(var i in Object.keys(dict)){
              html += WAVE.strHTMLTemplate("<option value='@value@' @selected@>@descr@</option>",
                                          {
                                            value: keys[i],
                                            selected: WAVE.strSame(keys[i], field.value()) ? "selected" : "",
                                            descr: dict[keys[i]]
                                          });
          }//for options

          html+=WAVE.strHTMLTemplate("</select><div id='@id@' class='@comboArrowCls@'></div></div>",
                                          {
                                            id: arrowId,
                                            comboArrowCls: published.CLS_COMBO_ARROW
                                          });
          divRoot.innerHTML = html;
          WAVE.id(idInput).__fieldView = fldView;
          $("#"+idInput).change(function(){

            var fv = field.value();
            if (field.readonly())
            {
              if (this.value !== fv) this.value = fv;
              return;
            }

            var val = this.value;
            this.__fieldView.field().value(val, true);//from GUI
          });

          WAVE.id(arrowId).onclick = function() {
            var element = WAVE.id(idInput);
            try {
              var event = new MouseEvent('mousedown');
              element.dispatchEvent(event);
            } catch(e) { }
            return false;
          };
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

          pk.eventBind(WAVE.GUI.EVT_PUZZLE_KEYPAD_CHANGE, function(kpad){
                          field.value().Answer = kpad.value();
                          hidden.value = JSON.stringify(field.value());
                       });

          fldView.PUZZLE = pk;
        }


        function buildHidden(fldView){
          fldView.DIV().innerHTML = WAVE.strHTMLTemplate("<input type='hidden' name='@fName@' value='@fValue@'>",
                                                        {fName: fldView.field().name(), fValue: fldView.field().displayValue()});
        }


        function buildErrorRec(fldView, summary){
          var record = fldView.record();
          var divRoot = fldView.DIV();


          var html = "";
          if (summary)
          {
            var allErrors = record.allValidationErrors();
            for(var i in allErrors)
            {
               var err = allErrors[i];
               if (err!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: err});
            }

          }
          else
          {
            var rve = record.validationError();
            if (rve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: rve});
          }

          divRoot.innerHTML = html;
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