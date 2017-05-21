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
        if (t.visible() && fTabs[i].name() !== fName) {
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
    if (tab.isActive() || tabs.tabActive() === null)
      tabs.tabActive(tab.name());

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