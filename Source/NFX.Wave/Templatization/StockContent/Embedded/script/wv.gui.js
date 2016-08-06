"use strict";
/*jshint devel: true,browser: true, sub: true */ 
/*global WAVE: true,$: true */

/*!
 * Wave Java Script Library Default UI v2.0.0 
 *
 * Based on IT Adapter JS Library 2002-2011 
 * License: Unrestricted use/modification with mandatory reference to IT Adapter Corp. as the original author  
 * (c) 2002-2011, 2013-2014 IT Adapter Corp.
 * http://www.itadapter.com/
 * Authors: Dmitriy Khmaladze
 * Revision Epoch:  May 9, 2014
 */

WAVE.GUI = (function(){

    var tUNDEFINED = "undefined";

    var published = { 
        TUNDEFINED: tUNDEFINED,
        CLS_TOAST: "wvToast",
        CLS_CURTAIN: "wvCurtain",
        CLS_DIALOG_BASE: "wvDialogBase",
        CLS_DIALOG_TITLE: "wvDialogTitle",
        CLS_DIALOG_CONTENT: "wvDialogContent",
        CLS_DIALOG_CONFIRM_FOOTER: "wvConfirmDialogFooter",
        CLS_DIALOG_CONFIRM_BUTTON: "wvConfirmDialogButton",

        DLG_UNDEFINED: '',
        DLG_CANCEL: 'cancel',
        DLG_OK: 'ok',
        DLG_YES: 'yes',
        DLG_NO: 'no',

        EVT_DIALOG_CLOSE: 'dlg-close',
        EVT_PUZZLE_KEYPAD_CHANGE: 'puzzle-change',

        // Tree {
        CLS_TREE_NODE: "wvTreeNode",
        CLS_TREE_NODE_OWN_SELECTED: "wvTreeNodeOwnSelected",
        CLS_TREE_NODE_BUTTON_EXPANDED: "wvTreeNodeButtonExpanded",
        CLS_TREE_NODE_BUTTON_COLLAPSED: "wvTreeNodeButtonCollapsed",
        CLS_TREE_NODE_CONTENT: "wvTreeNodeContent",
        CLS_TREE_NODE_OWN: "wvTreeNodeOwn",
        CLS_TREE_NODE_CHILDREN: "wvTreeNodeChildren",

        EVT_PHASE_BEFORE: "before",
        EVT_PHASE_AFTER:  "after",

        
        EVT_TREE_NODE_ADD: 'tree-node-add',
        EVT_TREE_NODE_REMOVE: 'tree-node-remove',

        EVT_TREE_NODE_CONTENT: 'tree-node-content',
        EVT_TREE_NODE_DATA: 'tree-node-data',

        EVT_TREE_NODE_EXPANSION: 'tree-node-expansion',
        EVT_TREE_NODE_SELECTION: 'tree-node-selection',

        EVT_RULER_MOUSE_MOVE: 'ruler-mouse-move',
        EVT_RULER_MOUSE_ENTER: 'ruler-mouse-enter',
        EVT_RULER_MOUSE_LEAVE: 'ruler-mouse-leave',

        TREE_SELECTION_NONE: 0,
        TREE_SELECTION_SINGLE: 1,
        TREE_SELECTION_MULTI: 2,
        // } Tree

        //ObjectInspector {
        CLS_OBJECTINSPECTOR_EDITOR: 'wvObjectInspectorEditor',
        // { ObjectInspector

        //PropSetEditor
        EVT_PS_EDITOR_UPDATED: 'editor-updated',

        //classes
        CLS_PS_DIV_LANGS_CONTAINER: 'wvPSLangsContainer',
        CLS_PS_BTN_ADD: 'wvPSBtnAdd',
        CLS_PS_BTN_DEL: 'wvPSBtnDel',
        CLS_PS_BTN_RAW: 'wvPSBtnRaw',
        CLS_PS_BTN_CLEAR: 'wvPSBtnClear',
        CLS_PS_DIV_LANG_NAME: 'wvPSLangLabel',
        CLS_PS_LANG_SELECTOR: 'wvPSLangSelector',
        CLS_PS_TEXTAREA_RAW_EDITOR: 'wvPSTextareaRawEditor',
        CLS_PS_LABEL_CONTAINER: 'wvPSLabelContainer',
        CLS_PS_INPUT_CONTAINER: 'wvPSInputContainer',


        //Tabs control
        CLS_TABS_ACTIVE_TAB: "wvTabsActive",
        CLS_TABS_UL: "wvTabsUl",
        CLS_TABS_CONTENT_DIV: "wvTabsContentDiv",
        CLS_TABS_CONTENT_DIV_HIDDEN: "wvTabsContentDivHidden",
        CLS_TABS_CONTENT_DIV_SHOWN: "wvTabsContentDivShown",
        CLS_TABS_CONTENT_CONTAINER: "wvTabsContentContainer",
        CLS_TABS_UL_CONTAINER: "wvTabsUlContainer",
        CLS_TABS_LI_DISABLED: "wvTabDisabled",

        EVT_TABS_TAB_CHANGED: "wv-active-tab-changed",
        EVT_TABS_TAB_ADD: "wv-tab-add",
        EVT_TABS_TAB_REMOVE: "wv-tab-remove",

        //Multiline
        EVT_MULTI_LINE_TEXT_UPDATED: "wv-multiline-updated",

        //ObjectEditor
        CLS_OBJECT_EDITOR_NODE_CONTAIER: "wvObjEditNode",
        CLS_OBJECT_EDITOR_ARRAY_ITEM: "wvObjEditItem",
        CLS_OBJECT_EDITOR_ARRAY_ITEM_SEPARATOR: "wvObjEditItemSeparator",
        CLS_OBJECT_EDITOR_ARRAY_CONTAIER: "wvObjEditArrDiv",
        CLS_OBJECT_EDITOR_OBJECT_CONTAIER: "wvObjEditObjDiv",
        CLS_OBJECT_EDITOR_INPUT_CONTAIER: "wvObjEditInputDiv",

        EVT_OBJECT_EDITOR_UPDATED: "wv-objectEditor-updated",
        EVT_OBJECT_EDITOR_VALIDATION_ERROR: "wv-objectEditor-validation-error",
        EVT_OBJECT_EDITOR_VALIDATION_SUCCESS: "wv-objectEditor-validation-success",
        //

        //Gallery {
        CLS_IMAGE_BOX_LOADING: "wvImageBox_loading",
        CLS_IMAGE_BOX_ERROR:   "wvImageBox_error",
        CLS_IMAGE_BOX_WIDER:   "wvImageBox_wider",
        CLS_IMAGE_BOX_HIGHER:  "wvImageBox_higher",
        CLS_IMAGE_VIEW:        "wvImageView",
        CLS_IMAGE_VIEW_ZOOM:   "wvImageView_zoom",
        CLS_IMAGE_VIEW_IMAGE:  "wvImageView_image",
        CLS_GALLERY:           "wvGallery",
        CLS_GALLERY_PREV:      "wvGallery_prev",
        CLS_GALLERY_NEXT:      "wvGallery_next",
        CLS_GALLERY_DISABLED:  "wvGallery_disabled",
        CLS_GALLERY_LIST:      "wvGallery_list",
        CLS_GALLERY_ITEM:      "wvGallery_item",
        CLS_GALLERY_THUMBNAIL: "wvGallery_thumbnail",
        CLS_GALLERY_CONTENT:   "wvGallery_content",
        CLS_GALLERY_IMAGE:     "wvGallery_image",
        CLS_GALLERY_SELECTED:  "wvGallery_selected",
        CLS_GALLERY_SIMPLE_THUMBNAILS: "wvGallerySimple_thumbnails",
        CLS_GALLERY_SIMPLE_PREVIEW:    "wvGallerySimple_preview",

        EVT_GALLERY_ADD:       "wv-gallery-add",
        EVT_GALLERY_REMOVE:    "wv-gallery-remove",
        EVT_GALLERY_CHANGE:    "wv-gallery-change",
        //} Gallery

        //Details control
        CLS_DETAILS: "wvDetails",
        CLS_DETAILS_TITLE: "wvDetailsTitle",
        CLS_DETAILS_TITLE_SHOWN: "wvDetailsTitleShown",
        CLS_DETAILS_TITLE_HIDDEN: "wvDetailsTitleHidden",
        CLS_DETAILS_CONTENT: "wvDetailsContent",
        CLS_DETAILS_CONTENT_VISIBLE: "wvDetailsContentVisible",
        CLS_DETAILS_CONTENT_HIDDEN: "wvDetailsContentHidden",

        EVT_DETAILS_SHOW: "wv-details-show",
        EVT_DETAILS_HIDE: "wv-details-hide"
    };

    var CURTAIN_ZORDER  = 500000;
    var TOAST_ZORDER   = 1000000;

    var fToastCount = 0;

    published.toast = function(msg, type, duration){
      var self = {};
      if (!WAVE.intValidPositive(duration)) duration = 2500;

      var div = document.createElement("div");
      var t = WAVE.strEmpty(type)? "" : "_"+type;
      div.className = published.CLS_TOAST + t;
      div.style.zIndex = TOAST_ZORDER;
      div.innerHTML = msg;
      div.style.left = 0;
      document.body.appendChild(div);
      var ml = Math.round(div.offsetWidth / 2);
      div.style.left = "50%";
      div.style.marginLeft = - ml + "px";
      fToastCount++;
      if (fToastCount>1){
        div.style.marginLeft =  - (ml + (fToastCount *  4)) + "px";
        div.style.marginTop = - (fToastCount *  4) + "px";
      }

      var fClosed = false;

      self.close = function(){
        if (fClosed) return false;
        document.body.removeChild(div);
        fToastCount--;
        fClosed = true;
        return true;
      };

      self.closed = function() { return fClosed;};

      setTimeout(function(){ self.close(); }, duration);
      return self;
    };


    var fCurtains = [];
    var fCurtainDIV = null;
    var fBodyOverflow = null;

    function preventFocusLeak(e) {
      if (fCurtains.length > 0){
        var topCurtain = fCurtains[fCurtains.length -1];
        var t = e.target;
        if (typeof(t) !== tUNDEFINED && t !== null){
           if(!WAVE.isParentOf(topCurtain, t)) {
            topCurtain.tabIndex = 0;
            topCurtain.focus();
          }
        }
      }
    }

    published.curtainOn = function(){
        try{ document.activeElement.blur(); } catch(e){}

        var div = document.createElement("div");
        div.style.backgroundColor = "rgba(127,127,127,0.45)";
        div.className = published.CLS_CURTAIN;
        div.style.position = "fixed";
        div.style.left = "0";
        div.style.top = "0";
        div.style.width = "100%";
        div.style.height = "100%";
        div.style.zIndex = CURTAIN_ZORDER + fCurtains.length;
        document.body.appendChild(div);
        fCurtainDIV = div;

        fCurtains.push(div);
        //prevent scrolling and add focus handler
        if (fCurtains.length === 1 && !WAVE.Platform.Mobile) {
          window.addEventListener("focus", preventFocusLeak, true);

          fBodyOverflow = document.body.style.overflow;
          document.body.style.overflow = "hidden";
        }
    };

    published.curtainOff = function(){
        if (fCurtains.length===0) return;
        var div = fCurtains[fCurtains.length-1];
        if (typeof(div.__DIALOG)!==tUNDEFINED){
            div.__DIALOG.cancel();
            return;
        }
                
        document.body.removeChild(div);
        WAVE.arrayDelete(fCurtains, div);
        //return scrolling and remove focus handler
        if (fCurtains.length === 0 && !WAVE.Platform.Mobile) {
          window.removeEventListener("focus", preventFocusLeak, true);
          document.body.style.overflow = fBodyOverflow;
        }
    };

    published.isCurtain = function(){ return fCurtains.length>0;};

    //Returns currently active dialog or null
    published.currentDialog = function(){
        for(var i=fCurtains.length-1; i>=0; i--)
         if (typeof(fCurtains[i].__DIALOG)!==tUNDEFINED) return fCurtains[i].__DIALOG;
        return null;
    };

    //Dialog window:
    //   {title: 'Title', content: 'html markup', cls: 'className', widthpct: 75, heightpct: 50, onShow: function(dlg){}, onClose: function(dlg, result){return true;}}
    published.Dialog = function(init)
    {
        if (!WAVE.isObject(init)) init = {};

        var fOnShow = WAVE.isFunction(init.onShow) ? init.onShow : function(){};
        var fOnClose = WAVE.isFunction(init.onClose) ? init.onClose : function(){ return true;};

        var dialog = this;
        WAVE.extend(dialog, WAVE.EventManager);

        published.curtainOn();

        var fdivBase = document.createElement("div");
        fdivBase.__DIALOG = this;
        fdivBase.className = published.CLS_DIALOG_BASE + ' ' + WAVE.strDefault(init['cls']);
        fdivBase.style.position = "fixed";
        fdivBase.style.zIndex = CURTAIN_ZORDER + fCurtains.length;

        var fWidthPct = WAVE.intValidPositive(init.widthpct) ? init.widthpct : 0;
        var fHeightPct = WAVE.intValidPositive(init.heightpct) ? init.heightpct : 0;

        if (fWidthPct>100) fWidthPct = 100;
        if (fHeightPct>100) fHeightPct = 100;


        document.body.appendChild(fdivBase);

        fCurtains.push(fdivBase);

        var fdivTitle = document.createElement("div");
        fdivTitle.className = published.CLS_DIALOG_TITLE;
        fdivTitle.innerHTML = WAVE.strDefault(init['title'], 'Dialog');
        fdivBase.appendChild(fdivTitle);

        var fdivContent = document.createElement("div");
        fdivContent.className = published.CLS_DIALOG_CONTENT;
        fdivContent.innerHTML = WAVE.strDefault(init['content'], '&nbsp;');
        fdivBase.appendChild(fdivContent);

        function adjustBounds(){
          var sw = $(window).width();
          var sh = $(window).height();
          var cx = sw / 2;
          var cy = sh / 2;

          var w = fWidthPct===0 ? fdivBase.offsetWidth : Math.round( sw * (fWidthPct/100));
          var h = fHeightPct===0 ? fdivBase.offsetHeight : Math.round( sh * (fHeightPct/100));

          fdivBase.style.left = Math.round(cx - (w / 2)) + "px";
          fdivBase.style.top = Math.round(cy - (h / 2)) + "px";
          fdivBase.style.width  = fWidthPct===0  ? "auto" : w + "px";
          fdivBase.style.height = fHeightPct===0 ? "auto" : h + "px";

        //  fdivContent.style.width  = fWidthPct===0  ? "auto" : w - (fdivContent.offsetLeft*2) + "px";
          fdivContent.style.height  = fWidthPct===0  ? "auto" :
                                                      h - (fdivContent.offsetTop + fdivContent.offsetLeft) + "px";//todo This may need to be put as published.OFFSETY that depends on style
        }

        var fResult = published.DLG_UNDEFINED;

        //returns dialog result or DLG_UNDEFINED
        this.result = function(){ return fResult; };

        //closes dialog with the specified result and returns the result
        this.close = function(result){
            if (typeof(result)===tUNDEFINED) result = published.DLG_CANCEL;
            if (fResult!==published.DLG_UNDEFINED) return fResult;

            if (!fOnClose(this, result)) return published.DLG_UNDEFINED;//aka CloseQuery

            fResult = result;

            document.body.removeChild(fdivBase);
            WAVE.arrayDelete(fCurtains, fdivBase);
            published.curtainOff();
            this.eventInvoke(published.EVT_DIALOG_CLOSE, result);
            return result;
        };

        //closes dialog with OK result
        this.ok = function(){ this.close(published.DLG_OK); };

        //closes dialog with CANCEL result
        this.cancel = function(){ this.close(published.DLG_CANCEL); };

        //get/set title
        this.title = function(val){
          if (typeof(val)===tUNDEFINED || val===null) return fdivTitle.innerHTML;
          fdivTitle.innerHTML = val;
          adjustBounds();
        };

        //get/set content
        this.content = function(val){
          if (typeof(val)===tUNDEFINED || val===null) return fdivContent.innerHTML;
          fdivContent.innerHTML = val;
          adjustBounds();
        };

        this.contentDIV = function() { return fdivContent; };

        //gets/sets width in screen size pct 0..100, where 0 = auto
        this.widthpct = function(val){
          if (typeof(val)===tUNDEFINED || val===fWidthPct) return fWidthPct;
          fWidthPct = val;
          adjustBounds();
        };

        //gets/sets height in screen size pct 0..100, where 0 = auto
        this.widthpct = function(val){
          if (typeof(val)===tUNDEFINED || val===fHeightPct) return fHeightPct;
          fHeightPct = val;
          adjustBounds();
        };

        var tmr = null;
        $(window).resize(function(){
           if (tmr) clearTimeout(tmr);//prevent unnecessary adjustments when too many resizes happen
           tmr = setTimeout(function(){  adjustBounds(); tmr = null; }, 500);
        });

        fOnShow(this);

        adjustBounds();
    };//dialog

    //Displays a simple confirmation propmt dialog
    published.showConfirmationDialog = function (title, content, buttons, callback, options) {
      if (!WAVE.isObject(options)) options = {};
      if (!WAVE.isArray(buttons)) buttons = [];

      content = WAVE.strDefault(content, 'Please confirm');
      var btnCls = WAVE.strDefault(options['btnCls']);
      var footerCls = WAVE.strDefault(options['footerCls']);

      function createButtonHtml(lbl, rslt) {
          var btnTemplate = '<button class="@btnClass@" onclick="WAVE.GUI.currentDialog().close(\'@result@\');">@label@</button>';
          return WAVE.strHTMLTemplate(
                                     btnTemplate,
                                     {
                                       btnClass: published.CLS_DIALOG_CONFIRM_BUTTON + ' ' + btnCls,
                                       result: rslt,
                                       label: lbl
                                     });
      }

      var btnYes = '';
      var btnNo = '';
      var btnOk = '';
      var btnCancel = '';

      if (WAVE.inArray(buttons, published.DLG_YES))
         btnYes = createButtonHtml('Yes', published.DLG_YES);
      if (WAVE.inArray(buttons, published.DLG_NO))
         btnNo = createButtonHtml('No', published.DLG_NO);
      if (WAVE.inArray(buttons, published.DLG_OK))
         btnOk = createButtonHtml('OK', published.DLG_OK);
      if (WAVE.inArray(buttons, published.DLG_CANCEL))
         btnCancel = createButtonHtml('Cancel', published.DLG_CANCEL);

      var fullContent =
        '<div>'+
          content+
          '<div class="'+published.CLS_DIALOG_CONFIRM_FOOTER+' '+footerCls+'" style="text-align:center; padding: 5px 0 0 0">'+
            btnYes + btnNo + btnOk + btnCancel +
          '</div>'+
        '<div>';

      var dialog = new published.Dialog(
        {
          title: WAVE.strDefault(title, 'Confirmation'),
          content: fullContent,
          cls: WAVE.strDefault(options['cls']),
          onShow: function(dlg){},
          onClose: callback
        });
    };

    var fDirty = false;

    //gets/sets dirty flag
    published.dirty = function(val){
      if (typeof(val)===tUNDEFINED) return fDirty;
      fDirty = val;
    };

    //Returns true if dirty flag is set or dialog shown
    published.isDirty = function(){
      return fDirty || published.currentDialog()!==null;
    };
    
    //Set to true to bypass check on page unload
    published.SUPPRESS_UNLOAD_CHECK = false;

    window.onbeforeunload = function(e){
      if (!published.SUPPRESS_UNLOAD_CHECK && (published.isDirty() || WAVE.RecordModel.isDirty()))
        (e || window.event).returnValue = "The page is in the middle of the data entry. If you navigate away/close the page now some changes will be lost. Are you sure?";
    };

    var PUZZLE_DFLT_HELP = "Please enter the following security information by touching the symbols below";

    //Puzzle keypad class: new PuzzleKeypad({DIV: divPuzzle, Image: '/security/captcha?for=login', Question: 'Your first name spelled backwards'});
    published.PuzzleKeypad = function(init)
    {
        if (typeof(init)===tUNDEFINED || init===null || typeof(init.DIV)===tUNDEFINED || WAVE.strEmpty(init.Image)) throw "PuzzleKeypad.ctor(init.DIV, init.Image)";
        
        var keypad = this;
        WAVE.extend(keypad, WAVE.EventManager);

        var rndKey = WAVE.genRndKey(4);
        var fDIV = init.DIV;
        var fHelp = WAVE.strEmpty(init.Help) ? PUZZLE_DFLT_HELP : init.Help;
        var fQuestion = WAVE.strEmpty(init.Question) ? "" : init.Question;
        var fValue = [];
        var fImage = init.Image;

        var fdivHelp = null;
        var fdivQuestion = null;
        var ftbValue = null;
        var fbtnClear = null;
        var fimgKeys = null;

        function rebuild(){
            var seed = "pzl_"+rndKey+"_"+WAVE.genAutoincKey("_puzzle#-?Keypad@Elements");

            var args = {
              hid: "divHelp_"+seed,
              qid: "divQuestion_"+seed,
              tid: "tbValue_"+seed,
              bid: "btnClear_"+seed,
              iid: "img_"+seed,
              help: fHelp,
              question: fQuestion, 
              img: fImage + "&req="+WAVE.genRndKey(),
              clear: "Clear"
            };
  
            var html = WAVE.strTemplate(
                        "<div class='wvPuzzleHelp'     id='@hid@'>@help@</div>"+
                        "<div class='wvPuzzleQuestion' id='@qid@'>@question@</div>"+
                        "<div class='wvPuzzleInputs'> <input id='@tid@' placeholder='···············' type='text' disabled /><button id='@bid@'>@clear@</button> </div>"+
                        "<div class='wvPuzzleImg'> <img id='@iid@' src='@img@'/></div>", args);

            fDIV.innerHTML = html;

            $("#"+args.bid).click(function(evt){//CLEAR
               evt.preventDefault();
               ftbValue.value = "";
               fValue = [];
               keypad.eventInvoke(published.EVT_PUZZLE_KEYPAD_CHANGE, fValue);
             });
             
            $("#"+args.iid).click(function(e){//IMAGE click
               var offset = $(this).offset();
               var point = {x: Math.round(e.pageX-offset.left), y: Math.round(e.pageY-offset.top)};
               fValue.push(point);
               ftbValue.value += "*";
               keypad.eventInvoke(published.EVT_PUZZLE_KEYPAD_CHANGE, fValue);
             });


            fdivHelp = WAVE.id(args.hid);
            fdivQuestion = WAVE.id(args.qid);
            ftbValue  = WAVE.id(args.tid);
            fbtnClear = WAVE.id(args.bid);
            fimgKeys  = WAVE.id(args.iid);
        }

        this.destroy = function() {
          fDIV.innerHTML = "";
        };

        //Returns value as an array of points where user clicked
        this.value = function(){ return fValue;};

        //Returns value as a JSON array of points where user clicked
        this.jsonValue = function(){ return JSON.stringify(fValue);};

        this.help = function(val){
           if (typeof(val)===tUNDEFINED) return fHelp;
           if (WAVE.strEmpty(val)) fHelp = PUZZLE_DFLT_HELP;
           else fHelp = val;
           fdivHelp.innerHTML = fHelp;
        };

        this.question = function(val){
           if (typeof(val)===tUNDEFINED) return fQuestion;
           if (WAVE.strEmpty(val)) fQuestion = "";
           else fQuestion = val;
           fdivQuestion.innerHTML = fQuestion;
        };

        this.image = function(url) {
           if (typeof(url)===tUNDEFINED) return fImage;
           if (WAVE.strEmpty(url)) fImage = "";
           else fImage = url;
           fimgKeys.src = fImage + "&req="+WAVE.genRndKey();
        };

        rebuild();
    };//keypad

    // Manages all ruler logic
    var RulerManager = function (cfg) {
      var self = this;

      WAVE.extend(this, WAVE.EventManager);

      var DEFAULT_SCOPE_NAME = "";

      var fScopeInfos = [], fScopeInfosWlk = WAVE.arrayWalkable(fScopeInfos); // [{name: , cfg: [{}], elementInfos: []}] 
      var fElementInfos = [], // [{element: , ruler: , cfg: {getInfo: , scopeMouseEnter: , scopeMouseLeave: , scopeMouseMove: }}, scopeInfos: []]
          fElementInfosWlk = WAVE.arrayWalkable(fElementInfos);

      var fRemovedFromConnected = false;

      function getElementInfo(element) {
       return fElementInfosWlk.wFirst(function(e) { return e.element === element; });
      }

      function getScopeInfo(scopeName) {
        return fScopeInfosWlk.wFirst(function(s) { return s.name === scopeName; });
      }

      function ensureScopeInfo(scopeName, cfg) {
        var scopeInfo = getScopeInfo(scopeName);
        if (scopeInfo === null) {
          scopeInfo = {name: scopeName, cfg: cfg || {}, elementInfos: []};
          fScopeInfos.push(scopeInfo);
        } else {
          if (cfg) scopeInfo.cfg = cfg;
        }
        return scopeInfo;
      }

      function ensureElementInfo(element, cfg) {
        var elementInfo = getElementInfo(element);
        if (elementInfo === null) {
          elementInfo = {element: element, cfg: cfg || {}, scopeInfos: []};

          var jElement = $(element);
          jElement.bind("mouseenter", onMouseEnter);
          jElement.bind("mouseleave", onMouseLeave);
          jElement.bind("mousemove", onMouseMove);

          var ruler = new ElementRuler(element, cfg);
          elementInfo.ruler = ruler;

          fElementInfos.push(elementInfo);
        } else {
          if (cfg) {
            elementInfo.cfg = cfg;
            elementInfo.ruler.cfg(cfg);
          }
        }
        return elementInfo;
      }

      function ensureElementInScope(element, elementCfg, scopeName, scopeCfg, cfg) {
        var scopeInfo = ensureScopeInfo(scopeName, scopeCfg);
        var elementInfo = ensureElementInfo(element, elementCfg);
        if (scopeInfo.elementInfos.indexOf(elementInfo) === -1) scopeInfo.elementInfos.push(elementInfo);
        if (elementInfo.scopeInfos.indexOf(scopeInfo) === -1) elementInfo.scopeInfos.push(scopeInfo);
      }

      this.set = function(e) { // {element: , elementCfg: , scopeName: , scopeCfg: , cfg: {}}
        var element = e.element;
        var elementCfg = e.elementCfg; // {getTxt: , getMasterInfo: , getSlaveInfo: }
        var scopeName = e.scopeName || DEFAULT_SCOPE_NAME;
        var scopeCfg = e.scopeCfg;
        var cfg = e.cfg;

        ensureElementInScope(element, elementCfg, scopeName, scopeCfg, cfg);
      };

      this.unset = function (e) {
        var element = e.element;
        var scopeName = e.scopeName;

        var elementInfo = getElementInfo(e.element);
        if (elementInfo === null) return;
        if(scopeName) {
          var scopeInfo = getScopeInfo(scopeName);
          if (scopeInfo !== null) WAVE.arrayDelete(scopeInfo.elements, elementInfo);
          WAVE.arrayDelete(elementInfo.scopes, scopeInfo);
        } else {
          WAVE.arrayWalkable(elementInfo.scopeInfos).wEach(function(s) { WAVE.arrayDelete(s.elementInfos, elementInfo); });
          elementInfo.scopeInfos.splice(0, elementInfo.scopeInfos.length);
        }
        
        if (elementInfo.scopeInfos.length === 0) {
          var jElement = $(element);
          jElement.unbind("mouseenter", onMouseEnter);
          jElement.unbind("mouseleave", onMouseLeave);
          jElement.unbind("mousemove", onMouseMove);
          WAVE.arrayDelete(fElementInfos, elementInfo);
        }
      };

      this.setScope = function (scopeName, cfg) {
        ensureScopeInfo(scopeName, cfg);
      };

      this.mouseMove = function(masterRes, scopeNames) {
        var scopeInfos = fScopeInfosWlk.wWhere(function(si) { return scopeNames.indexOf(si.name) !== -1;}).wToArray();
        moveSlaves(null, scopeInfos, masterRes);
      };

      this.mouseEnter = function (scopeNames) {
        var scopeInfos = fScopeInfosWlk.wWhere(function (si) { return scopeNames.indexOf(si.name) !== -1; }).wToArray();
        enterSlaves(null, scopeInfos);
      };

      this.mouseLeave = function (scopeNames) {
        var scopeInfos = fScopeInfosWlk.wWhere(function (si) { return scopeNames.indexOf(si.name) !== -1; }).wToArray();
        leaveSlaves(null, scopeInfos);
      };

      function onMouseMove(e) {
        var el = e.currentTarget;
        var elementInfo = getElementInfo(el);
        if (elementInfo === null) return;

        var masterRes = elementInfo.ruler.onMouseMove(e);

        // {scope: scopeInfo, masterRes: masterRes, event: e}

        moveSlaves(el, elementInfo.scopeInfos, masterRes);

        var scopeNames = WAVE.arrayWalkable(elementInfo.scopeInfos).wSelect(function(si) { return si.name; }).wToArray();
        self.eventInvoke(published.EVT_RULER_MOUSE_MOVE, masterRes, scopeNames);
      }

      function onMouseEnter(e) {
        var el = e.currentTarget;
        var elementInfo = getElementInfo(el);
        if (elementInfo === null) return;

        elementInfo.ruler.onMouseEnter();

        enterSlaves(el, elementInfo.scopeInfos);

        var scopeNames = WAVE.arrayWalkable(elementInfo.scopeInfos).wSelect(function (si) { return si.name; }).wToArray();
        self.eventInvoke(published.EVT_RULER_MOUSE_ENTER, scopeNames);
      }

      function onMouseLeave(e) {
        var el = e.currentTarget;
        var elementInfo = getElementInfo(el);
        if (elementInfo === null) return;

        elementInfo.ruler.onMouseLeave(e);

        leaveSlaves(el, elementInfo.scopeInfos);

        var scopeNames = WAVE.arrayWalkable(elementInfo.scopeInfos).wSelect(function (si) { return si.name; }).wToArray();
        self.eventInvoke(published.EVT_RULER_MOUSE_LEAVE, scopeNames);
      }

      function moveSlaves(element, scopeInfos, masterRes) {
        var slaveEvt = { scope: null, masterRes: masterRes};

        for (var iSi in scopeInfos) {
          var si = scopeInfos[iSi];
          slaveEvt.scope = si;
          for (var iEi in si.elementInfos) {
            var ei = si.elementInfos[iEi];
            if (element === ei.element) continue;
            ei.ruler.onScopeMouseMove(slaveEvt);
          }
        }
      }

      function enterSlaves(element, scopeInfos) {
        for (var iSi in scopeInfos) {
          var si = scopeInfos[iSi];
          for (var iEi in si.elementInfos) {
            var ei = si.elementInfos[iEi];
            if (element === ei.element) continue;
            ei.ruler.onScopeMouseEnter(si);
          }
        }
      }

      function leaveSlaves(element, scopeInfos) {
        for (var iSi in scopeInfos) {
          var si = scopeInfos[iSi];
          for (var iEi in si.elementInfos) {
            var ei = si.elementInfos[iEi];
            if (element === ei.element) continue;
            ei.ruler.onScopeMouseLeave(si);
          }
        }
      }

      //function fireMouseMove(masterRes, scopeNames) { self.eventInvoke(published.EVT_RULER_MOUSE_MOVE, masterRes, scopeNames); }

    };//RulerManager

    var ElementRuler = function(element, cfg) {
      var self = this;

      var fElement = element;
      var fCfg = cfg || {};
      this.cfg = function (val) {
        if (typeof(val) !== tUNDEFINED && val !== cfg) {
          fCfg = val;
        }
        return fCfg;
      };

      var fRulerHintCls = fCfg.hintCls || "wvRulerHint";
      var fRulerSightCls = fCfg.sightCls || "wvRulerSight";

      var fSigntSize = fCfg.sightSize || 8;

      var fMasterElementsCreated = false, fSlaveElementsCreated = false;

      var divHint = null, divSightLeft = null, divSightTop = null, divSightRight = null, divSightBottom = null, divSightCenter = null,
          divSightBoxLeft = null, divSightBoxTop = null, divSightBoxRight = null, divSightBoxBottom = null, divSightBoxCenter = null,
          divSlave = null;

      this.onMouseEnter = function(e) {
        ensureMasterElements();
      };

      this.onMouseLeave = function(e) {
        ensureNoMasterElements();
      };

      this.onMouseMove = function (e) {
        ensureNoSlaveElements();
        ensureMasterElements();

        var parentRc;
        if (fCfg.parentRc) {
          parentRc = fCfg.parentRc;
        } else {
          var parentRect = fElement.getBoundingClientRect();
          parentRc = WAVE.Geometry.toRectWH(parentRect.left, parentRect.top, parentRect.width, parentRect.height);
        }

        var elX = e.clientX - parentRc.left();
        var elY = e.clientY - parentRc.top();

        divHint.style.left = e.clientX + "px";
        divHint.style.top = e.clientY + "px";

        var txt = null;

        if (fCfg.getTxt) {
          txt = fCfg.getTxt({
            clientPoint: new WAVE.Geometry.Point(e.clientX, e.clientY),
            divHint: divHint
          });
        } else {
          txt = elX + ", " + elY;
        }

        divHint.innerHTML = txt;

        var divHintRect = divHint.getBoundingClientRect();

        var hintRc = WAVE.Geometry.toRectWH(divHintRect.left, divHintRect.top, divHintRect.width, divHintRect.height);
        
        var hintPos = getHintPos(e.clientX, e.clientY, hintRc, parentRc);

        divHint.style.left = hintPos.x() + "px";
        divHint.style.top = hintPos.y() + "px";

        locateSight(e.clientX, e.clientY, parentRc);

        var r = fCfg.getMasterInfo ?
                  fCfg.getMasterInfo({clientPoint: new WAVE.Geometry.Point(e.clientX, e.clientY)}) :
                  { elementRcPoint: new WAVE.Geometry.Point(elX, elY), isInParentRc: true }; 

        ensureDivsVisibility(e.clientX, e.clientY, r.isInParentRc);

        return r;
      };

          function locateSight(clientX, clientY, parentRc) {
            var left = clientX - fSigntSize;
            if (left < parentRc.left()) left = parentRc.left();

            var right = clientX + fSigntSize;
            if (right > parentRc.right()) right = parentRc.right();

            var top = clientY - fSigntSize;
            if (top < parentRc.top()) top = parentRc.top();

            var bottom = clientY + fSigntSize;
            if (bottom > parentRc.bottom()) bottom = parentRc.bottom();

            divSightLeft.style.left = parentRc.left() + "px";
            divSightLeft.style.width = (left - parentRc.left()) + "px";
            divSightLeft.style.top = clientY + "px";

            divSightRight.style.left = right + "px";
            divSightRight.style.width = (parentRc.right() - right) + "px";
            divSightRight.style.top = clientY + "px";

            divSightTop.style.top = parentRc.top() + "px";
            divSightTop.style.height = (top - parentRc.top()) + "px";
            divSightTop.style.left = clientX + "px";

            divSightBottom.style.top = bottom + "px";
            divSightBottom.style.height = (parentRc.bottom() - bottom) + "px";
            divSightBottom.style.left = clientX + "px";

            divSightBoxLeft.style.top = top + "px";
            divSightBoxLeft.style.height = (bottom - top) + "px";
            divSightBoxLeft.style.left = left + "px";

            divSightBoxRight.style.top = top + "px";
            divSightBoxRight.style.height = (bottom - top) + "px";
            divSightBoxRight.style.left = right + "px";

            divSightBoxTop.style.left = left + "px";
            divSightBoxTop.style.width = (right - left) + "px";
            divSightBoxTop.style.top = top + "px";

            divSightBoxBottom.style.left = left + "px";
            divSightBoxBottom.style.width = (right - left) + "px";
            divSightBoxBottom.style.top = bottom + "px";
          }

          function ensureDivsVisibility(clientX, clientY, isInParentRc) {
            var allDivsWlk = getAllDivsWlk();
            if (!isInParentRc)
            {
              allDivsWlk.wEach(function (d) {
                if (d !== null && d.style.visibility !== "hidden")
                  d.style.visibility = "hidden";
              });
            } else {
              allDivsWlk.wEach(function (d) {
                if (d !== null && d.style.visibility === "hidden")
                  d.style.visibility = "visible";
              });
            }
          }

          function getAllDivsWlk() {
            return new WAVE.arrayWalkable([
                divHint, divSightLeft, divSightTop, divSightRight, divSightBottom, divSightCenter,
                divSightBoxLeft, divSightBoxTop, divSightBoxRight, divSightBoxBottom, divSightBoxCenter,
                divSlave]);
          }

      this.onScopeMouseEnter = function (scope) {
        ensureSlaveElements();
      };

      this.onScopeMouseLeave = function (scope) {
        ensureNoSlaveElements();
      };

      this.onScopeMouseMove = function (e) { // {scope: scopeInfo, masterRes: masterRes, event: e}
        ensureNoMasterElements();
        ensureSlaveElements();

        var slaveParentRc = self.getParentRc();

        var clientX;
        var clientY;

        var elementRcPoint;
        var slaveIsInParentRc;
        if (fCfg.getSlaveInfo) {
          var slaveInfo = fCfg.getSlaveInfo({ masterRes: e.masterRes });
          //elementRcPoint = slaveInfo.elementRcPoint;
          slaveIsInParentRc = slaveInfo.isInParentRc;
          clientX = slaveInfo.clientPoint.x();
          clientY = slaveInfo.clientPoint.y();
        } else {
          elementRcPoint = e.masterRes.elementRcPoint;
          slaveIsInParentRc = true;
          clientX = slaveParentRc.left() + elementRcPoint.x();
          clientY = slaveParentRc.top() + elementRcPoint.y();
        }

        

        ensureDivsVisibility(clientX, clientY, e.masterRes.isInParentRc && slaveIsInParentRc);

        divSlave.style.top = slaveParentRc.top() + "px";
        divSlave.style.height = slaveParentRc.height() + "px";
        divSlave.style.left = clientX + "px";
      };

      this.getParentRc = function () {
        var parentRc;

        if (fCfg.parentRc) {
          parentRc = fCfg.parentRc;
        } else {
          var parentRect = fElement.getBoundingClientRect();
          parentRc = WAVE.Geometry.toRectWH(parentRect.left, parentRect.top, parentRect.width, parentRect.height);
        }

        return parentRc;
      };

      function ensureMasterElements() {
        if (fMasterElementsCreated) return;

        divHint = document.createElement("div");
        divHint.id = "WAVE_GUI_Ruler";
        divHint.innerHTML = "Hint DIV";
        divHint.className = fRulerHintCls;
        divHint.style.position = "absolute";
        document.body.appendChild(divHint);

        divSightRight = createLineDiv(true);
        divSightLeft = createLineDiv(true);
        divSightBoxRight = createLineDiv(false);
        divSightBoxLeft = createLineDiv(false);

        divSightTop = createLineDiv(false);
        divSightBottom = createLineDiv(false);
        divSightBoxTop = createLineDiv(true);
        divSightBoxBottom = createLineDiv(true);

        fMasterElementsCreated = true;
      }

      function ensureNoMasterElements() {
        if (!fMasterElementsCreated) return;

        removeDiv(divHint);           divHint = null;
        removeDiv(divSightRight);     divSightRight = null;
        removeDiv(divSightLeft);      divSightLeft = null;
        removeDiv(divSightBoxRight);  divSightBoxRight = null;
        removeDiv(divSightBoxLeft);   divSightBoxLeft = null;
        removeDiv(divSightTop);       divSightTop = null;
        removeDiv(divSightBottom);    divSightBottom = null;
        removeDiv(divSightBoxTop);    divSightBoxTop = null;
        removeDiv(divSightBoxBottom); divSightBoxBottom = null;

        fMasterElementsCreated = false;
      }

      function ensureSlaveElements() {
        if (fSlaveElementsCreated) return;
        divSlave = createLineDiv(false);
        fSlaveElementsCreated = true;
      }

      function ensureNoSlaveElements() {
        if (!fSlaveElementsCreated) return;
        removeDiv(divSlave); divSlave = null;
        fSlaveElementsCreated = false;
      }

        function createLineDiv(horizontal) {
          var div = document.createElement("div");
          if (horizontal) div.style.height = "1px"; else div.style.width = "1px";
          div.style.position = "absolute";
          div.className = fRulerSightCls;
          document.body.appendChild(div);
          return div;
        }

        function removeDiv(div) {
          if (div === null) return;
          document.body.removeChild(div); 
        }

        function getHintPos(cx, cy, hintRc, parentElementRc) {
          var resultRc = null;
          var hintSquare = hintRc.square();

          var minPenalty = Number.MAX_VALUE;
          for (var rad = 0; rad < 2 * Math.PI; rad += Math.PI / 4) {
            var rc = WAVE.Geometry.rotateRectAroundCircle(cx, cy, 20, hintRc.width(), hintRc.height(), rad);

            var visibleSquare = WAVE.Geometry.overlapAreaRect(parentElementRc, rc);

            var penalty = hintSquare - visibleSquare;

            if (penalty === 0) {
              resultRc = rc;
              break;
            }

            if (penalty < minPenalty) {
              minPenalty = penalty;
              resultRc = rc;
            }
          }

          return resultRc.topLeft();
        }
    };//ElementRuler

    var fRulerManager = null;

    published.rulerSet = function (e) { // {element: , elementCfg: , scopeName: , scopeCfg: , cfg: {}}
      if (fRulerManager === null) fRulerManager = new RulerManager();
      fRulerManager.set(e);
    };

    published.rulerUnset = function(e) { // {element: , scope: }}
      if (fRulerManager === null) return;
      fRulerManager.unset(e);
    };

    published.rulerSetScope = function (scopeName, cfg) { 
      if (fRulerManager === null) fRulerManager = new RulerManager();
      fRulerManager.setScope(scopeName, cfg);
    };

    published.rulerMouseMove = function(masterRes, scopeNames) {
      if (fRulerManager === null) return;
      fRulerManager.mouseMove(masterRes, scopeNames);
    };

    published.rulerMouseEnter = function (scopeNames) {
      if (fRulerManager === null) return;
      fRulerManager.mouseEnter(scopeNames);
    };

    published.rulerMouseLeave = function (scopeNames) {
      if (fRulerManager === null) return;
      fRulerManager.mouseLeave(scopeNames);
    };

    published.rulerEventBind = function(e, handler) {
      if (fRulerManager === null) fRulerManager = new RulerManager();
      fRulerManager.eventBind(e, handler);
    };

    // Implements inter-window browser commenication
    published.WindowConnector = function (cfg) {
      cfg = cfg || {};

      var self = this;

      var fWnd = cfg.wnd || window;

      var fDomain = fWnd.location.protocol + "//" + fWnd.location.host;

      var fConnectedWindows = [], fConnectedWindowsWlk = WAVE.arrayWalkable(fConnectedWindows);
      this.connectedWindows = function () { return WAVE.arrayShallowCopy(fConnectedWindows); };

      this.openWindow = function (href) {
        var win = window.open(href || window.location.href);
        fConnectedWindowsWlk.wEach(function (w) {
          w.Connector.addWindow(win);
        });
        self.addWindow(win);
      };

      this.closeCurrentWindow = function () {
        fConnectedWindowsWlk.wEach(function (w) { w.Connector.removeWindow(fWnd); });
      };

      this.addWindow = function (e) {
        fConnectedWindows.push(e.window);
      };

      this.removeWindow = function (w) {
        WAVE.arrayDelete(fConnectedWindows, w);
      };

      this.callWindowFunc = function (func) { // func is callback of type function(w) { }
        var closedWindows;
        fConnectedWindowsWlk.wEach(function (cw) {
          if (cw.closed) {
            if (!closedWindows) closedWindows = [];
            closedWindows.push(cw);
          } else {
            func(cw);
          }
        });

        if (closedWindows) {
          var closedWindowsWlk = WAVE.arrayWalkable(closedWindows);
          closedWindowsWlk.wEach(function (rw) {
            self.removeWindow(rw);
          });
          closedWindowsWlk.wEach(function (rw) {
            fConnectedWindowsWlk.wEach(function (w) {
              w.Connector.removeWindow(rw);
            });
          });
        }
      };

      this.logMsg = function (msg) {
        console.log(msg);
      };

      if (fWnd.opener && fWnd.opener.Connector) {
        self.addWindow(fWnd.opener);
        var openerConnectedWindows = fWnd.opener.Connector.connectedWindows();
        for (var i in openerConnectedWindows) {
          var cw = openerConnectedWindows[i];
          if (cw === fWnd) continue;
          if (cw.closed) continue;
          self.addWindow({ window: cw });
        }
      }
    };//WindowConnector

    // Ensures that wnd (window by default) has Connector property of type WindowConnector.
    // Call this prior to first call to window.Connector
    published.connectorEnsure = function (wnd) {
      wnd = wnd || window;
      if (!wnd.Connector) wnd.Connector = new published.WindowConnector({ wnd: wnd });
    };

    var fTreeNodeIDSeed = 0;

    var TREE_NODE_TEMPLATE =  "<div id='exp_@id@' class='@wvTreeNodeButton@'></div>" +
                              "<div id='content_@id@' class='@wvTreeNodeContent@'>" +
                              "  <div id='own_@id@' class='@wvTreeNodeOwn@'></div>" +
                              "  <div id='children_@id@' class='@wvTreeNodeChildren@'>" +
                              "  </div>" +
                              "</div>";

    published.Tree = function(init) {

                var Node = function(nodeInit) {
                  nodeInit = nodeInit || {};

                  var node = this;

                  fTreeNodeIDSeed++;
                  var fElmID = "tvn_" + fTreeNodeIDSeed;

                  var fParent = nodeInit.parent || null;
                  if (fParent !== null && fParent.tree() !== tree) throw "Tree.Node.ctor(wrong parent tree)";
                  this.parent = function() { return fParent; };

                  var fDIV;
                  var fExpander;
                  var fDIVContent;
                  var fDIVOwn;
                  var fDIVChildren;
                  this.__divChildren = function() { return fParent !== null ? fDIVChildren : fTreeDIV; };

                  var fID = typeof(nodeInit.id) !== tUNDEFINED ? nodeInit.id.toString() : fTreeNodeIDSeed.toString();
                  this.id = function() { return fID; };

                          function updateExpansionContent() {
                            if (!fExpander) return;
                            fExpander.innerHTML = fExpanded ? fExpandedContent : fCollapsedContent;
                          }

                  var fExpandedContent = typeof(nodeInit.expandedContent) === tUNDEFINED ? tree.DEFAULT_NODE_EXPANDED_CONTENT : nodeInit.expandedContent;
                  this.expandedContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpandedContent;
                    fExpandedContent = val;
                    updateExpansionContent();
                  };

                  var fCollapsedContent = typeof(nodeInit.collapsedContent) === tUNDEFINED ? tree.DEFAULT_NODE_COLLAPSED_CONTENT : nodeInit.collapsedContent;
                  this.collapsedContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fCollapsedContent;
                    fCollapsedContent = val;
                    updateExpansionContent();
                  };

                  this.path = function() {
                    if (fParent === null) return "";
                    if (fParent === fRootNode) return fID;
                    return fParent.path() + "/" + fID;
                  };

                  // returns integer nesting level from root
                  this.level = function() {
                    if (fParent === null) return -1;
                    if (fParent === fRootNode) return 0;
                    return fParent.level() + 1;
                  };

                  this.html = function(val) { 
                    if (fParent === null) return null;
                    if (typeof(val) === tUNDEFINED) return fDIVOwn.innerHTML;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, oldContent: fDIVOwn.innerHTML, newContent: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_CONTENT, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fDIVOwn.innerHTML = val;

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, oldContent: fDIVOwn.innerHTML, newContent: val};
                    treeEventInvoke(published.EVT_TREE_NODE_CONTENT, evtArgsAfter);
                  };

                  var fSelectable = nodeInit.selectable !== false;
                  this.selectable = function(val) {
                    if (typeof(val) === tUNDEFINED) return fSelectable;
                    fSelectable = val;
                    if (fSelected && !val) node.selected(false);
                  };

                  var fSelected = nodeInit.selected === true;
                  this.selected = function(val, supressEvents) {
                    if (fTreeSelectionType === published.TREE_SELECTION_NONE) return false;

                    if (typeof(val) === tUNDEFINED) return fSelected;
                    if (val === fSelected) return;

                    if (fSelectable || (!fSelectable && !val)) {

                      if (supressEvents !== true) {
                        var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, value: val, abort: false};
                        treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsBefore);
                        if (evtArgsBefore.abort === true) return;
                      }
                    
                      fSelected = val;
                      fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();

                      if (supressEvents !== true) onNodeSelectionChanged(node, val);
                    }
                  };

                  this.wvTreeNode = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIV.className;
                    fDIV.className = val;
                  };

                  this.wvTreeNodeButton = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpander.className;
                    fExpander.className = val;
                  };

                  this.wvTreeNodeContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIVContent.className;
                    fDIVContent.className = val;
                  };


                  var fWVTreeNodeButtonExpanded = nodeInit.wvTreeNodeButtonExpanded || fNodeTemplateClsArgs.wvTreeNodeButtonExpanded;
                  this.wvTreeNodeButtonExpanded = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeButtonExpanded || fNodeTemplateClsArgs.wvTreeNodeButtonExpanded);
                    if (val === fWVTreeNodeButtonExpanded) return;
                    fWVTreeNodeButtonExpanded = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                  };

                  var fWVTreeNodeButtonCollapsed = nodeInit.wvTreeNodeButtonCollapsed || fNodeTemplateClsArgs.wvTreeNodeButtonCollapsed;
                  this.wvTreeNodeButtonCollapsed = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeButtonCollapsed || fNodeTemplateClsArgs.wvTreeNodeButtonCollapsed);
                    if (val === fWVTreeNodeButtonCollapsed) return;
                    fWVTreeNodeButtonCollapsed = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                  };


                  var fWVTreeNodeOwn = nodeInit.wvTreeNodeOwn || fNodeTemplateClsArgs.wvTreeNodeOwn;
                  this.wvTreeNodeOwn = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeOwn || fNodeTemplateClsArgs.wvTreeNodeOwn);
                    if (val === fWVTreeNodeOwn) return;
                    fWVTreeNodeOwn = val;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();
                  };

                  var fWVTreeNodeOwnSelected = nodeInit.wvTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected;
                  this.wvTreeNodeOwnSelected = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected);
                    if (val === fWVTreeNodeOwnSelected) return;
                    fWVTreeNodeOwnSelected = val;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();
                  };

                  this.wvTreeNodeChildren = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIVChildren.className;
                    fDIVChildren.className = val;
                  };

                  var fChildrenDisplayVisible = nodeInit.childrenDisplayVisible || fNodeChildrenDisplayVisible;
                  this.childrenDisplayVisible = function(val) {
                    if (typeof(val) === tUNDEFINED) return fChildrenDisplayVisible;
                    if (val === fChildrenDisplayVisible) return;
                    fChildrenDisplayVisible = val;
                    if (fExpanded) fDIVChildren.style.display = fChildrenDisplayVisible;
                  };

                  var fData = nodeInit.data;
                  this.data = function(val) { 
                    if (typeof(val) === tUNDEFINED) return fData;
                    if (fData === val) return;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, oldData: fData, newData: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_DATA, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fData = val;

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, oldData: fData, newData: val};
                    treeEventInvoke(published.EVT_TREE_NODE_DATA, evtArgsAfter);
                  };

                  var fExpanded = nodeInit.expanded === true;
                  this.expanded = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpanded;
                    if (fParent === null) return; // fake root node coudn't be expanded/collapsed
                    if (fExpanded === val) return;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, value: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_EXPANSION, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fExpanded = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                    fExpander.innerHTML = val ? fExpandedContent : fCollapsedContent;
                    fDIVChildren.style.display = val ? fChildrenDisplayVisible : "none";
                      
                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
                    treeEventInvoke(published.EVT_TREE_NODE_EXPANSION, evtArgsAfter);
                  };

                  var fChildren = [];
                  this.children = function() { return WAVE.arrayShallowCopy(fChildren); };
                  this.__children = function() { return fChildren; };

                  this.tree = function() { return tree; };

                  // public {

                  this.getDescendants = function() {
                    return node.__getDescendants(0);
                  };

                  this.__getDescendants = function(level) {
                    var walker = function() {
                      var childrenWalker = WAVE.arrayWalkable(fChildren).getWalker();
                      var childWalker = null;
                      var currentType = 0; // 0 - not initialized, 1 - child, 2 - grandchild
                      var nodeNum = -1;

                      this.reset = function() {
                        childrenWalker = WAVE.arrayWalkable(fChildren).getWalker();
                        currentType = 0;
                        nodeNum = -1;
                      };

                      this.moveNext = function() {
                        if (currentType === 0) {
                          return moveChildrenWalker();
                        } else if (currentType === 1) {
                          childWalker = childrenWalker.current().__getDescendants(level+1).getWalker();
                          if (!childWalker.moveNext()) {
                            return moveChildrenWalker();
                          } else {
                            currentType = 2;
                            return true;
                          }
                        } else if (currentType === 2) {
                          if (!childWalker.moveNext()) {
                            return moveChildrenWalker();
                          } else {
                            return true;
                          }
                        }

                        return true;

                        function moveChildrenWalker() {
                          if (!childrenWalker.moveNext()) { 
                            currentType = 0;
                            return false;
                          } else {
                            nodeNum++;
                            currentType = 1;
                            return true;
                          }
                        }

                      };

                      this.current = function() {
                        if (currentType === 1) return {level: level, nodeNum: nodeNum, node: childrenWalker.current()};
                        if (currentType === 2) return childWalker.current();
                      };
                    };

                    var walkable = {getWalker: function() { return new walker(); }, tree: tree};
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
                  };

                  this.getChildIdx = function (id, recursive) {
                    if (!recursive)
                      return WAVE.arrayWalkable(fChildren).wFirstIdx(function (c) { return WAVE.strSame(c.id(), id); });
                    else
                      return node.getDescendants(0).wFirstIdx(function (d) { return WAVE.strSame(d.node.id(), id); });
                  };

                  this.getChild = function (id, recursive) {
                    if (!recursive)
                      return WAVE.arrayWalkable(fChildren).wFirst(function (c) { return WAVE.strSame(c.id(), id); });
                    else {
                      var descendant = node.getDescendants(0).wFirst(function (d) { return WAVE.strSame(d.node.id(), id); });
                      return descendant ? descendant.node : null;
                    }
                  };

                  this.navigate = function(path) {
                    if (typeof(path)===tUNDEFINED || path===null) return null;

                    var segments;
                    if (WAVE.isArray(path)) {
                      segments = path;
                    } else if (!WAVE.strEmpty(path)) {
                      segments = path.split(/[\\/]+/);
                    } else {
                      return null;
                    }

                    function getNodeIdSegmentEqualsFunc(segment) {
                        var f = function (n) { return n.id() === segment; };
                        return f;
                    }

                    var node = null;
                    var childrenWalkable = WAVE.arrayWalkable(fChildren);
                    for(var i in segments) {
                      var segment = segments[i];
                      node = childrenWalkable.wFirst(getNodeIdSegmentEqualsFunc(segment));
                      if (node===null) return null;
                      childrenWalkable = WAVE.arrayWalkable(node.__children());
                    }
                    return node;
                  };

                  // Expands all parent nodes
                  this.unveil = function() {
                    var parent = fParent;
                    if (parent === null) return;

                    while(parent !== null) {
                      var parentParent = parent.parent();
                      if (parentParent !== null) parent.expanded(true);
                      parent = parentParent;
                    }
                  };

                  this.addChild = function(childNodeInit) {
                    if (typeof(childNodeInit) === tUNDEFINED) childNodeInit = {};

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, parentNode: node, childNodeInit: childNodeInit, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_ADD, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    childNodeInit.parent = node;
                    var childNode = new Node(childNodeInit);
                    fChildren.push(childNode);
                    if (fParent !== null)
                      fExpander.style.visibility = "visible";

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, parentNode: node, childNode: childNode};
                    treeEventInvoke(published.EVT_TREE_NODE_ADD, evtArgsAfter);

                    return childNode;
                  };

                  this.__removeChild = function(childNode) {
                    WAVE.arrayDelete(fChildren, childNode);
                    if (fChildren.length === 0 && fParent !== null)
                      fExpander.style.visibility = "hidden";
                  };

                  this.remove = function() {
                    if (fParent === null) {
                      console.error("Root node couldn't be removed");
                      return;
                    }

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, parentNode: fParent, childNode: node, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_REMOVE, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fDIV.parentNode.removeChild(fDIV);
                    fParent.__removeChild(node);

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, parentNode: fParent, childNode: node};
                    treeEventInvoke(published.EVT_TREE_NODE_REMOVE, evtArgsAfter);
                  };

                  this.removeAllChildren = function() {
                    var children = WAVE.arrayShallowCopy(fChildren);
                    for(var i in children) {
                      var child = children[i];
                      child.remove();
                    }
                  };
                  
                  // public }

                  if (fParent !== null) {
                    var nodeTemplateClsArgs = {
                      id: fElmID,

                      wvTreeNode            : nodeInit.wvTreeNode            || fNodeTemplateClsArgs.wvTreeNode,
                      wvTreeNodeContent     : nodeInit.wvTreeNodeContent     || fNodeTemplateClsArgs.wvTreeNodeContent,
                      wvTreeNodeOwn         : nodeInit.wvTreeNodeOwn         || fNodeTemplateClsArgs.wvTreeNodeOwn,
                      wvTreeNodeOwnSelected : nodeInit.wvTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected,
                      wvTreeNodeChildren    : nodeInit.wvTreeNodeChildren    || fNodeTemplateClsArgs.wvTreeNodeChildren
                    };

                    nodeTemplateClsArgs.wvTreeNodeButton = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();

                    var html = WAVE.strTemplate(TREE_NODE_TEMPLATE, nodeTemplateClsArgs);
                  
                    var parentDIV = fParent.__divChildren();
                    
                    fDIV = document.createElement("div");
                    fDIV.id = "root_" + fElmID;
                    var cls = "";
                    fDIV.className = nodeTemplateClsArgs.wvTreeNode;

                    fDIV.innerHTML = html;

                    parentDIV.appendChild(fDIV);
                    
                    fDIVContent = WAVE.id("content_" + fElmID);
                    fDIVOwn = WAVE.id("own_" + fElmID);
                    fDIVChildren = WAVE.id("children_" + fElmID);

                    fDIVOwn.innerHTML = nodeInit.html;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();

                    $(fDIVOwn).click(function() {
                      node.selected(!fSelected);
                    });

                    var exp = nodeInit.expanded === true;
                    fExpander = WAVE.id("exp_" + fElmID);
                    fExpander.innerHTML = fExpanded ? fExpandedContent : fCollapsedContent;
                    fExpander.style.visibility = "hidden";

                    $(fExpander).click(function() {
                      node.expanded(!fExpanded);
                    });

                    fDIVChildren.style.display = exp ? fChildrenDisplayVisible : "none";
                  }

                }; //Node class 


      if (typeof(init)===tUNDEFINED || init===null || typeof(init.DIV)===tUNDEFINED || init.DIV===null) throw "Tree.ctor(init.DIV)";

      var tree = this;
      WAVE.extend(tree, WAVE.EventManager);

      this.DEFAULT_NODE_EXPANDED_CONTENT = '-';
      this.DEFAULT_NODE_COLLAPSED_CONTENT = '+';

      var fTreeDIV = init.DIV;

      var fNodeTemplateClsArgs = {
        wvTreeNode                : init.wvTreeNode                || published.CLS_TREE_NODE,
        wvTreeNodeButtonExpanded  : init.wvTreeNodeButtonExpanded  || published.CLS_TREE_NODE_BUTTON_EXPANDED,
        wvTreeNodeButtonCollapsed : init.wvTreeNodeButtonCollapsed || published.CLS_TREE_NODE_BUTTON_COLLAPSED,
        wvTreeNodeContent         : init.wvTreeNodeContent         || published.CLS_TREE_NODE_CONTENT,
        wvTreeNodeOwn             : init.wvTreeNodeOwn             || published.CLS_TREE_NODE_OWN,
        wvTreeNodeOwnSelected     : init.wvTreeNodeOwnSelected     || published.CLS_TREE_NODE_OWN_SELECTED,
        wvTreeNodeChildren        : init.wvTreeNodeChildren        || published.CLS_TREE_NODE_CHILDREN
      };

      var fNodeChildrenDisplayVisible = init.childrenDisplayVisible || "block";

      var fTreeSelectionType = init.treeSelectionType || published.TREE_SELECTION_NONE;
      this.treeSelectionType = function(val) {
        if (typeof(fTreeSelectionType) === tUNDEFINED) return fTreeSelectionType;
        fTreeSelectionType = val;
      };

      function onNodeSelectionChanged(node, val) {
        var evtArgsAfter;

        if (fTreeSelectionType === published.TREE_SELECTION_SINGLE) {
          
          if (val === true) {
            fRootNode.getDescendants().wEach(function(n) {
              if (n.node !== node && n.node.selected() === true) {

                var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: n.node, value: n.node.selected()};
                treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsBefore);

                n.node.selected(false, true);

                var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: n.node, value: n.node.selected()};
                treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);
              }
            });
          }

          evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
          treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);

        } else if (fTreeSelectionType === published.TREE_SELECTION_MULTI) {
          evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
          treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);
        }
      }

      this.selectedNodes = function(val) {
        if (typeof(val) === tUNDEFINED)
          return fRootNode.getDescendants().wWhere(function(n) { return n.node.selected() === true;});

        if (val === null) throw "val couldn't be null";

        var selectedWalkable;
        if (WAVE.isFunction(val.wAny)) {
          selectedWalkable = val;
        } else if (WAVE.isArray(val)) {
          selectedWalkable = WAVE.arrayWalkable(val);
        } else {
          throw "val type is invalid";
        }

        fRootNode.getDescendants().wEach(function(n) {
          n.node.selected(selectedWalkable.wAny(function(nodeId){ return nodeId === n.node.id();}), true);
        });
      };

      var fSupressEvents = init.supressEvents === true;
      this.supressEvents = function(val) { 
        if (typeof(val) === tUNDEFINED) return fSupressEvents;
        fSupressEvents = val;
      };

      function treeEventInvoke(evt, args) {
        if (!fSupressEvents) tree.eventInvoke(evt, args);
      }

      var fRootNode = new Node();
      this.root = function() { return fRootNode; };
    };//Tree

    var fObjectInspectorEditorIDSeed = 0, fObjectInspectorEditorIDPrefix = "objinsp_edit_";
    // Visulalizes object properties hierarchy in editable form
    published.ObjectInspector = function (obj, cfg) {
      var self = this;

      WAVE.extend(this, WAVE.EventManager);

      var fTree = new published.Tree({ DIV: cfg.div });

      cfg = cfg || {};

      var fWVObjectInspectorEditor = cfg.wvObjectInspectorEditor || published.CLS_OBJECTINSPECTOR_EDITOR;
      this.wvObjectInspectorEditor = function (val) {
        if (typeof (val) === tUNDEFINED) return fWVObjectInspectorEditor;
        fWVObjectInspectorEditor = val;
      };

      var HTML_EDITOR = '<div class="@cls@">' +
                        '  <label for="@id@">@key@</label>' +
                        '  <input type="textbox" id="@id@" value="@val@" objpath="@objpath@">' +
                        '</div>';

      function onInput(e) {
          var keys = e.target.getAttribute("objpath").split(/\//);
          var objToEdit = obj;
          for (var i = 0; i < keys.length-1; i++)
              objToEdit = objToEdit[keys[i]];
              
          objToEdit[keys[keys.length - 1]] = e.target.value;
      }

      function build(troot, oroot, orootpath) {
        var keys = Object.keys(oroot);
        var cn;
        for (var iKey in keys) {
          var key = keys[iKey];
          var val = oroot[key];
          if (val !== null && typeof (val) !== "object") { //leaf
            var editorID = fObjectInspectorEditorIDPrefix + (fObjectInspectorEditorIDSeed++);
            var html = WAVE.strTemplate(HTML_EDITOR, {
              id: editorID, key: key, val: val,
              objpath: orootpath ? orootpath + "/" + key : key,
              cls: fWVObjectInspectorEditor
            });

            cn = troot.addChild({ html: html });
            
            var input = WAVE.id(editorID);
            input.objpath = cn.path();
            input.addEventListener("input", onInput);
          } else { //branch
            cn = troot.addChild({ html: key });
            build(cn, val, orootpath ? orootpath + "/" + key : key);
          }
        }
      }

      build(fTree.root(), obj, "");
    };//ObjectInspector

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
        }else if(type === "check") {
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


    var fTabsIdSeed = 0;
    //{
    //  "DIV" : DIV,
    //  "unswitchable" : false,
    //  "parse": true|false
    //  "tabs" : [ 
    //    {
    //      "title" : "text",
    //      "name" : "name of tab",
    //      "content" : "text or hml",
    //      "isHtml" : "true",
    //      "isActive" : "false",
    //      "visible" : true,
    //      "cls" : class name,
    //      "contId" : "id of content"
    //    }
    //  ]
    //}
    published.Tabs = function (init) {
      if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "Tabs.ctor(init.DIV)";

      var tabs = this;
      WAVE.extend(tabs, WAVE.EventManager);
      fTabsIdSeed++;
      var fSeed = "wvTabs_" + fTabsIdSeed;
      var fAttributePrefix = "data-tabs-";

      var fDIV = init.DIV;
      var fDIVDisplay = WAVE.styleOf(fDIV, "display");
      var fUnswitchable = WAVE.strAsBool(init.unswitchable, false);
      var fParse = WAVE.strAsBool(init.parse, false);
      var fTabs = WAVE.isArray(init.tabs) ? init.tabs : [];
      var fTSeed = 0;

      var Tab = function(tabInit) {
        fTSeed++;
        tabInit = !WAVE.isObject(tabInit) ? {} : tabInit;

        var tab = this;
        var fTitle = WAVE.strDefault(tabInit.title, fTSeed.toString());
        var fContent = WAVE.strDefault(tabInit.content);
        var fIsHtml = WAVE.strAsBool(tabInit.isHtml, true);
        var fIsActive = WAVE.strAsBool(tabInit.isActive, false);
        var fId = WAVE.strDefault(tabInit.id, fSeed + "_" + fTSeed);
        var fName = WAVE.strDefault(tabInit.name, fTSeed.toString());
        var fVisible = WAVE.strAsBool(tabInit.visible, true);
        var fClass = WAVE.strDefault(tabInit.cls);
        var fCont = WAVE.get(tabInit, "cont", null);
        var fLi = WAVE.get(tabInit, "li", null);
        var baseDisplay;

        function renderContent() {
          if(fIsHtml) 
            fCont.innerHTML = fContent;
          else
            fCont.innerText = fContent;
        }

        this.build = function(firstTime, visibilityChanged) {
          if (firstTime) {
            if (fLi === null) {
              var ulTabs = WAVE.id(fControlsIds.ulTabsId);
              ulTabs.insertAdjacentHTML("beforeend", "<li id='" + fId + "'></li>");
              fLi = WAVE.id(fId);
            }
            fLi.innerText = fTitle;
            fLi.onclick = function(){
              tabs.tabActive(fName);
            };
            if (visibilityChanged) baseDisplay = WAVE.styleOf(fLi, "display");
          }
          if (fCont === null) {
            var divId = "content_" + fId;
            var divTabs = WAVE.id(fControlsIds.divContentContId);
            divTabs.insertAdjacentHTML("beforeend", "<div id='" + divId + "' class='" + published.CLS_TABS_CONTENT_DIV + " " + fClass + "'></div>");
            fCont = WAVE.id(divId);
            renderContent();
          }
          fLi.style.display = fVisible ? baseDisplay : "none";
          WAVE.addClass(fCont, (fVisible && fIsActive) ? published.CLS_TABS_CONTENT_DIV_SHOWN : published.CLS_TABS_CONTENT_DIV_HIDDEN);
          WAVE.removeClass(fCont, (fVisible && fIsActive) ? published.CLS_TABS_CONTENT_DIV_HIDDEN : published.CLS_TABS_CONTENT_DIV_SHOWN);
        };

        this.activate = function() {
          fIsActive = true;
          WAVE.addClass(fCont, published.CLS_TABS_CONTENT_DIV_SHOWN);
          WAVE.removeClass(fCont, published.CLS_TABS_CONTENT_DIV_HIDDEN);
          WAVE.addClass(fLi, published.CLS_TABS_ACTIVE_TAB);
        };

        this.deactivate = function() {
          fIsActive = false;
          WAVE.addClass(fCont, published.CLS_TABS_CONTENT_DIV_HIDDEN);
          WAVE.removeClass(fCont, published.CLS_TABS_CONTENT_DIV_SHOWN);
          WAVE.removeClass(fLi, published.CLS_TABS_ACTIVE_TAB);
        };

        this.destroy = function() {
          return WAVE.removeElem(fCont.id) && WAVE.removeElem(fLi.id);
        };

        this.name = function() { return fName; };
        this.tab = function() { return fLi; };
        this.cont = function() { return fCont; };
        this.isActive = function(val) {
          if (typeof (val) === tUNDEFINED || val === null) return fIsActive; 

          fIsActive = val;
        };
        this.content = function(val) {
           if (typeof (val) === tUNDEFINED || val === null) return fContent;

          fContent = val;
          renderContent();
        };
        this.title = function(val) {
           if (typeof (val) === tUNDEFINED || val === null) return fTitle;

          fTitle = val;
          fLi.innerText = fTitle;
        };
        this.visible = function(val) {
          if (typeof (val) === tUNDEFINED || val === null) return fVisible;

          fVisible = val === true;
          tab.build(false, true);
          var allInvisible = true;
          for(var i = 0, l = fTabs.length; i < l; i++) {
            var t = fTabs[i];
            if (t.visible() && fTabs[0].name() !== fName) {
              if (fIsActive && !fVisible) 
                tabs.tabActive(t.name());
              allInvisible = false;
            }
          }
          if (allInvisible) {
            fDIVDisplay = WAVE.styleOf(fDIV, "display");
            fDIV.style.display = "none";
          } else {
            fDIV.style.display = fDIVDisplay;
          }
        };

        this.titleCtrl = function() { return fLi; };
        this.contentCtrl = function() { return fCont; };

        return tab;
      };

      for(var i = 0; i < fTabs.length; i++){
         fTabs[i] = new Tab(fTabs[i]);
      }

      var fControlsIds = {
        divTabsContId : "tabs_cont_" + fSeed,
        divContentContId : "content_cont_" + fSeed,
        ulTabsId : "ul_tabs_ " + fSeed
      };

      function activate(name){
        var result = null;
        for(var i = 0; i < fTabs.length; i++){
          var tab = fTabs[i];
          if(tab.name() === name){
            result = name;
            tab.activate();
          } else {
            tab.deactivate();
          }
        }
        return result;
      }

      function getTabByName(name) {
        for(var i = 0; i < fTabs.length; i++) {
          if (fTabs[i].name() === name) return fTabs[i];
        }
        return null;
      }

      this.DIV = function() { return fDIV; };

      this.tabs = function() { return fTabs; };

      this.unswitchable = function(value) {
        if (typeof(value) === tUNDEFINED) return fUnswitchable;
        
        fUnswitchable = value === true;
        for(var i = 0; i < fTabs.length; i++){
          var tab = fTabs[i];
          if(fUnswitchable){
            if(!fTabs[i].isActive())
              tab.tab().className += " " + published.CLS_TABS_LI_DISABLED;
          }
          else 
            WAVE.removeClass(tab.tab(), published.CLS_TABS_LI_DISABLED);
        }
        return value;
      };

      this.tabByName = function(name) {
        if(typeof(name) === tUNDEFINED || name === null) return null;

        return getTabByName(name);
      };

      this.tabAdd = function(tabInit){
        var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, tabControl: tabs, tabInit: tabInit, abort: false };
        tabs.eventInvoke(published.EVT_TABS_TAB_ADD, evtArgsBefore);
        if (evtArgsBefore.abort === true) return;

        var tab = new Tab(tabInit);
        var ft = getTabByName(tab.name());
        if (ft !== null) {
          ft.destroy();
          fTabs.splice(fTabs.indexOf(ft), 1);
        }
        fTabs.push(tab);
        tab.build(true, false);
        if (tab.isActive())
          activate(tab.name());

        tabs.eventInvoke(published.EVT_TABS_TAB_ADD, { phase: published.EVT_PHASE_AFTER, tabControl: tabs, tab: tab });

        return tab;
      };

      this.tabDelete = function(name){
        if(typeof(name) === tUNDEFINED || name === null) return false;

        var res = false;
        for(var i = 0; i < fTabs.length; i++){
          if(fTabs[i].name() === name){
            var tab = fTabs[i];

            var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, tabControl: tabs, tab: tab, abort: false };
            tabs.eventInvoke(published.EVT_TABS_TAB_REMOVE, evtArgsBefore);
            if (evtArgsBefore.abort === true) return res;

            res = tab.destroy();
            fTabs.splice(i, 1);
            if(tab.isActive() && fTabs.length > 0){
              activate(fTabs[0].name());
            }
            break;
          }
        }

        tabs.eventInvoke(published.EVT_TABS_TAB_REMOVE, { phase: published.EVT_PHASE_AFTER, tabControl: tabs, name: name, ok: res });
        return res;
      };

      this.tabActive = function(name){
        var result = null;
        if(typeof(name) === tUNDEFINED || name === null) {
          for(var i = 0; i < fTabs.length; i++) {
            if(fTabs[i].isActive()) {
              result = fTabs[i].name();
              break;
            }
          }
        } else {
          if(!fUnswitchable)
            if(tabs.tabActive() !== name) {
              result = activate(name);
              tabs.eventInvoke(published.EVT_TABS_TAB_CHANGED, name);
            }
        }
        return result;
      };

      this.tabContent = function(name, value) {
        if (typeof(name) === tUNDEFINED || name === null || WAVE.strEmpty(name)) return null;
        var tab = getTabByName(name);
        if (tab === null) return null;

        if (typeof(value) === tUNDEFINED) {
          return tab.content();
        }
        tab.content(value);
        return value;
      };  

      this.tabTitle = function(name, value) {
        if (typeof(name) === tUNDEFINED || name === null || WAVE.strEmpty(name)) return null;
        var tab = getTabByName(name);
        if (tab === null) return null;

        if (typeof(value) === tUNDEFINED) {
          return tab.title();
        }
        tab.title(value);
        return value;
      };

      this.tabVisible = function(name, val) {
        if (typeof(name) === tUNDEFINED || name === null || WAVE.strEmpty(name)) return null;
        var tab = getTabByName(name);

        if (tab === null) return null;
        if (typeof(val) === tUNDEFINED) {
          return tab.visible();
        }

        val = val === true; 
        tab.visible(val);
        return val;
      };

      function build() {
        var i, l, tab;

        if (fParse) {
          fTabs = [];
            var tabs = fDIV.getElementsByTagName("li");
            for(i = 0, l = tabs.length; i<l; i++) {
              var t = tabs[i];
              tab = {};
              tab.title = t.innerText;
              tab.name = WAVE.strDefault(t.getAttribute(fAttributePrefix + "name"));
              if (WAVE.strEmpty(tab.name)) tab.name = null;
              tab.isHtml = WAVE.strAsBool(t.getAttribute(fAttributePrefix + "isHtml"), true);
              tab.isActive = WAVE.strAsBool(t.getAttribute(fAttributePrefix + "isActive"), false);
              tab.visible = WAVE.strAsBool(t.getAttribute(fAttributePrefix + "visible"), true);
              tab.cont = WAVE.id(WAVE.strDefault(t.getAttribute(fAttributePrefix + "contId")));
              tab.li = t;
              fTabs.push(new Tab(tab));
            }
        } else {
          var html = 
            "<div id='@divTabsId@' class='@ulContainer@'>" +
              "<ul id='@ulId@' class='@ulClass@'></ul>" +
            "</div>" +
            "<div id='@divContId@' class='@divClass@'></div>";

          fDIV.innerHTML = WAVE.strHTMLTemplate(html, 
          {
            divTabsId : fControlsIds.divTabsContId,
            divContId : fControlsIds.divContentContId,
            ulId : fControlsIds.ulTabsId,
            ulClass : published.CLS_TABS_UL,
            divClass : published.CLS_TABS_CONTENT_CONTAINER,
            ulContainer : published.CLS_TABS_UL_CONTAINER
          });
        }

        if(fTabs.length === 0) return;

        var activeTab = null;
        var hit = false;
        for(i = 0; i < fTabs.length; i++) {
          tab = fTabs[i];
          if(tab.isActive() && tab.visible()){
            if(!hit){
              activeTab = tab.name();
              hit = true;
            } else {
              tab.isActive (false);
            }
          }
          tab.build(true, false);
        }
        if(!hit){
          i = 0;
          while(i < fTabs.length){
            if (fTabs[i].visible()) {
              activeTab = fTabs[i].name();
              fTabs[i].isActive(true);
              break;
            }
            i++;
          }
        }
        if (activeTab !== null)
          activate(activeTab);
      }
      build();

      return tabs;
    };//Tabs

  /*
    init : {
      DIV: control|Id
      title: text|html
      altTitle: text|html
      titleCtrl: control|id
      content: text|html
      contentCtrl: control|id
      isTitleHtml: bool
      isContentHtml: bool
      mode: swap|modal
      showOnClick: bool
      showOnFocus: bool
      hideOnClick: bool
      hideOnFocus: bool
      hideOnTimer: bool
      timeout: int
    }
  */
    var fDetailsIdSeed = 0;
    published.Details = function (init) {
      if (typeof(init) === tUNDEFINED || init === null) throw "Details.ctor(init)";

      var details = this;
      WAVE.extend(details, WAVE.EventManager);

      var fId = "wvDetails_" + fDetailsIdSeed++;
      var fDiv;
      var fTitleControl;
      var fContentControl;
      var fTitle = WAVE.strDefault(init.title);
      var fAltTitle = WAVE.strDefault(init.altTitle, fTitle);
      var fContent = WAVE.strDefault(init.content);
      var fIsTitleHtml = WAVE.strAsBool(init.isTitleHtml, true);
      var fIsContentHtml = WAVE.strAsBool(init.isContentHtml, true);

      var fShowOnClick = WAVE.strAsBool(init.showOnClick, true);
      var fShowOnFocus = WAVE.strAsBool(init.showOnFocus, false);
      var fHideOnTitle = WAVE.strAsBool(init.hideOnTitle, true);
      var fHideOnClick = WAVE.strAsBool(init.hideOnClick, true);
      var fHideOnFocus = WAVE.strAsBool(init.hideOnFocus, false);
      var fHideOnTimer = WAVE.strAsBool(init.hideOnTimer, false);

      var fMode = WAVE.strDefault(init.mode, "swap");
      var r = WAVE.tryParseInt(init.timeout);
      var fTimeout = r.ok ? r.value : 0;
      var titleId = fId + "_title";
      var contId = fId + "_content";
      var fDialog = null;
      var fHiddenState = false;

      if (WAVE.has(init, 'DIV'))
        fDiv = WAVE.isString(init.DIV) ? WAVE.id(init.DIV) : init.DIV;
      if (WAVE.has(init, 'titleCtrl'))
        fTitleControl = WAVE.isString(init.titleCtrl) ? WAVE.id(init.titleCtrl) : init.titleCtrl;
      if (WAVE.has(init, 'contentCtrl'))
        fContentControl = WAVE.isString(init.contentCtrl) ? WAVE.id(init.contentCtrl) : init.contentCtrl;

      function hideContent() {
        if (fHideOnTimer) {
          setTimeout(function() {
            __hide();
          }, fTimeout);
        }
        if (fHideOnFocus) {
          window.addEventListener("focus", hideEventHandler, true);
          setTimeout(function(){ window.addEventListener("mousemove", moveEventHandler, false); } , 1);
        } 
        if (fHideOnClick)
          setTimeout(function(){ window.addEventListener("click", hideEventHandler, false); }, 1);
      }

      function hideEventHandler(e) {
        var target = e.target;
        if (!WAVE.isParentOf(fContentControl, target)) __hide();
      }

      function moveEventHandler(e) {
        var target = e.target;
        if (!WAVE.isParentOf(fContentControl, target) && !WAVE.isParentOf(fTitleControl, target)) __hide();
      }

      function build()
      {
        if(typeof(fDiv) !== tUNDEFINED && fDiv !== null) {
          WAVE.addClass(fDiv, published.CLS_DETAILS);
          var html;

          if (fMode === "modal") {
            html = "<span id='@titleId@' class='@titleClass@' tabindex='0'></span>";
            fDiv.innerHTML = WAVE.strHTMLTemplate(html, {
              titleId: titleId,
              titleClass: published.CLS_DETAILS_TITLE
            });
            fTitleControl = WAVE.id(titleId);

            if (fIsTitleHtml) 
              fTitleControl.innerHTML = fTitle;
            else
              fTitleControl.innerText = fTitle;

          } else {

            html = "<span id='@titleId@' class='@titleClass@' tabindex='0'></span><div id='@contId@' class='@contClass@'></div>";
            fDiv.innerHTML = WAVE.strHTMLTemplate(html, {
              titleId: titleId,
              titleClass: published.CLS_DETAILS_TITLE,
              contId: contId,
              contClass: published.CLS_DETAILS_CONTENT
            });
            fTitleControl = WAVE.id(titleId);
            fContentControl = WAVE.id(contId);


            if (fIsContentHtml)
              fContentControl.innerHTML = fContent;
            else
              fContentControl.innerText = fContent;

          }
        } else if (fTitleControl !== null && fContentControl !== null) {
          WAVE.addClass(fTitleControl, published.CLS_DETAILS_TITLE);
          WAVE.addClass(fContentControl, published.CLS_DETAILS_CONTENT);
        } else 
          throw "Details.build(unacceptable init)";

        setTitle(fTitle);

        if (fShowOnClick) 
          fTitleControl.addEventListener("click", __show, false);
        if (fShowOnFocus)
          fTitleControl.onmouseover = fTitleControl.onfocus = __show;

        __hide();
      }

      function setTitle(cont) {
        if(WAVE.strEmpty(fTitle)) return;

        if (fIsTitleHtml) 
          fTitleControl.innerHTML = cont;
        else
          fTitleControl.innerText = cont;
      }

      function __hide() {
        if(fHiddenState) return;

        var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, abort: false };
        details.eventInvoke(published.EVT_DETAILS_HIDE, evtArgsBefore);
        if (evtArgsBefore.abort === true) return;

        setTitle(fTitle);
        if (fShowOnClick)
          fTitleControl.addEventListener("click", __show, false);
        if (fHideOnTitle)
          fTitleControl.removeEventListener("click", __hide, false);

        if (fMode === "modal" && fDialog !== null) fDialog.close();
        else {
          WAVE.addClass(fContentControl, published.CLS_DETAILS_CONTENT_HIDDEN);
          WAVE.removeClass(fContentControl, published.CLS_DETAILS_CONTENT_VISIBLE);
        }
        fHiddenState = true;
        window.removeEventListener("focus", hideEventHandler, false);
        window.removeEventListener("click", hideEventHandler, false);
        window.removeEventListener("mousemove", moveEventHandler, false);

        WAVE.addClass(fTitleControl, published.CLS_DETAILS_TITLE_HIDDEN);
        WAVE.removeClass(fTitleControl, published.CLS_DETAILS_TITLE_SHOWN);

        details.eventInvoke(published.EVT_DETAILS_HIDE, { phase: published.EVT_PHASE_AFTE });
      }

      function __show() {
        if (!fHiddenState) return;

        var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, abort: false };
        details.eventInvoke(published.EVT_DETAILS_SHOW, evtArgsBefore);
        if (evtArgsBefore.abort === true) return;

        setTitle(fAltTitle);
        if (fShowOnClick)
          fTitleControl.removeEventListener("click", __show, false);
        if (fHideOnTitle)
          fTitleControl.addEventListener("click", __hide, false);

        if (fMode === "modal") {
          fDialog = new published.Dialog({
            cls: published.CLS_DETAILS_CONTENT + " " + contId,
            content: fContent, 
            onShow: hideContent
          });
          fContentControl = document.getElementsByClassName(contId)[0];
        } else {//"swap" - default
          WAVE.removeClass(fContentControl, published.CLS_DETAILS_CONTENT_HIDDEN);
          WAVE.addClass(fContentControl, published.CLS_DETAILS_CONTENT_VISIBLE);
          hideContent();
        }
        fHiddenState = false;
        WAVE.addClass(fTitleControl, published.CLS_DETAILS_TITLE_SHOWN);
        WAVE.removeClass(fTitleControl, published.CLS_DETAILS_TITLE_HIDDEN);

        details.eventInvoke(published.EVT_DETAILS_SHOW, { phase: published.EVT_PHASE_AFTER });
      }

      this.hide = function() {
        __hide();
      };

      this.show = function() {
        __show();
      };

      this.toggle = function() {
        if(fHiddenState) __show();
        else __hide();
      };

      build();

      return details;
    };//Details

    function equalsImage(left, right) {
      if (left === right) return true;
      if (typeof(left) === tUNDEFINED || typeof(right) === tUNDEFINED) return false;
      if (!WAVE.isObject(left)) left = { url: left };
      if (!WAVE.isObject(right)) right = { url: right };
      if (left.url === right.url) {
        if (WAVE.has(left, 'clip')) {
          if (WAVE.has(right, 'clip')) {
            return left.left|0 === right.left|0
              && left.right|0  === right.right|0
              && left.bottom|0 === right.bottom|0
              && left.top|0    === right.top|0;
          }
        } else {
          if (!WAVE.has(right, 'clip')) return true;
        }
      }
      return false;
    }

    function loadImage(image, onload, onerror, urlMap) {
      if (!WAVE.isObject(image)) image = { url: image };
      image.url = WAVE.strDefault(ensureUrl(WAVE.strDefault(image.url)));
      var img = new Image();
      img.onerror = function () {
        image.error = true;
        if (typeof(onerror) !== tUNDEFINED)
          onerror.apply(img, arguments);          
      };
      if (WAVE.has(image, 'clip')) {
        var clip = image.clip;
        var left =   clip.left|0;
        var right  = clip.right|0;
        var bottom = clip.bottom|0;
        var top    = clip.top|0;
        var 
          clipWidth = right - left,
          clipHeight = bottom - top;
        var canvas = document.createElement('canvas');
        canvas.width = clipWidth;
        canvas.height = clipHeight;
        img.onload = function () {
          image.load = true;
          image.width = clipWidth;
          image.height = clipHeight;
          var ctx = canvas.getContext('2d');
          ctx.drawImage(this, left, top, clipWidth, clipHeight, 0, 0, clipWidth, clipHeight);
          if (typeof(onload) !== tUNDEFINED)
            onload.apply(canvas, arguments);
          delete image.img;
        };
        image.element = canvas;
      } else {
        img.onload = function () {
          image.load = true;
          image.width = this.width;
          image.height = this.height;
          var that = this;
          if (typeof(onload) !== tUNDEFINED)
            onload.apply(img, arguments);
        };
        image.element = img;
      }
      img.src = image.url;
      image.free = free;
      
      return image;

      function free() {
        if (image.element.parentNode)
          image.element.parentNode.removeChild(image.element);
        delete image.element;
        delete image.error;
        delete image.load;
        delete image.img;
        delete image.width;
        delete image.height;
        delete image.free;
      }
      function ensureUrl(url) { return url.indexOf('$') === 0 && WAVE.isObject(urlMap) ? urlMap[url.substr(1)] : url; }
    }

    function makeDraggable(element, ondrag, onmove, ondrop, preventDefault) {
      element.addEventListener('mousedown', mousedown, false);
      element.addEventListener('touchstart', touchstart, false);

      function mousedown(event) {
        if (event.button === 0) {
          if (preventDefault) event.preventDefault();
          dragstart(event.pageX, event.pageY);
        }
      }
      function touchstart(event) {
        var touches = event.touches;
        if (touches.length === 1) {
          if (preventDefault) event.preventDefault();
          dragstart(touches[0].pageX, touches[0].pageY);
        }
      }
      function dragstart(x, y) {
        if (typeof(ondrag) !== tUNDEFINED)
          ondrag.call(element, x, y);
        document.addEventListener('mousemove', mousemove, false);
        document.addEventListener('touchmove', touchmove, false);
        document.addEventListener('mouseup', mouseup, false);
        document.addEventListener('touchend', touchend, false);
        function mousemove(event) {
          event.preventDefault();
          var 
            pageX = event.pageX,
            pageY = event.pageY;
          dragmove(x, y, x - pageX, y - pageY);
          x = pageX; y = pageY;
        }
        function touchmove(event) {
          event.preventDefault();
          var touches = event.touches;
          var 
            pageX = touches[0].pageX,
            pageY = touches[0].pageY;
          dragmove(x, y, x - pageX, y - pageY);
          x = pageX; y = pageY;
        }
        function mouseup(event) {
          dragend(event.pageX, event.pageY);
          cleanup();
        }
        function touchend(event) {
          dragend(x, y);
          cleanup();
        }
        function cleanup() {
          document.removeEventListener('mousemove', mousemove, false);
          document.removeEventListener('touchmove', touchmove, false);
          document.removeEventListener('mouseup',   mouseup,   false);
          document.removeEventListener('touchend',  touchend,  false);            
        }
      }
      function dragmove(x, y, offsetX, offsetY) {
        if (typeof(onmove) !== tUNDEFINED)
          onmove.call(element, x, y);
        element.scrollLeft += offsetX;
        element.scrollTop += offsetY;
      }
      function dragend(x, y) {
        if (typeof(ondrop) !== tUNDEFINED)
          ondrop.call(element, x, y);
      }
    }

    var fImageViewIdSeed = 0;
    published.ImageView = function (init, urlMap) {
      if (typeof(init) === tUNDEFINED || init === null) init = {};
      if (WAVE.isString(init)) init = { DIV: init };
      if (!WAVE.has(init, 'DIV')) throw "ImageView.ctor(init.thumbnails)";

      var fId = published.CLS_IMAGE_VIEW + fImageViewIdSeed++;

      var imageView = this;
      WAVE.extend(imageView, WAVE.EventManager);

      var fViewbox = build(WAVE.isString(init.DIV) ? WAVE.id(init.DIV) : init.DIV);

      var fImage;
      imageView.image = function (image) { 
        if (typeof(image) !== tUNDEFINED)  {
          fImage = updateImage(image);
        }
        return fImage;
      };
      imageView.clear = function (flag) {
        if (flag)
          fViewbox.innerHTML = '';
        return fViewbox.innerHTML === '';
      };

      imageView.image(init.image);
      
      window.addEventListener('resize', function() { update(imageView.image() || {}); }, false);

      return imageView;
      
      function build(parent) {
        var viewbox = document.createElement('div');
        viewbox.className = published.CLS_IMAGE_VIEW;
        parent.appendChild(viewbox);
        makeDraggable(viewbox, function (x, y) {
          var 
            image = imageView.image(),
            offset = position(viewbox),
            box = size(viewbox),
            client = { x: x - offset.x - box.x / 2, y: y - offset.y - box.y / 2 },
            sz = size(fImage.element), 
            newClient = { 
              x: client.x * fImage.width / sz.x, 
              y: client.y * fImage.height / sz.y 
            };
          if (fImage.width > sz.x || fImage.height > sz.y) {
            viewbox.className += ' ' + published.CLS_IMAGE_VIEW_ZOOM;          
            var scr = scroll(viewbox);
            viewbox.scrollLeft = newClient.x + scr.x / 2 - (x - offset.x);
            viewbox.scrollTop = newClient.y + scr.y / 2 - (y - offset.y);
          }
        }, undefined, update, true);
        return viewbox;
      }

      function updateImage(image) {
        if (typeof(image) === tUNDEFINED) return;
        imageView.clear(true);
        if (image === null) return;

        if (WAVE.isObject(image) && WAVE.has(image, 'element')) {
          if (!image.error) fViewbox.appendChild(image.element);
          update(image);
        } else {
          image = loadImage(image, function () {
            this.className = published.CLS_IMAGE_VIEW_IMAGE;
            if (imageView.clear())
              fViewbox.appendChild(this);
            update(image);
          }, function () {
            update(image);
          }, urlMap);
        }
        return image;
      }

      function update(image) {
        if (typeof(fViewbox) === tUNDEFINED) return;
        fViewbox.className = published.CLS_IMAGE_VIEW;
        if (image.error) fViewbox.className += ' ' + published.CLS_IMAGE_BOX_ERROR;
        else if (!image.load) fViewbox.className += ' ' + published.CLS_IMAGE_BOX_LOADING; 
        else {
          var 
            sz = size(fViewbox), aspect = sz.x / sz.y,
            imageAspect = (image.width|0) / (image.height|0);
          if (!isNaN(imageAspect))
            fViewbox.className += ' ' + ((imageAspect >= aspect) ? published.CLS_IMAGE_BOX_WIDER : published.CLS_IMAGE_BOX_HIGHER);          
        }
      }

      function position(elm) {
        var ox=0, oy=0;
        while (elm !== null)
          ox += elm.offsetLeft, oy += elm.offsetTop, elm = elm.offsetParent;
        return { x: ox, y: oy };
      }
      function size(elm) { return { x: elm.offsetWidth, y: elm.offsetHeight }; }
      function scroll(elm) { return { x: elm.scrollWidth, y: elm.scrollHeight }; }
    };//ImageView

    /*
    Image = {
      url: $URL_NAME | URL,
      clip: { top: NUMBER, bottom: NUMBER, left: NUMBER, right: NUMBER }
    };
    Galery = {
      thumbnails: DIV,
      preview: DIV,
      urls: {
        name: URL
      },
      images: [
        {
          id: TEXT,
          image: Image | $URL_NAME | URL,
          thumb: Image | URL,
          title: TEXT,
          content: TEXT | HTML,
          isText: BOOL
        } | Image | URL
      ]
    } | DIV_ID;
    */
    var fGalleryIdSeed = 0;
    published.Gallery = function (init) {
      if (typeof(init) === tUNDEFINED || init === null) init = {};
      if (WAVE.isString(init)) init = { thumbnails: init };
      if (!WAVE.has(init, 'thumbnails')) throw "Gallery.ctor(init.thumbnails)";
      var hasUrls = WAVE.has(init, 'urls');
      if (hasUrls && !WAVE.isObject(init.urls)) throw "Gallery.ctor(init.urls)";

      var fId = fGalleryIdSeed++;

      var gallery = this;
      WAVE.extend(gallery, WAVE.EventManager);

      gallery.urls = (hasUrls ? init.urls : {});

      var fUpdate;
      var fThumbnails = build(WAVE.isString(init.thumbnails) ? WAVE.id(init.thumbnails) : init.thumbnails);
      var fPreview = WAVE.isString(init.preview) ? new published.ImageView(init.preview, gallery.urls) : init.preview;
      
      var fCurrent;
      var fItems = [];
      var fItemsLookup = {};

      var fItemIdSeed = 0;
      gallery.Item = function (init, gallery, view) {
        var seqId = fItemIdSeed++;

        var fId = WAVE.isObject(init) && WAVE.has(init, 'id') ? init.id : seqId;
        init.id = fId;
        var fInit = {};
        
        var item = this;
        WAVE.extend(item, WAVE.EventManager);

        var fGallery = gallery, fView = view;
        var fImage, fThumbImage;

        var
          fElem = document.createElement('li'),
          fThumb = document.createElement('div'),
          fContent = document.createElement('div');
        fElem.className = published.CLS_GALLERY_ITEM;
        fContent.className = published.CLS_GALLERY_CONTENT;
        fThumb.className = published.CLS_GALLERY_THUMBNAIL;
        fElem.appendChild(fThumb);
        fElem.appendChild(fContent);
        fElem.onclick = function() { 
          gallery.current(fId);
        };

        item.id        = function () { return fId; };
        item.title     = function (title) { 
          if (typeof(title) !== tUNDEFINED 
            && fInit.title !== title) 
            updateTitle(fInit.title = title);
          return WAVE.strDefault(fInit.title, fId); 
        };
        item.content   = function (content, isText) { 
          if (typeof(content) !== tUNDEFINED 
            && (fInit.content !== content
              || !!fInit.isText !== !!isText)) 
            updateContent(fInit.content = content, fInit.isText = !!isText);
          return WAVE.strDefault(fInit.content); 
        };
        item.isText    = function () { return !!fInit.isText; };
        item.image     = function (image) { 
          if (typeof(image) !== tUNDEFINED 
            && !equalsImage(fInit.image, image)) 
            updateImage(fInit.image = image);
          return fImage; 
        };
        item.thumb     = function (image) { 
          if (typeof(image) !== tUNDEFINED
            && !equalsImage(fInit.thumb, image)) 
            updateThumbnail(fInit.thumb = image);
          return fThumbImage; 
        };
        item.data      = function (data) {
          if (typeof(data) !== tUNDEFINED) {
            fInit.data = data;
          }
          return fInit.data;
        };
        item.show      = function () { fImage = fView.image(fImage); };
        item.update    = update;
        item.free      = function () {
          if (fElem.parentNode)
            fElem.parentNode.removeChild(fElem);
          if (fView.image() === fImage && fImage.element.parentNode)
            fImage.element.parentNode.removeChild(fImage.element);
        };
        item.element   = function () { return fElem; };

        item.update(init);

        return item;

        function update(init) {
          if (!WAVE.isObject(init) || WAVE.has(init, "url"))  init = { image: init };
          if (!WAVE.has(init, 'image')) throw "Gallery.Item.update(init.image)";
          if (init.id !== fId) throw "Gallery.Item.update(init.id)";
          item.image(init.image);
          item.thumb(init.thumb || init.image);
          item.title(init.title);
          item.content(init.content, init.isText);
          item.data(init.data);
        }

        function updateImage(image) {
          var selected = false;
          if (typeof(fImage) !== tUNDEFINED) {
            selected = fImage === view.image();
            if (WAVE.has(fImage, 'free')) fImage.free();
          }
          fImage = image;
          if (selected) item.show();
        }

        function updateThumbnail(image) {
          fThumb.innerHTML = '';          
          fThumb.className = published.CLS_GALLERY_THUMBNAIL + ' ' + published.CLS_IMAGE_BOX_LOADING;
          fThumbImage = loadImage(image, function() {
            this.className = published.CLS_GALLERY_IMAGE;
            this.id = published.CLS_GALLERY_IMAGE + item.id();
            fThumb.appendChild(this);
            fThumb.className = published.CLS_GALLERY_THUMBNAIL + ' ' 
              + ((this.width >= this.height) ? published.CLS_IMAGE_BOX_WIDER : published.CLS_IMAGE_BOX_HIGHER);
          }, function () {
            fThumb.className = published.CLS_GALLERY_THUMBNAIL + ' ' + published.CLS_IMAGE_BOX_ERROR;
          }, gallery.urls);
          fUpdate();
        }

        function updateContent(content, isText) {
          if (isText) fContent.innerText = content;
          else fContent.innerHTML = content;
        }

        function updateTitle(title) { fThumb.title = title; }
      };

      var fUpdateCurrentWithEvent = withEvent(gallery, published.EVT_GALLERY_CHANGE, updateCurrent);

      gallery.current = function (id) {
        if (typeof(id) !== tUNDEFINED) {
          var current = fUpdateCurrentWithEvent(id);
          if (typeof(current) !== tUNDEFINED) fCurrent = current;
        }
        if (typeof(fCurrent) !== tUNDEFINED) return fCurrent.id();
      };
      gallery.has = has;
      gallery.get = get;
      var fRemoveWithEvent = withEvent(gallery, published.EVT_GALLERY_REMOVE, remove);
      gallery.remove = function (id) {
        if (!has(id)) return true;
        return !!fRemoveWithEvent(id);
      };
      gallery.images = function () { return fItems; };
      gallery.update = withEvent(gallery, published.EVT_GALLERY_ADD, updateItem);

      WAVE.each(init.images, updateItem);

      fUpdate();

      return gallery;

      function build(parent) {
        var 
          thumbnails = document.createElement('ul'),
          prev = document.createElement('div'),
          next = document.createElement('div');
        thumbnails.className = published.CLS_GALLERY_LIST;
        prev.className = published.CLS_GALLERY_PREV;
        next.className = published.CLS_GALLERY_NEXT;
        prev.onclick = function (e) {
          e.preventDefault();
          var sz = size(thumbnails.firstChild);
          scroll(thumbnails, { x: -sz.x, y: -sz.y}, 400, undefined, update);
        };
        next.onclick = function (e) {
          e.preventDefault();
          var sz = size(thumbnails.firstChild);
          scroll(thumbnails, sz, 400, undefined, update);
        };
        makeDraggable(thumbnails, undefined, update, update);      
        parent.appendChild(prev);
        parent.appendChild(thumbnails);
        parent.appendChild(next);
        window.addEventListener('resize', update, false);
        fUpdate = update;
        return thumbnails;
        function update() {
          prev.className = published.CLS_GALLERY_PREV 
            + ((fThumbnails.scrollLeft === 0 && fThumbnails.scrollTop === 0) ? ' ' + published.CLS_GALLERY_DISABLED : '');
          next.className = published.CLS_GALLERY_NEXT 
            + ((fThumbnails.scrollLeft + fThumbnails.offsetWidth === fThumbnails.scrollWidth 
              && fThumbnails.scrollTop + fThumbnails.offsetHeight === fThumbnails.scrollHeight) ? ' ' + published.CLS_GALLERY_DISABLED : '');
        }
      }

      function updateItem(item) {
        var it;
        if (has(item.id)) {
          it = get(item.id);
          it.update(item);
        } else {
          it = new gallery.Item(item, gallery, fPreview);
          it.order = item.order|0;
          var idx = insertAt(fItems, it, orderComp);
          fThumbnails.insertBefore(it.element(), WAVE.has(fItems, idx) ? fItems[idx].element() : null);
          fItems.splice(idx, 0, it);
          fItemsLookup[it.id()] = it;
        }
        return it;
      }

      function has(id) { 
        if (typeof(id) === tUNDEFINED) return false;
        return WAVE.has(fItemsLookup, id); 
      }

      function get(id) { return fItemsLookup[id]; }

      function updateCurrent(id) {
        if (!has(id)) return; 
        var item = get(id);

        if (typeof(fCurrent) !== tUNDEFINED)
          fCurrent.element().className = published.CLS_GALLERY_ITEM;
        item.element().className = published.CLS_GALLERY_ITEM + ' ' + published.CLS_GALLERY_SELECTED;
        item.show();
        return item;
      }

      function remove(id) {
        if (!has(id)) return true;  
        get(id).free();
        fItems = fItems.filter(function (it) { return it.id() !== id; });
        delete fItemsLookup[id];
        return true;
      }

      function insertAt(sorted, val, comp) {
        var low = 0, hig = sorted.length;
        var mid = -1, c = 0;
        while(low < hig) {
          mid = ((low + hig)/2) | 0;
          c = comp(sorted[mid], val);
          if (c < 0) low = mid + 1;
          else if ( c > 0) hig = mid;
          else  {
            while (mid < hig && comp(sorted[mid], val) === 0) mid++;
            return mid;
          }
        }
        return low;
      }

      function orderComp(a, b) { return a.order - b.order; }

      function size(elm) { return { x: elm.offsetWidth, y: elm.offsetHeight }; }
      function animate(opts) {
        var start = Date.now();
        var id = setInterval(function () {
          var progress = (Date.now() - start) / (+opts.duration || 1000);
          if (progress > 1) progress = 1;
          var cont = opts.step((opts.delta || def)(progress)) || true;
          if (progress === 1 || !cont) clearInterval(id);
        }, +opts.delay || 10);
        function def(progress) { return progress; }
      }
      function scroll(elm, to, duration, delta, onstep) {
        var scr = { x: elm.scrollLeft, y: elm.scrollTop };
        animate({ 
          duration: duration,
          delta: delta,
          step: function (delta) {
            elm.scrollLeft = scr.x + to.x * delta;
            elm.scrollTop = scr.y + to.y * delta;
            if (onstep)
              onstep.call(elm, delta);
        } });
      }

      function withEvent(that, evt, func) {
        return function () {
          var evtArgs = { evt: evt, phase: published.EVT_PHASE_BEFORE, args: arguments, abort: false };
          that.eventInvoke(evt, evtArgs);
          if (evtArgs.abort === true) return;

          var result = func.apply(that, arguments);

          evtArgs.result = result;
          evtArgs.phase = published.EVT_PHASE_AFTER;
          delete evtArgs.abort;
          that.eventInvoke(evt, evtArgs);
          return result;          
        };
      }
    };//Gallery

    published.GallerySimple = function (init) {
      if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "GallerySimple.ctor(init.DIV)";      
      var fDIV = WAVE.isString(init.DIV) ? WAVE.id(init.DIV) : init.DIV;
      var preview = document.createElement('div');
      preview.className = published.CLS_GALLERY_SIMPLE_PREVIEW;
      init.preview = new published.ImageView({ DIV: preview }, init.urls);
      init.thumbnails = document.createElement('div');
      init.thumbnails.className = published.CLS_GALLERY_SIMPLE_THUMBNAILS;
      fDIV.appendChild(init.thumbnails);
      fDIV.appendChild(preview);
      return published.Gallery.call(this, init);
    };//GallerySimple

    return published;
}());//WAVE.GUI



WAVE.RecordModel.GUI = (function(){

    var tUNDEFINED = "undefined";

    var published = { 
        CLS_ERROR: 'wvError',
        CLS_REQ: 'wvRequired',
        CLS_MOD: 'wvModified',
        CLS_PUZZLE: 'wvPuzzle',
        CLS_COMBO_ARROW: 'wvComboArrow',
        CLS_COMBO_WRAP: 'wvComboWrap'
    };

        var AIKEY = "__wv.rm.gui";
        var fErroredInput = null;
        
        function buildTestControl(fldView){
          fldView.DIV().innerHTML = WAVE.strHTMLTemplate("<div>@about@</div><input type='text' name='@fName@' value='@fValue@'/>",
                                                        {about: fldView.field().about(), fName: fldView.field().name(), fValue: fldView.field().value()});
        }

        function genIDSeed(fldView){
          var id = WAVE.genAutoincKey(AIKEY);
          return "_"+fldView.recView().ID()+"_"+id;
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
}());//WAVE.RecordModel.GUI