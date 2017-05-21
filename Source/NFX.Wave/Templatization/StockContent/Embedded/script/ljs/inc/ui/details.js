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
    modalTitle: text
    modalCls: css class

    contentUrl: url for ajax
    loadCallback: function(data) { optional returns text }

    showOnClick: bool
    showOnFocus: bool
    hideOnClick: bool
    hideOnFocus: bool
    hideOnTimer: bool
    hideOnScroll: bool
    hideOnResize: bool
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

  var fContentUrl = WAVE.strDefault(init.contentUrl);
  var fContentType = WAVE.strDefault(init.contentType);
  var fLoadCallback = WAVE.isFunction(init.loadCallback) ? init.loadCallback : null;

  var fIsTitleHtml = WAVE.strAsBool(init.isTitleHtml, true);
  var fIsContentHtml = WAVE.strAsBool(init.isContentHtml, true);
  var fModalCls = WAVE.strDefault(init.modalCls, published.CLS_DETAILS_MODAL);
  var fModalTitle = "<div class='wvDetailsHeader'>" + WAVE.strDefault(init.modalTitle) + "</div><div class='wvCloseDetails' id='" + fId + "close'></div>";

  var fShowOnClick = WAVE.strAsBool(init.showOnClick, true);
  var fShowOnFocus = WAVE.strAsBool(init.showOnFocus, false);
  var fHideOnTitle = WAVE.strAsBool(init.hideOnTitle, true);
  var fHideOnClick = WAVE.strAsBool(init.hideOnClick, true);
  var fHideOnFocus = WAVE.strAsBool(init.hideOnFocus, false);
  var fHideOnTimer = WAVE.strAsBool(init.hideOnTimer, false);
  var fHideOnScroll = WAVE.strAsBool(init.hideOnScroll, false);
  var fHideOnResize = WAVE.strAsBool(init.hideOnResize, false);

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
      WAVE.addEventHandler(window, "focus", outsideClickHandler);
      setTimeout(function(){
        WAVE.addEventHandler(window, "mousemove", outsideMouseMoveHandler);
      } , 1);
    }
    if (fHideOnClick)
      setTimeout(function(){
        WAVE.addEventHandler(window, "click", outsideClickHandler, true);
      }, 1);
    if (fHideOnScroll)
      setTimeout(function(){
        WAVE.addEventHandler(window, "scroll", __hide);
      }, 1);
    if (fHideOnResize)
      setTimeout(function(){
        WAVE.addEventHandler(window, "resize", __hide);
      }, 1);
  }

  function outsideClickHandler(e) {
    var target = e.target;
    var container = ((fMode === "modal") && (fDialog !== null)) ? fDialog.baseDIV() : fContentControl;
    if ((fDialog !== null && e.target === fDialog.baseDIV()) || !WAVE.isParentOf(container, target)) {
      e.preventDefault();
      __hide();
    }
  }

  function outsideMouseMoveHandler(e) {
    var target = e.target;
    if ((!WAVE.isParentOf(fContentControl, target) && !WAVE.isParentOf(fTitleControl, target))) __hide();
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
        setTitle(fTitle);

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

        setContent(fContent);
      }
    } else if (fTitleControl !== null && fContentControl !== null) {
      WAVE.addClass(fTitleControl, published.CLS_DETAILS_TITLE);
      WAVE.addClass(fContentControl, published.CLS_DETAILS_CONTENT);
    } else
      throw "Details.build(unacceptable init)";

    setTitle(fTitle);

    if (fShowOnClick)
      WAVE.addEventHandler(fTitleControl, "click", __show);
    if (fShowOnFocus) {
      WAVE.addEventHandler(fTitleControl, "mouseover", __show);
      WAVE.addEventHandler(fTitleControl, "focus", __show);
    }

    __hide();
  }

  function setTitle(cont) {
    if(WAVE.strEmpty(fTitle)) return;

    if (fIsTitleHtml)
      fTitleControl.innerHTML = cont;
    else
      fTitleControl.innerText = cont;
  }

  function setContent(cont) {
    if (fIsContentHtml)
      fContentControl.innerHTML = cont;
    else
      fContentControl.innerText = cont;
  }

  function __hide() {
    if(fHiddenState) return;

    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, abort: false };
    details.eventInvoke(published.EVT_DETAILS_HIDE, evtArgsBefore);
    if (evtArgsBefore.abort === true) return;

    setTitle(fTitle);
    if (fShowOnClick)
      WAVE.addEventHandler(fTitleControl, "click", __show);
    if (fHideOnTitle)
      WAVE.removeEventHandler(fTitleControl, "click", __hide);

    if (fMode === "modal" && fDialog !== null) fDialog.close();
    else {
      WAVE.addClass(fContentControl, published.CLS_DETAILS_CONTENT_HIDDEN);
      WAVE.removeClass(fContentControl, published.CLS_DETAILS_CONTENT_VISIBLE);
    }
    WAVE.removeEventHandler(window, "focus", outsideClickHandler);
    WAVE.removeEventHandler(window, "click", outsideClickHandler, true);
    WAVE.removeEventHandler(window, "mousemove", outsideMouseMoveHandler);

    WAVE.addClass(fTitleControl, published.CLS_DETAILS_TITLE_HIDDEN);
    WAVE.removeClass(fTitleControl, published.CLS_DETAILS_TITLE_SHOWN);

    setTimeout(function() {
      fHiddenState = true;
      details.eventInvoke(published.EVT_DETAILS_HIDE, { phase: published.EVT_PHASE_AFTER });
    }, 1);
  }

  function __show() {
    if (!fHiddenState) return;

    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, abort: false };
    details.eventInvoke(published.EVT_DETAILS_SHOW, evtArgsBefore);
    if (evtArgsBefore.abort === true) return;

    setTitle(fAltTitle);
    if (fShowOnClick)
      WAVE.removeEventHandler(fTitleControl, "click", __show);
    if (fHideOnTitle)
      WAVE.addEventHandler(fTitleControl, "click", __hide);

    if (fMode === "modal") {
      fDialog = new published.Dialog({
        cls: published.CLS_DETAILS_CONTENT + " " + fModalCls,
        body: fContent,
        onShow: function(dialog) {
          hideContent();

          if (WAVE.strEmpty(fContentUrl)) return;

          WAVE.ajaxCall(
            "GET",
            fContentUrl,
            null,
            function(data) {
              if (fLoadCallback !== null) {
                var result = fLoadCallback(data);
                if(!WAVE.strEmpty(result))
                  dialog.content(result);
              } else {
                dialog.content(data);
              }
            },
            console.error,
            console.error,
            fContentType,
            fContentType
          );
        },
        header: fModalTitle
      });
      fContentControl = fDialog.bodyDIV();
      WAVE.id(fId + "close").onclick = __hide;
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

  this.content = function(value) {
    if (typeof(value) === tUNDEFINED || value === null)
      return fContent;

    fContent = value;
    setContent(fContent);
  };

  build();

  return details;
};//Details