
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