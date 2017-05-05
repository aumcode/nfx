
    published.SVG = (function () {
      var svg = {};

      var SVG_NS = "http://www.w3.org/2000/svg";
      var SVG_XLINK_NS = "http://www.w3.org/1999/xlink";

      svg.createCircle = function (cx, cy, r, cfg) {
        cfg = cfg || {};
        cfg.cx = cx;
        cfg.cy = cy;
        cfg.r = r;

        return svg.createShape("circle", cfg);
      };

      svg.createRect = function (x, y, width, height, cfg) {
        cfg = cfg || {};
        cfg.x = x;
        cfg.y = y;
        cfg.width = width;
        cfg.height = height;

        return svg.createShape("rect", cfg);
      };

      svg.createLine = function (x1, y1, x2, y2, cfg) {
        cfg = cfg || {};
        cfg.x1 = x1;
        cfg.y1 = y1;
        cfg.x2 = x2;
        cfg.y2 = y2;

        return svg.createShape("line", cfg);
      };

      svg.createPath = function (cfg) {
        return svg.createShape("path", cfg);
      };

      svg.createText = function (txt, x, y, cfg) {
        cfg = cfg || {};
        cfg.x = x;
        cfg.y = y;
        var textWrapper = svg.createShape("text", cfg);
        var textEl = document.createTextNode(txt);
        textWrapper.appendChild(textEl);
        return textWrapper;
      };

      svg.createGroup = function (cfg) {
        return svg.createShape("g", cfg);
      };

      svg.createMarker = function (cfg, elems) {
        var marker = this.createShape("marker", cfg);

        for (var el in elems) marker.appendChild(elems[el]);

        return marker;
      };

      svg.createDefs = function (elems) {
        var defs = this.createShape("defs");

        for (var el in elems) defs.appendChild(elems[el]);

        return defs;
      };

      svg.createUse = function (defs, x, y) {
        var use = svg.createShape("use", { x: x, y: y });
        use.setAttributeNS(SVG_XLINK_NS, 'xlink:href', '#' + defs);
        return use;
      };

      svg.createClipPath = function (id, elems, cfg) {
        cfg = cfg || {};
        cfg.id = id;
        var clipPath = svg.createShape("clipPath", cfg);

        for (var el in elems) clipPath.appendChild(elems[el]);

        return clipPath;
      };

      svg.createShape = function (shapeType, cfg) {
        var el = document.createElementNS(SVG_NS, shapeType);

        for (var attrName in cfg) {
          try {
            el.setAttribute(attrName,cfg[attrName]);
          } catch(e) {
            console.error(e);
          }
        }
        return el;
      };

      return svg;
    }());