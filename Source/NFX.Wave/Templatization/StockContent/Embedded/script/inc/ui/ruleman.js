

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