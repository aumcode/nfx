//Dialog window:
//   {header: 'html markup', body: 'html markup', footer: 'html markup', cls: 'className', widthpct: 75, heightpct: 50, onShow: function(dlg){}, onClose: function(dlg, result){return true;}}
published.Dialog = function(init)
{
  if (!WAVE.isObject(init)) init = {};

  var dialog = this,
      fOnShow = WAVE.isFunction(init.onShow) ? init.onShow : function(){},
      fOnClose = WAVE.isFunction(init.onClose) ? init.onClose : function(){ return true;},
      fCloseOnClickOutside = WAVE.strAsBool(init.closeOnClickOutside, false),
      fCloseOnEscape = WAVE.strAsBool(init.closeOnEscape, true),
      fWidthPct = WAVE.intValidPositive(init.widthpct) ? init.widthpct : 0,
      fHeightPct = WAVE.intValidPositive(init.heightpct) ? init.heightpct : 0,
      fPreventScroll = WAVE.strAsBool(init.preventScroll, true),
      headCtn = init.header,
      bodyCtn = init.body,
      footerCtn = init.footer,
      fdivBase = null,
      fdivContent = null,
      fdivHeader = null,
      fdivBody = null,
      fdivFooter = null;

  WAVE.extend(dialog, WAVE.EventManager);

  if (fWidthPct>100) fWidthPct = 100;
  if (fHeightPct>100) fHeightPct = 100;

  function appendContent(el, ctn, dfl) {
    if (WAVE.isObject(ctn)) el.appendChild(ctn);
    else el.innerHTML = WAVE.strDefault(ctn, dfl);
  }

  fdivBase = document.createElement("div");
  fdivBase.__DIALOG = this;
  fdivBase.className = published.CLS_DIALOG_BASE + ' ' + WAVE.strDefault(init['cls']);
  fdivBase.style.position = "fixed";
  fdivBase.style.zIndex = CURTAIN_ZORDER + fCurtains.length;

  document.body.appendChild(fdivBase);

  fCurtains.push(fdivBase);

  fdivContent = document.createElement("div");
  fdivContent.className = published.CLS_DIALOG_CONTENT;
  fdivBase.appendChild(fdivContent);

  fdivHeader = document.createElement("div");
  fdivHeader.className = published.CLS_DIALOG_HEADER;
  appendContent(fdivHeader, headCtn, "Dialog");
  fdivContent.appendChild(fdivHeader);

  fdivBody = document.createElement("div");
  fdivBody.className = published.CLS_DIALOG_BODY;
  appendContent(fdivBody, bodyCtn, "&nbsp;");
  fdivContent.appendChild(fdivBody);

  fdivFooter = document.createElement("div");
  fdivFooter.className = published.CLS_DIALOG_FOOTER;
  appendContent(fdivFooter, footerCtn, "");
  fdivContent.appendChild(fdivFooter);

  function __adjustBounds(){
    if (fWidthPct === 0 && fHeightPct === 0)
      return;

    var sw = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
    var sh = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;
    var cx = sw / 2;
    var cy = sh / 2;

    var w = fWidthPct === 0 ? fdivContent.offsetWidth : Math.round( sw * (fWidthPct/100));
    var h = fHeightPct === 0 ? fdivContent.offsetHeight : Math.round( sh * (fHeightPct/100));
    fdivContent.style.width  = fWidthPct === 0  ? "auto" : w + "px";

    fdivContent.style.left = Math.round(cx - (fdivContent.offsetWidth / 2)) + "px";
    var top = Math.round(cy - (fdivContent.offsetHeight / 2));
    if (top < 0) top = 0;

    fdivContent.style.top = top + "px";

    fdivContent.style.width  = fWidthPct===0  ? "auto" : w - (fdivContent.offsetLeft*2) + "px";
    fdivContent.style.height = fWidthPct===0  ?
                                "auto" :
                                (h - (fdivContent.offsetTop + fdivContent.offsetLeft) + "px");//todo This may need to be put as published.OFFSETY that depends on style
  }

  function outsideClickHandler(e) {
    var target = e.target;
    if (!WAVE.isParentOf(fdivContent, target))
      dialog.close();
  }

  function buttonPressed(e) {
    var keyCode = e.keyCode;
    if (keyCode === 27)
      dialog.close();
  }

  function show() {
    if (fCloseOnClickOutside)
      setTimeout(function() {WAVE.addEventHandler(window, "click", outsideClickHandler)}, 1);
    if (fCloseOnEscape)
      WAVE.addEventHandler(window, "keyup", buttonPressed);
    fOnShow(dialog);
  }

  var tmr = null;
  function __resizeEventHandler() {
    if (tmr) clearTimeout(tmr);//prevent unnecessary adjustments when too many resizes happen
    tmr = setTimeout(function(){  __adjustBounds(); tmr = null; }, 500);
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
    WAVE.removeEventHandler(window, "resize", __resizeEventHandler);
    if (fPreventScroll)
      tryRetrunScrollingToBody();

    if (fCloseOnClickOutside)
      WAVE.removeEventHandler(window, "click", outsideClickHandler);
    if (fCloseOnEscape)
      WAVE.removeEventHandler(window, "keyup", buttonPressed);

    fdivBase.__DIALOG = null;
    fdivBase = null,
    fdivContent = null;
    fdivHeader = null;
    fdivBody = null;
    fdivFooter = null;
    fOnClose = null;
    fOnShow = null;
    dialog = null;

    this.eventInvoke(published.EVT_DIALOG_CLOSE, result);

    return result;
  };

  //closes dialog with OK result
  this.ok = function(){ this.close(published.DLG_OK); };

  //closes dialog with CANCEL result
  this.cancel = function(){ this.close(published.DLG_CANCEL); };

  //get/set title
  this.title = function(val){
    if (typeof(val)===tUNDEFINED || val===null) return fdivHeader.innerHTML;
    fdivHeader.innerHTML = val;
    __adjustBounds();
  };

  //get/set content
  this.content = function(val){
    if (typeof(val)===tUNDEFINED || val===null) return fdivBody.innerHTML;
    $(fdivBody).html( val );
    __adjustBounds();
  };

  this.baseDIV = function() { return fdivBase; };
  this.headerDIV = function() { return fdivHeader; };
  this.bodyDIV = function() { return fdivBody; };
  this.footerDIV = function() { return fdivFooter; };

  //gets/sets width in screen size pct 0..100, where 0 = auto
  this.widthpct = function(val){
    if (typeof(val)===tUNDEFINED || val===fWidthPct) return fWidthPct;
    fWidthPct = val;
    __adjustBounds();
  };

  //gets/sets height in screen size pct 0..100, where 0 = auto
  this.widthpct = function(val){
    if (typeof(val)===tUNDEFINED || val===fHeightPct) return fHeightPct;
    fHeightPct = val;
    __adjustBounds();
  };

  this.adjustBounds = function() { __adjustBounds(); };

  WAVE.addEventHandler(window, "resize", __resizeEventHandler);
  show();

  __adjustBounds();
  if (fPreventScroll)
    tryMakeBodyUnscrollable();

  return dialog;
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

  var fullContent = '<div>' + content + '<div>';

  var divButtons = '<div class="' + published.CLS_DIALOG_CONFIRM_FOOTER + ' ' + footerCls + '">' +
                      btnYes + btnNo + btnOk + btnCancel +
                    '</div>';

  var dialog = new published.Dialog({
    header: WAVE.strDefault(title, 'Confirmation'),
    body: fullContent,
    footer: divButtons,
    cls: WAVE.strDefault(options['cls']),
    onShow: function(dlg){},
    onClose: callback
  });
};

//Returns currently active dialog or null
published.currentDialog = function () {
  for (var i = fCurtains.length - 1; i >= 0; i--)
    if (typeof (fCurtains[i].__DIALOG) !== tUNDEFINED) return fCurtains[i].__DIALOG;
  return null;
};