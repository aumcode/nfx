    //RecordView class
    // id - required, unique in page id of the view
    // rec - required, data record instance
    // gui - GUI library, if null then default script "wv.gui.js" must be included
    // manualViews - if true then view controls will not be auto-constructed
    published.RecordView = function(id, rec, gui, manualViews)
    {
        if (WAVE.strEmpty(id)) throw "RecordView.ctor(id: must specify)";
        if (!WAVE.isObject(rec)) throw "RecordView.ctor(rec: must be Record)";
        if (!WAVE.isObject(gui)) gui = WAVE.RecordModel.GUI;//dflt GUI lib
        var fID = id;
        var fRecord = rec;
        var fGUI = gui;
        var recview = this;
        var fViews = [];
        var fRootElement = null;


        //Returns record view instance ID used for FieldView bindings
        this.ID = function(){ return fID;};

        //Returns bound record
        this.record = function(){ return fRecord;};

        //Returns GUI backend that renders controls for the view
        this.gui = function(){ return fGUI;};

        //Returns root element that this record view is building controls under
        this.rootElement = function() { return fRootElement;};


        //Unbinds and deletes all views
        this.destroyViews = function(){
          while(fViews.length>0) fViews[0].destroy();
        };

        var divHiddenFields = null;

        //Builds and binds view controls to record fields from declarative page markup: <...dava-wv-rid='r1'...> <div data-wv-fld='LastName' ... />
        this.buildViews = function(){
          fRootElement = null;
          this.destroyViews();

          var allForms = document.getElementsByTagName("form");
          for (var i=0, imax=allForms.length; i < imax; i++) {
            var frm = allForms[i];

            var irid = frm.getAttribute(published.DATA_RECVIEW_ID_ATTR);
            if (WAVE.strSame(fID, irid))
            {
                fRootElement = frm;
                break;
            }
          }

          if (fRootElement===null){
              var allDIVS = document.getElementsByTagName("div");
              for (var j=0, jmax=allDIVS.length; j < jmax; j++) {
                var div = allDIVS[j];
                var jrid = div.getAttribute(published.DATA_RECVIEW_ID_ATTR);
                if (WAVE.strSame(fID, jrid))
                {
                    fRootElement = div;
                    break;
                }
              }
          }

          if (fRootElement===null) throw "No 'form' or 'div' element could be found with record binding '"+published.DATA_RECVIEW_ID_ATTR+"' annotation";

          //find DIVs for views, need copy as DOM indexes will shift as views get added
          var allViewDIVS = WAVE.arrayShallowCopy( fRootElement.getElementsByTagName("div") );

          for (var k=0, kmax=allViewDIVS.length; k < kmax; k++) {
                var kdiv = allViewDIVS[k];
                var fname = kdiv.getAttribute(published.DATA_FIELD_NAME_ATTR);
                if (!WAVE.strEmpty(fname))
                {
                    if (fname==="#")//whole-record-level binding
                    {
                      new this.FieldView(kdiv, null);//bind to whole record
                    }
                    else
                    {
                      var fld = fRecord.fieldByName(fname);
                      if (fld!==null) new this.FieldView(kdiv, fld);
                    }
                }
          }


          if (!WAVE.strEmpty(fRecord.formMode()) || !WAVE.strEmpty(fRecord.csrfToken()) || !WAVE.strEmpty(fRecord.roundtrip()))
          {
             if (divHiddenFields===null)
             {
                divHiddenFields = document.createElement("div");
                divHiddenFields.style.display = "none";
                fRootElement.appendChild(divHiddenFields);
             }

             var content = "";

             if (!WAVE.strEmpty(fRecord.formMode()))
                content += "<input type='hidden' name='__FormMode' value='"+fRecord.formMode()+"'>";

             if (!WAVE.strEmpty(fRecord.csrfToken()))
                content +=  "<input type='hidden' name='__CSRFToken' value='"+fRecord.csrfToken()+"'>";

             if (!WAVE.strEmpty(fRecord.roundtrip()))
                content +=  "<input type='hidden' name='__Roundtrip' value='"+fRecord.roundtrip()+"'>";

             divHiddenFields.innerHTML = content;
          }

        };

        //Returns copy of view list
        this.views = function() {
           return WAVE.arrayShallowCopy(fViews);
        };


        //Individual field view class, if fld==null binds to whole record context
        this.FieldView = function(div, fld)
        {
            if (!div) throw "FieldView.ctor(div: element must be passed)";
            if (fld!==null && !WAVE.isObject(fld)) throw "FieldView.ctor(fld: must be Record.Field)";
            var fField = fld;
            var fieldview = this;
            var fDIV = div;
            fViews.push( fieldview );//register with parent

            if (fField!==null)
              fField.eventSinkBind(this);
            else
              fRecord.eventSinkBind(this);


            var recPropertyName = "";

            if (fld!==null)
            {
              recPropertyName = "fld"+fld.name();
              recview[recPropertyName] = fieldview;//make recview.fldLastName shortcut
            }

            //Invoked by control changes
            this.eventNotify = function(evtName, sender, phase){
              if (!WAVE.strOneOf(evtName,[WAVE.RecordModel.EVT_DATA_CHANGE,
                                          WAVE.RecordModel.EVT_INTERACTION_CHANGE,
                                          WAVE.RecordModel.EVT_VALIDATION_DEFINITION_CHANGE,
                                          WAVE.RecordModel.EVT_VALIDATED])) return;

              if (typeof(phase)===tUNDEFINED) phase = "";
              if (phase === WAVE.RecordModel.EVT_PHASE_BEFORE) return;

              if (fField!==null)
                fDIV.style.visibility = fField.visible() ? "visible" : "hidden";

              fGUI.eventNotifyFieldView( fieldview, evtName, sender, phase);
            };

            //Unbinds the view and deletes all internal markup
            this.destroy = function(){
                fDIV.innerHTML = "";//destroy control content
                if (fField!==null)
                  fField.eventSinkUnbind(this);
                else
                  fRecord.eventSinkUnbind(this);

                WAVE.arrayDelete(fViews, this);

                if (fField!==null)
                  delete recview[recPropertyName];
            };

            this.record = function(){ return fRecord; };
            this.recView = function() { return recview;};
            this.field = function(){ return fField; };


            //Returns root element in which the "visual control" gets built
            this.DIV = function(){ return fDIV; };

            //Gets control type specified on this view or infersfrom field
            this.getOrInferControlType = function(){
               var ctp = fDIV.getAttribute(published.DATA_CTL_TP_ATTR);
               if (!WAVE.strEmpty(ctp)) return ctp;
               return fField===null ? WAVE.RecordModel.CTL_TP_ERROR_REC : fField.getOrInferControlType();
            };

            if (fField===null || fField.visible())
                fGUI.buildFieldViewAnew( this );
        };//FieldView


        if (!manualViews) this.buildViews();
    };//RecordView