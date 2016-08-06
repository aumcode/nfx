"use strict";
/*jshint devel: true,browser: true, sub: true */ 
/*global WAVE: true,$: true */
WAVE.Chart = {};

WAVE.Chart.SVG = (function () {
  var published = {};

  var AIKEY = "__wv.chart.svg";

  var tUNDEFINED = "undefined";

  var EVT_CHANGED = "EVT_CHANGED";

  published.EVT_SERIES_POINT_MOUSEOVER = "EVT_SERIES_POINT_MOUSEOVER";
  published.EVT_SERIES_POINT_MOUSEOUT = "EVT_SERIES_POINT_MOUSEOUT";
  published.EVT_SERIES_POINT_CLICK = "EVT_SERIES_POINT_CLICK";

  published.EVT_SERIES_POINT_TITLE_MOUSEOVER = "EVT_SERIES_POINT_TITLE_MOUSEOVER";
  published.EVT_SERIES_POINT_TITLE_MOUSEOUT = "EVT_SERIES_POINT_TITLE_MOUSEOUT";
  published.EVT_SERIES_POINT_TITLE_CLICK = "EVT_SERIES_POINT_TITLE_CLICK";

  var UICls = {
      CSS_CLASS_SVG_BKGR:  "wvSvgBkgr",

      CSS_CLASS_AXIS_LINE:  "wvAxisLine",
      CSS_CLASS_AXIS_TICK_LINE:  "wvAxisTickLine",
      CSS_CLASS_AXIS_LABEL:  "wvAxisLabel",

      CSS_CLASS_XZONE_BKGR:  "wvXZoneBackground",
      CSS_CLASS_XAXIS_LABEL:  "wvXAxisLabel",

      CSS_CLASS_YZONE_BKGR:  "wvYZoneBackground",
      CSS_CLASS_YAXIS_LABEL:  "wvYAxisLabel",

      CSS_CLASS_SZONE_BKGR:  "wvSZoneBackground",
      CSS_CLASS_GRID_LINE:  "wvGridLine",
      CSS_CLASS_GRID_XLINE:  "wvGridXLine",
      CSS_CLASS_GRID_YLINE:  "wvGridYLine",

      CSS_CLASS_LZONE_BKGR:  "wvLZoneBackground",
      CSS_CLASS_LEGEND_NAME:  "wvLegendName",

      CSS_CLASS_SERIES_LINE:  "wvSeriesLine",
      CSS_CLASS_SERIES_LINE_SQUARE:  "wvSeriesLineArea",
      CSS_CLASS_SERIES_POINT:  "wvSeriesPoint",
      CSS_CLASS_SERIES_RECT:  "wvSeriesRect",

      CSS_CLASS_TOOLTIP:  "wvChartTooltip",

      CSS_CLASS_TITLE_TEXT:  "wvChartPointTitle"
    };

  var DEFAULT_POINT_TOOLTIP_FORMAT = "Series: @series@, title: @title@, v: [@vx@, @vy@], d: [@dx@, @dy@]";
  var DEFAULT_POINT_TITLE_FORMAT = "@title@";

  var geo = WAVE.Geometry;

  published.DataType = { NUMBER: 0, DATE: 1 };

  published.ChartType = { POINT: 0x01, LINE: 0x02, SPLINE: 0x04, LINE_SQUARE: 0x08, SPLINE_SQUARE: 0x10, BAR: 0x20 };
  published.SquareFillType = { TOP: 0x01, BOTTOM: 0x02 };
  published.PointType = { NONE: 0, CIRCLE: 1, RECT: 2, TRI: 3};
  published.RectSide = { LEFT: 0, TOP: 1, RIGHT: 2, BOTTOM: 3 };
  published.RectCorner = { LEFTTOP: 0, RIGHTTOP: 1, LEFTBOTTOM: 2, RIGHTBOTTOM: 3 };

  published.AxisMarkType = { AUTO: 0, EXACT_BY_POINTS: 1, CUSTOM: 2 };
  
  var MINUTE_SECONDS = 60;
  var HOUR_SECONDS = 60 * 60;
  var DAY_SECONDS = 60 * 60 * 24;
  var MONTH_SECONDS = 30 * DAY_SECONDS;
  var YEAR_SECONDS = 365 * DAY_SECONDS;

  var secondThresholds = [
    1, 2, 5, 10, 20, 30,
    1 * MINUTE_SECONDS, 2 * MINUTE_SECONDS, 5 * MINUTE_SECONDS, 10 * MINUTE_SECONDS, 15 * MINUTE_SECONDS, 20 * MINUTE_SECONDS, 30 * MINUTE_SECONDS,
    1 * HOUR_SECONDS, 2 * HOUR_SECONDS, 3 * HOUR_SECONDS, 4 * HOUR_SECONDS, 6 * HOUR_SECONDS, 8 * HOUR_SECONDS, 12 * HOUR_SECONDS,
    1 * DAY_SECONDS, 2 * DAY_SECONDS, 3 * DAY_SECONDS, 7 * DAY_SECONDS,
    1 * MONTH_SECONDS, 2 * MONTH_SECONDS, 3 * MONTH_SECONDS, 6 * MONTH_SECONDS,
    1 * YEAR_SECONDS, 2 * YEAR_SECONDS, 3 * YEAR_SECONDS, 5 * YEAR_SECONDS, 10 * YEAR_SECONDS, 20 * YEAR_SECONDS, 50 * YEAR_SECONDS];
  var secondThresholdsItr = WAVE.arrayWalkable(secondThresholds);

  function mathLog10(x) { return Math.log(x) / Math.LN10; }

  published.dvInfo = function (_d0, _d1, _v0, _v1, _isLinear) {

    var fIsDate = _d0 instanceof Date;
    this.isDate = function() { return fIsDate; };

    var fd0 = _d0;
    this.d0 = function (val) {
      if (typeof (val) !== tUNDEFINED) fd0 = val;
      return fd0;
    };

    var fd1 = _d1;
    this.d1 = function (val) {
      if (typeof (val) !== tUNDEFINED) fd1 = val;
      return fd1;
    };

    var fv0 = _v0;
    this.v0 = function (val) {
      if (typeof (val) !== tUNDEFINED) fv0 = val;
      return fv0;
    };

    var fv1 = _v1;
    this.v1 = function (val) {
      if (typeof (val) !== tUNDEFINED) fv1 = val;
      return fv1;
    };

    var fIsLinear = _isLinear;
    this.isLinear = function (val) {
      if (typeof (val) !== tUNDEFINED) fIsLinear = val;
      return fIsLinear;
    };

    
    if (_isLinear) {
      
      var dPerV;
      
      if (fIsDate) {
        dPerV = (_d1.getTime() - _d0.getTime()) / (_v1 - _v0);

        this.d2v = function (d) { return Math.floor(_v0 + (d.getTime() - _d0.getTime()) / dPerV); };
        this.v2d = function (v) { return new Date(Math.ceil(_d0.getTime() + (v - _v0) * dPerV)); };
      } else {
        dPerV = (_d1 - _d0) / (_v1 - _v0);
        this.d2v = function (d) { 
          var v = (_v0 + (d - _d0) / dPerV);
          return v;
        };
        this.v2d = function (v) {
          return (_d0 + (v - _v0) * dPerV);
        };
      }
    } else {

      var lgd0;
      var lgd1;
      var k;
      var s;

      if (fIsDate) {
        lgd0 = mathLog10(_d0.getTime());
        lgd1 = mathLog10(_d1.getTime());
        k = (_v1 - _v0) / (lgd1 - lgd0);
        s = _v0 - k * lgd0;

        this.d2v = function (d) { return Math.floor(d.getTime() <= 1 ? 0 : k * mathLog10(d.getTime()) + s); };
        this.v2d = function (v) { return new Date(Math.ceil(Math.pow(10, (v - s) / k))); };
      } else {
        lgd0 = mathLog10(_d0 <= 0 ? 1 : _d0);
        lgd1 = mathLog10(_d1 <= 0 ? 1 : _d1);
        k = (_v1 - _v0) / (lgd1 - lgd0);
        s = _v0 - k * lgd0;

        this.d2v = function (d) { return Math.floor(d <= 1 ? 0 : k * mathLog10(d) + s); };
        this.v2d = function (v) { return Math.ceil(Math.pow(10, (v - s) / k)); };
      }
    }
  };

  published.dvInfo.prototype.toString = function () {
    return "(d0=" + this.d0() + ",d1=" + this.d1() + ") (v0=" + this.v0() + ",v1=" + this.v1() + ")" + " isLinear=" + this.isLinear();
  };

  published.Axis = function (chart, cfg) {
    var self = this;

    WAVE.extend(self, WAVE.EventManager);

    cfg = cfg || {};

    this.chart = function () { return chart; };

    var fDataType = cfg.dataType || published.DataType.NUMBER;
    this.dataType = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fDataType) {
        fDataType = val;
        self.fireChanged();
      }
      return fDataType;
    };

    var fMin = cfg.min || null;
    this.min = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fMin) {
        fMin = val;
        self.fireChanged();
      }
      return fMin;
    };

    var fMax = cfg.max || null;
    this.max = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fMax) {
        fMax = val;
        self.fireChanged();
      }
      return fMax;
    };

    var fMinMargin = cfg.minMargin || 0;
    this.minMargin = function(val) {
      if (typeof(val) !== tUNDEFINED && $.isNumeric(val) && val !== fMinMargin) {
        fMinMargin = val;
        self.fireChanged();
      }
      return fMinMargin;
    };

    var fMaxMargin = cfg.maxMargin || 0;
    this.maxMargin = function(val) {
      if (typeof(val) !== tUNDEFINED && $.isNumeric(val) && val !== fMaxMargin) {
        fMaxMargin = val;
        self.fireChanged();
      }
      return fMaxMargin;
    };

    var fIsLinear = cfg.isLinear !== false;
    this.isLinear = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fIsLinear) {
        fIsLinear = val;
        self.fireChanged();
      }
      return fIsLinear;
    };

    var fMarkType = cfg.markType || published.AxisMarkType.AUTO;
    this.markType = function(val) {
      if (typeof (val) !== tUNDEFINED && val !== fMarkType) {
        fMarkType = val;
        self.fireChanged();
      }
      return fMarkType;
    };

    var fTickLength = 5;
    this.tickLength = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fTickLength) {
        fTickLength = val;
        self.fireChanged();
      }
      return fTickLength;
    };

    var fTickTextMargin = cfg.tickTextMargin || 3;
    this.tickTextMargin = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fTickTextMargin) {
        fTickTextMargin = val;
        self.fireChanged();
      }
      return fTickTextMargin;
    };

    var fDVInfo;
    this.dvInfo = function (val) {
      if (typeof (val) !== tUNDEFINED) fDVInfo = val;
      return fDVInfo;
    };

    var fTicks;
    this.ticks = function (val) {
      if (typeof (val) !== tUNDEFINED) fTicks = val;
      return fTicks;
    };

    this.fireChanged = function() { self.eventInvoke(EVT_CHANGED); };
  };

  published.Axis.prototype.toSeconds = function(dt) { return dt.valueOf() / 1000; };

  published.Axis.prototype.fromSeconds = function (totalSeconds) { return new Date(totalSeconds * 1000); };

  published.Axis.prototype.getVTimeTicks = function (v0, v1, dt0, dt1, labelLength, labelMargin) {
    var d0 = this.toSeconds(dt0), d1 = this.toSeconds(dt1);
    var dd = d1 - d0, dv = v1 - v0;
    var dPerV = dd / dv;

    var ticksLength = this.calcTimeTickLength(dv, dd, labelLength, labelMargin);
    var dTickLength = ticksLength.dThreshhold, vTickLength = ticksLength.vTickLength;

    var dStartTick = Math.floor(d0 / dTickLength) * dTickLength;
    var vTick = (dStartTick - d0) / dPerV;
    var vTicks = [];
    var firstV = labelMargin + labelLength / 2; var lastV = dv - (labelMargin + labelLength / 2);
    while (vTick <= v1) {
      if (vTick >= firstV && vTick <= lastV) {
        vTicks.push({ vTick: v0 + vTick, dTick: this.fromSeconds(d0 + vTick * dPerV) });
      }

      vTick += vTickLength;
    }
    return vTicks;
  };

  published.Axis.prototype.calcTimeTickLength = function (dv, dd, vLblLength, vLblMargin) {
    var dPerV = dd / dv;
    var vTicksMaxQty = Math.floor(dv / (vLblLength + 2 * vLblMargin));
    var dTickMaxLength = dd / vTicksMaxQty;
    var dThreshhold = secondThresholdsItr.wFirst(function (th) { return dTickMaxLength < th; });
    var vTickLength = dThreshhold / dPerV;
    return { vTickLength: vTickLength, dThreshhold: dThreshhold };
  };

  published.Axis.prototype.getVLog10Ticks = function (v0, v1, d0, d1, labelLength, labelMargin) {
    var dd = d1 - d0, dv = v1 - v0;

    var maxPow = Math.ceil(Math.log(dd) / Math.LN10);
    var maxD = Math.pow(10, maxPow);

    var vTickLength = dv / maxPow;

    var vTicks = [];

    var firstV = labelMargin + labelLength / 2; var lastV = dv - (labelMargin + labelLength / 2);
    for (var pow = 0, vTick = v0, dTick = 1; pow <= maxPow; pow++, vTick += vTickLength, dTick *= 10) {
      if (vTick >= firstV && vTick <= lastV)
        vTicks.push({ vTick: v0 + vTick, dTick: d0 + dTick });
    }

    return vTicks;
  };

  published.Axis.prototype.getVTicks = function (v0, v1, d0, d1, labelLength, labelMargin) {
    var vTicks = [];

    var dd = d1 - d0, dv = v1 - v0;
    var dPerV = dd / dv;
    var ticksLength = this.calcTicksLength(dv, dd, labelLength, labelMargin);
    var dTickLength = ticksLength.dTickLength, vTickLength = ticksLength.vTickLength;

    var dStartTick = Math.ceil(d0 / dTickLength) * dTickLength;
    var vTick = (dStartTick - d0) / dPerV;

    var firstV = labelMargin + labelLength / 2; var lastV = dv - (labelMargin + labelLength / 2);
    while (vTick <= v1) {
      if (vTick >= firstV && vTick <= lastV)
        vTicks.push({ vTick: v0 + vTick, dTick: d0 + vTick * dPerV });

      vTick += vTickLength;
    }

    return vTicks;
  };

  published.Axis.prototype.calcTicksLength = function (dv, dd, vLblLength, vLblMargin) {
    var dPerV = dd / dv;
    var threshholds = WAVE.arrayWalkable([1, 2, 3, 5, 10]);
    var vTicksMaxQty = Math.floor(dv / (vLblLength + vLblMargin)) - 1;
    var dTickMaxLength = dd / vTicksMaxQty;
    var d10Pow = Math.floor(Math.log(dTickMaxLength) / Math.LN10);
    var d10K = Math.pow(10, d10Pow);
    var dNormMinTickLength = dTickMaxLength / d10K;
    var dThreshhold = threshholds.wFirst(function (th) { return dNormMinTickLength < th; });
    var dTickLength = dThreshhold * d10K;
    var vTickLength = dTickLength / dPerV;
    return { vTickLength: vTickLength, dTickLength: dTickLength };
  };

  published.Axis.prototype.labelValToStr = function (val) {
    var res;
    if (this.dataType() === published.DataType.NUMBER) {
      res = WAVE.siNum(val, 2);
    } else {
      res = WAVE.toUSDateTimeString(val);
    }
    return res;
  };

  published.Axis.prototype.calcLabelRc = function (svgEl, min, max) {
    var str;

    if (this.dataType() === published.DataType.NUMBER) {
      var strMin = this.labelValToStr(min);
      var strMax = this.labelValToStr(max);
      str = strMax.length >= strMin.length ? strMax : strMin;
    } else { // assume that type is Date
      str = this.labelValToStr(min);
    }

    var txtEl = WAVE.SVG.createText(str, 0, 0, { class: UICls.CSS_CLASS_AXIS_LABEL + " " + UICls.CSS_CLASS_XAXIS_LABEL, visibility: "hidden" });
    svgEl.appendChild(txtEl);
    var boundingRc = txtEl.getBBox();
    svgEl.removeChild(txtEl);
    return boundingRc;
  };

  published.XZone = function (chart) {
    var self = this;

    WAVE.extend(self, WAVE.EventManager);

    this.chart = function () { return chart; };

    var fEnabled = true;
    this.enabled = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fEnabled) { 
        fEnabled = val;
        fireChanged();
      }
      return fEnabled;
    };

    function fireChanged() { self.eventInvoke(EVT_CHANGED); }
  };

  published.XZone.prototype.getHeight = function (svgEl) {
    var seriesItr = this.chart().seriesListWalkable();
    var axises = seriesItr.wGroup(function (s) { return s.xAxis(); }, function (s) { return s.dataSet(); });
    var axisesNHeights = WAVE.arrayWalkable(axises.wSelect(function (e) { return { axis: e.k, height: e.k.getHeight(svgEl, e.v) }; }).wToArray());
    var zoneHeights = axisesNHeights.wGroup(function (e) { return e.axis.isBottom(); }, function (e) { return e.height; });
    var zoneHeight = zoneHeights.wGroupAggregate(function (k, r, e) { return (r || 0) + e; });

    var r = { top: 0, bottom: 0 };

    if (!this.enabled()) return r;

    var walker = zoneHeight.getWalker();
    while(walker.moveNext()) {
      var zh = walker.current();
      if (zh.k === true) r.bottom = zh.v; else r.top = zh.v;
    }

    return r;
  };

  published.XZone.prototype.draw = function (svgEl, areas) {
    for (var i in areas) {
      var area = areas[i];
      var bkgrRc = WAVE.SVG.createRect(area.left(), area.top(), area.width(), area.height(), { class: UICls.CSS_CLASS_SVG_BKGR + " " + UICls.CSS_CLASS_XZONE_BKGR });
      svgEl.appendChild(bkgrRc);
    }

    var axises = this.chart().seriesListWalkable().wGroup(function (s) { return s.xAxis(); }, function (s) { return s.dataSet(); });
    var axisesWalker = axises.getWalker();
    var yTop = areas.top.bottom(), yBottom = areas.bottom.top();

    while (axisesWalker.moveNext()) {
      var axis = axisesWalker.current();

      var axisHeight = axis.k.getHeight(svgEl, axis.v);
      var axisRect;
      if (axis.k.isBottom() === true) {
        axisRect = new geo.Rectangle(new geo.Point(areas.bottom.left(), yBottom), new geo.Point(areas.bottom.right(), yBottom+axisHeight));
        yBottom += axisHeight;
      } else {
        axisRect = new geo.Rectangle(new geo.Point(areas.top.left(), yTop - axisHeight), new geo.Point(areas.top.right(), yTop));
        yTop -= axisHeight;
      }
      axis.k.draw(svgEl, axisRect, axis.v);
    }
  };

  published.XAxis = function (chart, cfg) {
    published.Axis.call(this, chart, cfg);

    var self = this;

    cfg = cfg || {};

    var fVerticalMargin = cfg.verticalMargin || 3;
    this.verticalMargin = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fVerticalMargin) {
        fVerticalMargin = val;
        self.fireChanged();
      }
      return fVerticalMargin;
    };

    var fIsBottom = cfg.isBottom !== false;
    this.isBottom = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fIsBottom) {
        fIsBottom = val;
        self.fireChanged();
      }
      return fIsBottom;
    };
  };

  published.XAxis.prototype = Object.create(published.Axis.prototype);
  published.XAxis.prototype.constructor = published.XAxis;

  published.XAxis.prototype.d2v = function (d) { return this.dvInfo().d2v(d); };

  published.XAxis.prototype.v2d = function (v) { return this.dvInfo().v2d(v); };

  published.XAxis.prototype.draw = function (svgEl, area, dataSets) {
    var minMax = this.getMinMax(dataSets);
    this.dvInfo(new published.dvInfo(minMax.min, minMax.max, area.left(), area.right(), this.isLinear()));
    var axisLine;
    if (this.isBottom())
      axisLine = WAVE.SVG.createLine(area.left(), area.top(), area.right(), area.top(), { class: UICls.CSS_CLASS_AXIS_LINE });
    else
      axisLine = WAVE.SVG.createLine(area.left(), area.bottom(), area.right(), area.bottom(), { class: UICls.CSS_CLASS_AXIS_LINE });
    svgEl.appendChild(axisLine);
    this.markAxis(svgEl, area, dataSets);
  };

  published.XAxis.prototype.getHeight = function (svgEl, dataSets) {
    this.minMax = this.getMinMax(dataSets);

    var h;

    if ((this.minMax.min || this.minMax.min === 0) && (this.minMax.max || this.minMax.max === 0)) {
      var lblRc = this.calcLabelRc(svgEl, this.minMax.min, this.minMax.max);
      h = Math.ceil(lblRc.height);
    } else {
      h = 0;
    }

    return this.tickLength() + this.verticalMargin() + this.tickTextMargin() + h;
  };

  published.XAxis.prototype.getMinMax = function (dataSets) {
    var min = this.min() || dataSets.wSelect(function (ds) { return ds.getMinX(); }).wWhere(function(m) {return m !== null;}).wMin();
    if (min === null) min = (this.dataType() === published.DataType.DATE) ? new Date() : 0;
    if (this.dataType() === published.DataType.DATE)
      min = new Date(min.getTime() - this.minMargin() * 1000);
    else 
      min -= this.minMargin();

    var max = this.max() || dataSets.wSelect(function (ds) { return ds.getMaxX(); }).wMax();
    if (max === null) max = (this.dataType() === published.DataType.DATE) ? new Date() : 0;
    if (this.dataType() === published.DataType.DATE)
      max = new Date(max.getTime() + this.maxMargin() * 1000);
    else 
      max += this.maxMargin();

    this.minMax = {min: min, max: max};
    return this.minMax;
  };

  published.XAxis.prototype.markAxis = function (svgEl, area, dataSets) {
    var _ticks;
    var minMaxs = this.getMinMax(dataSets);
    if ((!this.minMax.min && this.minMax.min !== 0) || (!this.minMax.max && this.minMax.max !== 0)) return;

    var lblRc = this.calcLabelRc(svgEl, minMaxs.min, minMaxs.max);

    var labelLength = lblRc.width;
    var labelHeight = lblRc.height;
    var labelMargin = 5;

    if (this.markType() === published.AxisMarkType.AUTO) {
      if (this.dataType() === published.DataType.DATE)
        _ticks = this.getVTimeTicks(area.left(), area.right(), minMaxs.min, minMaxs.max, labelLength, labelMargin);
      else
        _ticks = this.getVTicks(area.left(), area.right(), minMaxs.min, minMaxs.max, labelLength, labelMargin);
    } else if (this.markType() === published.AxisMarkType.EXACT_BY_POINTS) {
      var primaryDs = dataSets[0];
      _ticks = [];
      for (var i in primaryDs) {
        var p = primaryDs[i];
        _ticks.push({ vTick: this.d2v(p.x), dTick: p.x });
      }
    }

    this.ticks(_ticks);

    for (var j in _ticks) {
      var tick = _ticks[j];

      var line;
      if (this.isBottom())
        line = WAVE.SVG.createLine(tick.vTick, area.top(), tick.vTick, area.top() + this.tickLength(), { class: UICls.CSS_CLASS_AXIS_TICK_LINE });
      else 
        line = WAVE.SVG.createLine(tick.vTick, area.bottom(), tick.vTick, area.bottom() - this.tickLength(), { class: UICls.CSS_CLASS_AXIS_TICK_LINE });
      svgEl.appendChild(line);

      var txt = this.labelValToStr(tick.dTick);
      var txtEl;
      if (this.isBottom())
        txtEl = WAVE.SVG.createText(txt, tick.vTick, area.top() + this.tickTextMargin() + labelHeight, { class: UICls.CSS_CLASS_AXIS_LABEL + " " + UICls.CSS_CLASS_XAXIS_LABEL });
      else
        txtEl = WAVE.SVG.createText(txt, tick.vTick, area.bottom() - this.tickTextMargin() - this.tickLength(), { class: UICls.CSS_CLASS_AXIS_LABEL + " " + UICls.CSS_CLASS_XAXIS_LABEL });
      svgEl.appendChild(txtEl);
      var txtRc = txtEl.getBBox();
      txtEl.setAttribute("x", tick.vTick - txtRc.width / 2);
    }
  };

  published.YZone = function (chart) {
    var self = this;

    WAVE.extend(self, WAVE.EventManager);

    this.chart = function () { return chart; };

    var fEnabled = true;
    this.enabled = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fEnabled) { 
        fEnabled = val;
        fireChanged();
      }
      return fEnabled;
    };

    this.getWidth = function (svgEl) {
      var seriesItr = self.chart().seriesListWalkable();
      
      var axises = seriesItr.wGroup(function (s) { return s.yAxis(); }, function (s) { return s.dataSet(); });
      var axisesNWidths = WAVE.arrayWalkable(axises.wSelect(function (e) { return { axis: e.k, width: e.k.getWidth(svgEl, e.v) }; }).wToArray());
      var zoneWidths = axisesNWidths.wGroup(function (e) { return e.axis.isLeft(); }, function (e) { return e.width; });
      
      var zoneWidth = zoneWidths.wGroupAggregate(function (k, r, e) { return (r || 0) + e; });

      var r = { left: 0, right: 0 };

      if (!fEnabled) return r;

      var walker = zoneWidth.getWalker();

      while (walker.moveNext()) {
        var zw = walker.current();
        if (zw.k === true) r.left = zw.v; else r.right = zw.v;
      }

      return r;
    };

    this.draw = function (svgEl, areas) {

      for (var i in areas) {
        var area = areas[i];
        var bkgrRc = WAVE.SVG.createRect(area.left(), area.top(), area.width(), area.height(), { class: UICls.CSS_CLASS_SVG_BKGR + " " + UICls.CSS_CLASS_YZONE_BKGR });
        svgEl.appendChild(bkgrRc);
      }

      var axises = self.chart().seriesListWalkable().wGroup(function (s) { return s.yAxis(); }, function (s) { return s.dataSet(); });
      var axisesWalker = axises.getWalker();
      var xLeft = areas.left.right(), xRight = areas.right.left();

      while(axisesWalker.moveNext()) {
        var axis = axisesWalker.current();
        var axisWidth = axis.k.getWidth(svgEl, axis.v);
        var axisRect;
        if (axis.k.isLeft() === true) {
          axisRect = new geo.Rectangle(new geo.Point(xLeft - axisWidth, areas.left.top()), new geo.Point(xLeft, areas.left.bottom()));
          xLeft -= axisWidth;
        } else {
          axisRect = new geo.Rectangle(new geo.Point(xRight, areas.left.top()), new geo.Point(xRight+axisWidth, areas.left.bottom()));
          xRight += axisWidth;
        }
        axis.k.draw(svgEl, axisRect, axis.v);
      }
    };

    function fireChanged() { self.eventInvoke(EVT_CHANGED); }
  };

  published.YAxis = function (chart, cfg) {
    published.Axis.call(this, chart, cfg);

    var self = this;

    cfg = cfg || {};

    var fHorizontalMargin = cfg.horizontalMargin || 3;
    this.horizontalMargin = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fHorizontalMargin) {
        fHorizontalMargin = val;
        self.fireChanged();
      }
      return fHorizontalMargin;
    };

    var fMinWidth = cfg.minWidth || 0;
    this.minWidth = function(val) {
      if (typeof (val) !== tUNDEFINED && val !== fMinWidth) {
        fMinWidth = val;
        self.fireChanged();
      }
      return fMinWidth;
    };

    var fIsLeft = cfg.isLeft !== false;
    this.isLeft = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fIsLeft) {
        fIsLeft = val;
        self.fireChanged();
      }
      return fIsLeft;
    };
  };

  published.YAxis.prototype = Object.create(published.Axis.prototype);
  published.YAxis.prototype.constructor = published.YAxis;

  published.YAxis.prototype.d2v = function (d) { 
    var v = this.dvInfo().v0() + this.dvInfo().v1() - this.dvInfo().d2v(d); 
    return v;
  };

  published.YAxis.prototype.v2d = function (v) { return this.dvInfo().v2d(this.dvInfo().v0() + this.dvInfo().v1() - v); };

  published.YAxis.prototype.markAxis = function (svgEl, area, dataSets) {
    var _ticks;

    if (this.markType() === published.AxisMarkType.AUTO) 
    {
      var minMax = this.getMinMax(dataSets);

      var lblRc = this.calcLabelRc(svgEl, minMax.min, minMax.max);
      var labelLength = lblRc.height;
      var labelMargin = labelLength * 0.2;
      var labelWidth = lblRc.width;
      //console.log(lblRc, minMax.min, minMax.max);

      if (this.dataType() === published.DataType.DATE) {
        _ticks = this.getVTimeTicks(area.top(), area.bottom(), minMax.min, minMax.max, labelLength, labelMargin);
      } else {
        if (this.isLinear())
          _ticks = this.getVTicks(area.top(), area.bottom(), minMax.min, minMax.max, labelLength, labelMargin);
        else
          _ticks = this.getVLog10Ticks(area.top(), area.bottom(), minMax.min, minMax.max, labelLength, labelMargin);
      }
    } else if (this.markType() === published.AxisMarkType.EXACT_BY_POINTS) {
      var primaryDs = dataSets[0];
      for (var i in primaryDs) {
        var p = primaryDs[i];
        _ticks.push({ vTick: this.d2v(p.y), dTick: p.y });
      }
    }

    this.ticks(_ticks);

    var maxTickTxtWidth = 0;

    var txtElms = [];
    for (var j in _ticks) {
      var tick = _ticks[j];
      var invVTick = area.top() + area.bottom() - tick.vTick;

      var line;
      if (this.isLeft())
        line = WAVE.SVG.createLine(area.right() - this.tickLength(), invVTick, area.right(), invVTick, { class: UICls.CSS_CLASS_AXIS_TICK_LINE });
      else
        line = WAVE.SVG.createLine(area.left(), invVTick, area.left() + this.tickLength(), invVTick, { class: UICls.CSS_CLASS_AXIS_TICK_LINE });
      svgEl.appendChild(line);

      var txt = this.labelValToStr(tick.dTick);
      var txtEl;
      if (this.isLeft())
        txtEl = WAVE.SVG.createText(txt, area.right() - this.tickTextMargin() - this.tickLength(), invVTick,
                                      { class: UICls.CSS_CLASS_AXIS_LABEL + " " + UICls.CSS_CLASS_YAXIS_LABEL });
      else
        txtEl = WAVE.SVG.createText(txt, area.left() + this.tickLength() + this.tickTextMargin(), invVTick,
                                      { class: UICls.CSS_CLASS_AXIS_LABEL + " " + UICls.CSS_CLASS_YAXIS_LABEL });
      svgEl.appendChild(txtEl);
      var txtRc = txtEl.getBBox();
      txtEl.setAttribute("y", invVTick + txtRc.height / 3);
      txtElms.push(txtEl);
      if (txtRc.width > maxTickTxtWidth) maxTickTxtWidth = txtRc.width;
    }

    if (!this.isLeft()) {
      for (var k in txtElms) {
        var txtElm = txtElms[k];
        var x = parseFloat( txtElm.getAttribute("x"));
        x += maxTickTxtWidth;
        txtElm.setAttribute("x", x);
      }
    }
  };

  published.YAxis.prototype.draw = function (svgEl, area, dataSets) {
    var minMax = this.getMinMax(dataSets);
    this.dvInfo(new published.dvInfo(minMax.min, minMax.max, area.top(), area.bottom(), this.isLinear()));
    var axisLine;
    if (this.isLeft())
      axisLine = WAVE.SVG.createLine(area.right(), area.top(), area.right(), area.bottom(), { class: UICls.CSS_CLASS_AXIS_LINE });
    else
      axisLine = WAVE.SVG.createLine(area.left(), area.top(), area.left(), area.bottom(), { class: UICls.CSS_CLASS_AXIS_LINE });
    svgEl.appendChild(axisLine);
    this.markAxis(svgEl, area, dataSets);
  };

  published.YAxis.prototype.getWidth = function (svgEl, itrDataSets) {
    this.minMax = this.getMinMax(itrDataSets);

    var lblRc = this.calcLabelRc(svgEl, this.minMax.min, this.minMax.max);
    var w = this.tickLength() + this.horizontalMargin() + Math.ceil(lblRc.width);

    return Math.max(w, this.minWidth());
  };

  published.YAxis.prototype.getMinMax = function (dataSets) {
    var min = this.min() || dataSets.wSelect(function (ds) { return ds.getMinY(); }).wWhere(function(m) {return m !== null;}).wMin();
    if (this.dataType() === published.DataType.DATE)
      min = new Date(min.getTime() - this.minMargin() * 1000);
    else 
      min -= this.minMargin();

    var max = this.max() || dataSets.wSelect(function (ds) { return ds.getMaxY(); }).wMax();
    if (this.dataType() === published.DataType.DATE)
      max = new Date(max.getTime() + this.maxMargin() * 1000);
    else 
      max += this.maxMargin();

    this.minMax = {min: min, max: max};
    return this.minMax;
  };

  published.LZone = function (chart, cfg) {
    cfg = cfg || {};

    var self = this;

    WAVE.extend(self, WAVE.EventManager);

    this.chart = function () {
      return chart;
    };

    var fEnabled = cfg.enabled !== false;
    this.enabled = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fEnabled) { 
        fEnabled = val;
        fireChanged();
      }
      return fEnabled;
    };

    var fLegendMargin = cfg.legendMargin || 10;
    this.legendMargin = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fLegendMargin) { 
        fLegendMargin = val;
        fireChanged();
      }
      return fLegendMargin;
    };

    var fSeriesMargin = cfg.seriesMargin || 3;
    this.seriesMargin = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fSeriesMargin) {
        fSeriesMargin = val;
        fireChanged();
      }
      return fSeriesMargin;
    };

    var fPrefixLength = cfg.prefixLength || 10;
    this.prefixLength = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPrefixLength) {
        fPrefixLength = val;
        fireChanged();
      }
      return fPrefixLength;
    };

    var fRowSpace = cfg.rowSpace || 3;
    this.rowSpace = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fRowSpace) {
        fRowSpace = val;
        fireChanged();
      }
      return fRowSpace;
    };

    var fCorner = cfg.corner || published.RectCorner.RIGHTTOP;
    this.corner = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fCorner) {
        fCorner = val;
        fireChanged();
      }
      return fCorner;
    };

    function fireChanged() { self.eventInvoke(EVT_CHANGED); }
  };

  published.LZone.prototype.getSize = function (svgEl) {
    var maxName = this.chart().seriesListWalkable().wSelect(function (e) { return e.title() || ""; }).wMax(function (a, b) { return a.length > b.length; });
    
    var txtEl = WAVE.SVG.createText(maxName, 0, 0, { class: UICls.CSS_CLASS_LEGEND_NAME, visibility: "hidden" });
    svgEl.appendChild(txtEl);
    var boundingRc = txtEl.getBBox();
    svgEl.removeChild(txtEl);

    var width = this.rowSpace() + this.prefixLength() + this.rowSpace() + boundingRc.width + this.rowSpace();
    var height = this.chart().seriesListWalkable().wCount() * (2 * this.seriesMargin() + boundingRc.height);

    return {w: width, h: height};
  };

  published.LZone.prototype.drawLegendPrefix = function (svgEl, area, series) {
    var y = (area.top() + area.bottom()) / 2;
    var pathEl = WAVE.SVG.createPath({ d: "M" + area.left() + "," + y + " L" + (area.left() + this.prefixLength()) + "," + y, class: series.lineClass() });
    svgEl.appendChild(pathEl);
  };

  published.LZone.prototype.draw = function (svgEl, area) {
    if (!this.enabled()) return;

    var self = this;

    var seriesList = this.chart().seriesListWalkable();

    var backRcEl = WAVE.SVG.createRect(area.left(), area.top(), area.width(), area.height(), { class: UICls.CSS_CLASS_LZONE_BKGR });
    svgEl.appendChild(backRcEl);

    var y = area.top() + this.seriesMargin();

    seriesList.wEach(function(series){
      var g = WAVE.SVG.createGroup({class: series.class()});
      svgEl.appendChild(g);      

      var txtEl = WAVE.SVG.createText(series.title(), area.left() + self.rowSpace() + self.prefixLength() + self.rowSpace(), y, { class: UICls.CSS_CLASS_LEGEND_NAME });
      g.appendChild(txtEl);
      var boundingRc = txtEl.getBBox();
      txtEl.setAttribute("y", y + boundingRc.height * 3 / 4);

      var prefixArea = new geo.Rectangle(
        new geo.Point(area.left() + self.rowSpace(), y),
        new geo.Point(area.left() + self.rowSpace() + self.prefixLength(), y + boundingRc.height));
      self.drawLegendPrefix(g, prefixArea, series);

      y += boundingRc.height + 2 * self.seriesMargin();
    });
  };

  var seriesIdSeed = 0;
  published.SZone = function (chart, cfg) {
    var self = this;

    WAVE.extend(self, WAVE.EventManager);

    cfg = cfg || {};

    this.chart = function () { return chart; };

    var fChartType = cfg.chartType || published.ChartType.LINE;
    this.chartType = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fChartType) {
        fChartType = val;
        fireChanged();        
      }
      return fChartType;
    };

    var fSquareFillType = cfg.squareFillType || published.SquareFillType.BOTTOM;
    this.squareFillType = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fSquareFillType) {
        fSquareFillType = val;
        fireChanged();        
      }
      return fSquareFillType;
    };

    var fPointType = cfg.pointType || published.PointType.CIRCLE;
    this.pointType = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointType) {
        fPointType = val;
        fireChanged();
      }
      return fPointType;
    };

    var fLineClass = cfg.lineClass || UICls.CSS_CLASS_SERIES_LINE;
    this.lineClass = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fLineClass) {
        fLineClass = val;
        fireChanged();
      }
      return fLineClass;
    };

    var fLineSquareClass = cfg.lineSquareClass || UICls.CSS_CLASS_SERIES_LINE_SQUARE;
    this.lineSquareClass = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fLineSquareClass) {
        fLineSquareClass = val;
        fireChanged();
      }
      return fLineSquareClass;
    };

    var fPointClass = cfg.pointClass || UICls.CSS_CLASS_SERIES_POINT;
    this.pointClass = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointClass) {
        fPointClass = val;
        fireChanged();
      }
      return fPointClass;
    };

    var fPointSize = cfg.pointSize || 5;
    this.pointSize = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointSize) {
        fPointSize = val;
        fireChanged();
      }
      return fPointSize;
    };

    var fBarClass = cfg.barClass || UICls.CSS_CLASS_SERIES_RECT;
    this.barClass = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fBarClass) {
        fBarClass = val;
        fireChanged();
      }
      return fBarClass;
    };

    var fBarWidth = cfg.barWidth || 10;
    this.barWidth = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fBarWidth) {
        fBarWidth = val;
        fireChanged();
      }
      return fBarWidth;
    };

    var fBarGroupMargin = cfg.barGroupMargin || 5;
    this.barGroupMargin = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fBarGroupMargin) {
        fBarGroupMargin = val;
        fireChanged();
      }
      return fBarGroupMargin;
    };

    var fBarMargin = cfg.barMargin || 3;
    this.barMargin = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fBarGroupMargin) {
        fBarMargin = val;
        fireChanged();
      }
      return fBarMargin;
    };

    var fShowPointTooltip = cfg.showPointTooltip || false;
    this.showPointTooltip = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fShowPointTooltip) {
        fShowPointTooltip = val;
      }
      return fShowPointTooltip;
    };

    var fPointTooltipFormat = cfg.pointTooltipFormat || DEFAULT_POINT_TOOLTIP_FORMAT;
    this.pointTooltipFormat = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointTooltipFormat) {
        fPointTooltipFormat = val;
      }
      return fPointTooltipFormat;
    };

    var fShowPointTitle = cfg.showPointTitle || false;
    this.showPointTitle = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fShowPointTitle) {
        fShowPointTitle = val;
        fireChanged();
      }
      return fShowPointTitle;
    };

    var fPointTitleFormat = cfg.pointTitleFormat || DEFAULT_POINT_TITLE_FORMAT;
    this.pointTitleFormat = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointTitleFormat) {
        fPointTitleFormat = val;
        fireChanged();
      }
      return fPointTitleFormat;
    };

    var fPointTitleClass = cfg.pointTitleClass || UICls.CSS_CLASS_TITLE_TEXT;
    this.pointTitleClass = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fPointTitleClass) {
        fPointTitleClass = val;
        fireChanged();
      }
      return fPointTitleClass;
    };

    var fPointTitleAngle = cfg.pointTitleAngle || 0;
    this.pointTitleAngle = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fPointTitleAngle) {
        fPointTitleAngle = val;
        fireChanged();
      }
      return fPointTitleAngle;
    };

    var fPointTitleMargin = cfg.pointTitleMargin || 5;
    this.pointTitleMargin = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fPointTitleMargin) {
        fPointTitleMargin = val;
        fireChanged();
      }
      return fPointTitleMargin;
    };

    var fShowPointTitleLeg = cfg.showPointTitleLeg !== false;
    this.showPointTitleLeg = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fShowPointTitleLeg) {
        fShowPointTitleLeg = val;
        fireChanged();
      }
      return fShowPointTitleLeg;
    };

    this.showDefaultRuler = function (val) {
      var showDefaultRuler = fRulerScopes.length > 0;
      if (typeof (val) !== tUNDEFINED && val !== showDefaultRuler) {
        if (val) self.attachToRulerScope(""); else self.detachFromRulerScope("");
        if (fRulerScopes.length)
          fireChanged();
        return val;
      } else {
        return showDefaultRuler;
      }
    };

    var fRulerScopes = [];
    this.rulerScopes = function() { return WAVE.arrayShallowCopy(fRulerScopes); };

    this.attachToRulerScope = function(scopeName) {
      if (fRulerScopes.indexOf(scopeName) !== -1) return;
      fRulerScopes.push(scopeName);
      fireChanged();
    };

    this.detachFromRulerScope = function(scopeName) {
      var idx = fRulerScopes.indexOf(scopeName);
      if (idx === -1) return;
      fRulerScopes.splice(idx, 1);
      fireChanged();
    };

    // Defines if x-coordinate should be within sZone rect to show ruler (sets isInParentRc in getSlaveInfo chart method)
    var fRulerCheckXContains = cfg.rulerCheckXContains !== false;
    this.rulerCheckXContains = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fRulerCheckXContains) {
        fRulerCheckXContains = val;
        fireChanged();
      }
      return fRulerCheckXContains;
    };

    // Defines if y-coordinate should be within sZone rect to show ruler (sets isInParentRc in getSlaveInfo chart method)
    var fRulerCheckYContains = cfg.rulerCheckYContains !== false;
    this.rulerCheckYContains = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fRulerCheckYContains) {
        fRulerCheckYContains = val;
        fireChanged();
      }
      return fRulerCheckYContains;
    };

    var fLastArea = null;
    this.lastArea = function () { return fLastArea; };

    var fVTitles = [], fVTitlesWalkable = WAVE.arrayWalkable(fVTitles); // array Geometry.Rectangle
    var fVPoints = [], fVPointsWalkable = WAVE.arrayWalkable(fVPoints); // array Geometry.Rectangle
    var fVLines = [], fVLinesWalkable = WAVE.arrayWalkable(fVLines); // array lines {x1, y1, x2, y2}

    this.addVTitle = function(t) {
      fVTitles.push(t);
    };

    this.addVPoint = function(p) {
      fVPoints.push(p);
    };

    this.addVLine = function(l) {
      fVLines.push(l);
    };

    this.getTitleRcPenalty = function(titleRc, area) {
      var selfSquare = titleRc.square();

      var visibleSquare = WAVE.Geometry.overlapAreaRect(area, titleRc);
      var invisibleSquare = selfSquare - visibleSquare;

      var titleOverlapSquare = fVTitlesWalkable
        .wWhere(function(tRc) { return !(tRc.right() < titleRc.left() || tRc.left() > titleRc.right() || tRc.bottom() < titleRc.top() || tRc.top() > titleRc.bottom()); })
        .wSelect(function(tRc) { return WAVE.Geometry.overlapAreaRect(titleRc, tRc); }).wSum();

      var pointsOverlap = fVPointsWalkable
        .wWhere(function(pRc) { return !(pRc.right() < titleRc.left() || pRc.left() > titleRc.right() || pRc.bottom() < titleRc.top() || pRc.top() > titleRc.bottom()); })
        .wSelect(function(pRc) { return WAVE.Geometry.overlapAreaRect(titleRc, pRc); }).wSum();

      var lineOverlap = fVLinesWalkable
        .wWhere(function(l) { return !(l.x2 < titleRc.left() || l.x1 > titleRc.right()); })
        .wSelect(function(l) { 
          return WAVE.Geometry.intersectionLengthRectLineXY(titleRc.left(), titleRc.top(), titleRc.right(), titleRc.bottom(), l.x1, l.y1, l.x2, l.y2); })
        .wSum();

      return invisibleSquare + titleOverlapSquare + pointsOverlap + 2 * lineOverlap;
    };

    function fireChanged() { self.eventInvoke(EVT_CHANGED); }

    this.draw = function (svgEl, area) {

      fLastArea = area;

      fVTitles.splice(0, fVTitles.length);
      fVPoints.splice(0, fVPoints.length);
      fVLines.splice(0, fVLines.length);

      var bkgrRc = WAVE.SVG.createRect(area.left(), area.top(), area.width(), area.height(), { class: UICls.CSS_CLASS_SVG_BKGR + " " + UICls.CSS_CLASS_SZONE_BKGR });
      svgEl.appendChild(bkgrRc);

      var id = "clip" + WAVE.genAutoincKey(AIKEY);
      var clipRectEl = WAVE.SVG.createRect(area.left(), area.top(), area.width(), area.height());
      var clipPathEl = WAVE.SVG.createClipPath(id, [clipRectEl]);
      var clipDefsEl = WAVE.SVG.createDefs([clipPathEl]);
      svgEl.appendChild(clipDefsEl);
      var clipGroup = WAVE.SVG.createGroup({ style: "clip-path: url(#" + id + ")" });

      svgEl.appendChild(clipGroup);

      var seriesItr = self.chart().seriesListWalkable();

      var xAxises = seriesItr.wSelect( function (s) { return s.xAxis(); }).wDistinct();
      var yAxises = seriesItr.wSelect( function (s) { return s.yAxis(); }).wDistinct();

      xAxises.wEach(function(xAxis) {
        var ticks = xAxis.ticks();
        for (var j in ticks) {
          var tick = ticks[j];
          var gridLine = WAVE.SVG.createLine(tick.vTick, area.top(), tick.vTick, area.bottom(), {class: UICls.CSS_CLASS_GRID_LINE + " " + UICls.CSS_CLASS_GRID_XLINE });
          clipGroup.appendChild(gridLine);
        }
      });

      yAxises.wEach(function(yAxis) {
        var ticks = yAxis.ticks();
        for (var i in ticks) {
          var tick = ticks[i];
          var invVTick = area.top() + area.bottom() - tick.vTick;
          var gridLine = WAVE.SVG.createLine(area.left(), invVTick, area.right(), invVTick, { class: UICls.CSS_CLASS_GRID_LINE + " " + UICls.CSS_CLASS_GRID_YLINE });
          clipGroup.appendChild(gridLine);
        }
      });

      var seriesByChartType = seriesItr.wGroup(function (el) { return el.chartType(); }, function (el) { return el; });

      seriesByChartType.wEach(function(seriesList) {
        switch (seriesList.k) {
          case published.ChartType.BAR:

            var barWidth = self.barWidth();
            var barGroupMargin = self.barGroupMargin();
            var barMargin = self.barMargin();

            var seriesQty = seriesList.v.wCount();
            var series1 = seriesList.v.wFirst();

            if (series1.barBase() === published.RectSide.BOTTOM) {

              var minSeries = seriesList.v.wMin(function(a, b) { return a.dataSet().getMinX() < a.dataSet().getMinX(); });
              if (typeof(minSeries) === tUNDEFINED) return;
              var maxSeries = seriesList.v.wMax(function(a, b) { return a.dataSet().getMaxX() > a.dataSet().getMaxX(); });
              if (typeof(maxSeries) === tUNDEFINED) return;

              var minXD = minSeries.dataSet().getMinX();
              var maxXD = maxSeries.dataSet().getMaxX();

              var xAxis = series1.xAxis();
              var dvInfo = xAxis.dvInfo();

              var minXV = dvInfo.d2v(minXD);
              var maxXV = dvInfo.d2v(maxXD);

              var barGroupWidth = seriesQty * (barWidth + barMargin) - barMargin;

              var groupQty = Math.floor((maxXV - minXV + barGroupMargin) / (barGroupWidth + barGroupMargin));

              var newFullGroupWidth = (maxXV - minXV + barGroupMargin) / groupQty;

              for(var iGroup=0; iGroup<groupQty; iGroup++) {

                var firstGroup = true;
                var startGroupXV = minXV + newFullGroupWidth * iGroup;

                var seriesBaseVX = startGroupXV;

                seriesList.v.wEach(function(s) {
                
                  var dx = dvInfo.v2d(startGroupXV);

                  var vy = s.getVY(dx);

                  var d = "M" + seriesBaseVX + "," + vy + " h" + barWidth + " V" + area.bottom() + " h-" + barWidth + " Z";

                  var barEl = WAVE.SVG.createPath({d: d, class: s.lineSquareClass() });

                  var g = WAVE.SVG.createGroup({class: s.class()});
                  clipGroup.appendChild(g);
                  g.appendChild(barEl);

                  seriesBaseVX += barWidth + barMargin;
                });            
              }
            }

            break;
          default:
              seriesList.v.wEach(function(series) {
                var g = WAVE.SVG.createGroup({class: series.class()});
                clipGroup.appendChild(g);
              
                series.drawAllButTitle(svgEl, g, area);
              });
            break;
        }
      });

      seriesItr.wWhere(function(s) { return s.chartType() & published.ChartType.POINT; }).wEach(function(series) {
        var g = WAVE.SVG.createGroup({class: series.class()});
        clipGroup.appendChild(g);

        series.drawPointTitles(svgEl, g, area);
      });

    };
  };

  var Series = function (chart, cfg) {
    var self = this;

    WAVE.extend(self, WAVE.EventManager);

    cfg = cfg || {};

    this.chart = function () { return chart; };

    var fID = typeof(cfg.id) !== tUNDEFINED ? cfg.id : (++seriesIdSeed).toString();
    this.id = function() { return fID; };

    var fTitle = cfg.title || "";
    this.title = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fTitle) { 
        fTitle = val;
        fireChanged();
      }
      return fTitle;
    };

    var fClass = cfg.class || "";
    this.class = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fClass) { 
        fClass = val;
        fireChanged();
      }
      return fClass;
    };

    var fLineClass = cfg.lineClass;
    this.lineClass = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fLineClass) {
        fLineClass = val;
        fireChanged();
      }
      return fLineClass || self.chart().sZone().lineClass();
    };

    var fLineSquareClass = cfg.lineSquareClass;
    this.lineSquareClass = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fLineSquareClass) {
        fLineSquareClass = val;
        fireChanged();
      }
      return fLineSquareClass || self.chart().sZone().lineSquareClass();
    };

    var fBarClass = cfg.barClass;
    this.barClass = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fBarClass) {
        fBarClass = val;
        fireChanged();
      }
      return fBarClass || self.chart().sZone().barClass();
    };

    var fChartType = cfg.chartType;
    this.chartType = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fChartType) {
        fChartType = val;
        fireChanged();
      }
      return fChartType || self.chart().sZone().chartType();
    };

    var fSquareFillType = cfg.squareFillType;
    this.squareFillType = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fSquareFillType) {
        fSquareFillType = val;
        fireChanged();        
      }
      return fSquareFillType || self.chart().sZone().squareFillType();
    };

    var fPointType = cfg.pointType;
    this.pointType = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointType) {
        fPointType = val;
        fireChanged();
      }
      return fPointType || self.chart().sZone().pointType();
    };

    var fPointClass = cfg.pointClass || UICls.CSS_CLASS_SERIES_POINT;
    this.pointClass = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointClass) {
        fPointClass = val;
        fireChanged();
      }
      return fPointClass || self.chart().sZone().pointClass();
    };

    var fPointSize = cfg.pointSize;
    this.pointSize = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointSize) {
        fPointSize = val;
        fireChanged();
      }
      return fPointSize || self.chart().sZone().pointSize();
    };

    var fBarBase = published.RectSide.BOTTOM;
    this.barBase = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fBarBase) {
        fBarBase = val;
        fireChanged();
      }
      return fBarBase || self.chart().sZone().barBase();
    };

    var fDataSet = new published.DataSet();
    fDataSet.eventBind(EVT_CHANGED, function() { self.eventInvoke(EVT_CHANGED); });
    this.dataSet = function () { return fDataSet; };

    var fXAxis = cfg.xAxis;
    this.xAxis = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fXAxis) {
        if (fXAxis) fXAxis.eventUnbind(EVT_CHANGED, fireChanged);
        fXAxis = val;
        fXAxis.eventBind(EVT_CHANGED, fireChanged);
        fireChanged();
      }
      return fXAxis || self.chart().xAxis();
    };

    var fYAxis = cfg.yAxis;
    this.yAxis = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fYAxis) {
        if (fYAxis) fYAxis.eventUnbind(EVT_CHANGED, fireChanged);
        fYAxis = val;
        fYAxis.eventBind(EVT_CHANGED, fireChanged);
        fireChanged();
      }
      return fYAxis || self.chart().yAxis();
    };

    var fShowPointTooltip = cfg.showPointTooltip;
    this.showPointTooltip = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fShowPointTooltip) {
        fShowPointTooltip = val;
      }
      return fShowPointTooltip || self.chart().sZone().showPointTooltip();
    };

    var fPointTooltipFormat = cfg.pointTooltipFormat;
    this.pointTooltipFormat = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointTooltipFormat) {
        fPointTooltipFormat = val;
      }
      return fPointTooltipFormat || self.chart().sZone().pointTooltipFormat();
    };

    var fShowPointTitle = cfg.showPointTitle;
    this.showPointTitle = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fShowPointTitle) {
        fShowPointTitle = val;
        fireChanged();
      }
      return fShowPointTitle || self.chart().sZone().showPointTitle();
    };

    var fPointTitleFormat = cfg.pointTitleFormat;
    this.pointTitleFormat = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fPointTitleFormat) {
        fPointTitleFormat = val;
        fireChanged();
      }
      return fPointTitleFormat || self.chart().sZone().pointTitleFormat();
    };

    var fPointTitleClass = cfg.pointTitleClass;
    this.pointTitleClass = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fPointTitleClass) {
        fPointTitleClass = val;
        fireChanged();
      }
      return fPointTitleClass || chart.sZone().pointTitleClass();
    };

    var fPointTitleAngle = cfg.pointTitleAngle;
    this.pointTitleAngle = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fPointTitleAngle) {
        fPointTitleAngle = val;
        fireChanged();
      }
      return fPointTitleAngle || chart.sZone().pointTitleAngle();
    };

    var fPointTitleMargin = cfg.pointTitleMargin;
    this.pointTitleMargin = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fPointTitleMargin) {
        fPointTitleMargin = val;
        fireChanged();
      }
      return fPointTitleMargin || chart.sZone().pointTitleMargin();
    };

    var fShowPointTitleLeg = cfg.showPointTitleLeg !== false;
    this.showPointTitleLeg = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fShowPointTitleLeg) {
        fShowPointTitleLeg = val;
        fireChanged();
      }
      return fShowPointTitleLeg === false ? false : fShowPointTitleLeg || chart.sZone().showPointTitleLeg();
    };

    function fireChanged() { self.invokeEvent(EVT_CHANGED); }
  };

  Series.prototype.getDY = function(dx) {
    var p = this.dataSet().getPointByX(dx);
    if (p !== null) return p.y;

    var neighbours = this.dataSet().getNearestNeighbours(dx);

    if (neighbours === null) return null;

    if (neighbours.left === null) { return neighbours.right.y; }
    if (neighbours.right === null) return neighbours.left.y;

    var k = (dx - neighbours.left.x) / (neighbours.right.x - neighbours.left.x);
    var dy = k * (neighbours.right.y - neighbours.left.y) + neighbours.left.y;

    return dy;
  };

  Series.prototype.getVY = function(dx) {
    var dy = this.getDY(dx);
    var vy = this.yAxis().d2v(dy);
    return vy;
  };

  Series.prototype.foreachPoint = function(func) {
    var self = this;

    var xAxis = this.xAxis();
    var yAxis = this.yAxis();

    self.dataSet().forEach( function(dp) {
      var vx = Math.round(xAxis.d2v(dp.x));
      var vy = Math.round(yAxis.d2v(dp.y));

      if (!isNaN(vx) && !isNaN(vy)) func(vx, vy, dp.x, dp.y, dp.title, self);
    });
  };

  Series.prototype.drawAllButTitle = function (svgEl, parentEl, area, drawTitle) {

    var self = this;

    var path;
    var firstX;
    var pathEl;

    if ((this.chartType() & published.ChartType.LINE) || (this.chartType() & published.ChartType.LINE_SQUARE)) {

      path = "";

      firstX = null;

      var prevVx, prevVy;
      self.foreachPoint(function (vx, vy, dx, dy, title, series) {
        var cmd;
        if (firstX === null) { 
          firstX = vx; cmd = "M";
        } else {
          cmd = "L";
          self.chart().sZone().addVLine({x1: prevVx, y1: prevVy, x2: vx, y2: vy});
        }
        path += cmd + vx + ',' + vy + ' ';
        prevVx = vx; prevVy = vy;
      });

      if (path !== "" && (this.chartType() & published.ChartType.LINE_SQUARE)) {
        if(this.squareFillType() === published.SquareFillType.BOTTOM)
          path += "V" + area.bottom() + " H" + firstX + " Z";
        else 
          path += "V" + area.top() + " H" + firstX + " Z";
      }

      pathEl = WAVE.SVG.createPath({ d: path, class: ((this.chartType() & published.ChartType.LINE_SQUARE)) ? 
        this.lineSquareClass() : this.lineClass() });
      parentEl.appendChild(pathEl);  
    }

    if (this.chartType() & published.ChartType.SPLINE || this.chartType() & published.ChartType.SPLINE_SQUARE) {
      path = "";
      var expectedPoint = "M", expectedQty = 0, buf = [];

      firstX = null;

      self.foreachPoint(function(vx, vy, dx, dy, title, series) {
        if (firstX === null) { firstX = vx; }


         var p1, p2, pTo;

        if (expectedPoint === "M") {
          path = "M" + vx + ',' + vy + ' ';
          expectedPoint = "C";
        } else if (expectedPoint === "C") {

          buf.unshift({x: vx, y: vy});
          expectedQty++;
          if (expectedQty === 3) {
            p1 = buf.pop();
            p2 = buf.pop();
            pTo = buf.pop();
            path += 'C' + p1.x + ',' + p1.y + ' ' + p2.x + ',' + p2.y + ' ' + pTo.x + ',' + pTo.y + ' ';
            expectedQty = 0;
            expectedPoint = "S";
          }
        } else if (expectedPoint === "S") {
          buf.unshift({x: vx, y: vy});
          expectedQty++;
          if (expectedQty === 2) {
            p2 = buf.pop();
            pTo = buf.pop();
            path += 'S' + p2.x + ',' + p2.y + ' ' + pTo.x + ',' + pTo.y + ' ';
            expectedQty = 0;
          }
        }
      });

      var p;
      if (expectedPoint === "C") {
        if (expectedQty === 1) {
          p = buf.pop();
          path += 'L' + p.x + ',' + p.y;
        } else if (expectedQty === 2) {
          var p1 = buf.pop(), pTo = buf.pop();
          path += 'Q' + p1.x + ',' + p1.y + ' ' + pTo.x + ',' + pTo.y;
        } 
      } else if (expectedPoint === "S") {
        if (expectedQty === 1) {
          p = buf.pop();
          path += "T" + p.x + ',' + p.y;
        }
      }

      if (path !== "" && (this.chartType() & published.ChartType.SPLINE_SQUARE)) {
        path += "V" + area.bottom() + " H" + firstX + " Z";
      }

      pathEl = WAVE.SVG.createPath({ d: path, class: ((this.chartType() & published.ChartType.SPLINE_SQUARE)) ? 
        this.lineSquareClass() : this.lineClass() });

      parentEl.appendChild(pathEl);
    }

    if (this.chartType() & published.ChartType.POINT) {
      self.foreachPoint(function (vx, vy, dx, dy, title, series) {
        self.drawPoint(svgEl, parentEl, area, vx, vy, dx, dy, title, series);
      });
    }
  };

  Series.prototype.drawPointTitles = function (svgEl, parentEl, area) {
    var self = this;
    if (this.chartType() & published.ChartType.POINT) {
      this.foreachPoint(function (vx, vy, dx, dy, title, series) {
        self.drawPointTitle(svgEl, parentEl, area, vx, vy, dx, dy, title);
      });
    }
  };

  Series.prototype.drawPoint = function (svgEl, parentEl, area, vx, vy, dx, dy, title) {
    var pointEl;
    switch(this.pointType()) {
      case published.PointType.CIRCLE:
          pointEl = this.drawPointCircle(vx, vy, dx, dy);
        break;
      case published.PointType.RECT:
          pointEl = this.drawPointRect(vx, vy, dx, dy);
        break;
      case published.PointType.TRI:
          pointEl = this.drawPointTriangle(vx, vy, dx, dy);
        break;
    }
    var self = this;

    if (pointEl) {
      pointEl.addEventListener("mouseover", function(evt) { self.chart().onSeriesMouseOver({originalEvt: evt, vx: vx, vy: vy, dx: dx, dy: dy, title: title, series: self});} );
      pointEl.addEventListener("mouseout", function(evt) { self.chart().onSeriesMouseOut({originalEvt: evt, vx: vx, vy: vy, dx: dx, dy: dy, title: title, series: self});} );
      pointEl.addEventListener("click", function(evt) { self.chart().onSeriesClick({originalEvt: evt, vx: vx, vy: vy, dx: dx, dy: dy, title: title, series: self});} );

      parentEl.appendChild(pointEl);
    }

    //this.drawPointTitle(svgEl, parentEl, area, vx, vy, dx, dy, title);
  };

  Series.prototype.drawPointTitle = function(svgEl, parentEl, area, vx, vy, dx, dy, title) {
    if (this.showPointTitle() && title) {
      var titleMsg = WAVE.strTemplate(this.pointTitleFormat(), {vx: vx, vy: vy, dx: dx, dy: dy, title: title, series: this.title()});
      
      var g = WAVE.SVG.createGroup({class: this.pointTitleClass(), transform: "rotate(" + this.pointTitleAngle() + "," + vx + "," + vy + ")"});
      var pointTxtEl = WAVE.SVG.createText(titleMsg, vx, vy);
      g.appendChild(pointTxtEl);

      svgEl.appendChild(g);
      var txtBBox = g.getBBox();
      svgEl.removeChild(g);

      var txtBox = {x: txtBBox.x, y: txtBBox.y, width: txtBBox.width, height: txtBBox.height};

      var padding = 2;
      txtBox.x -= padding;
      txtBox.y -= padding;
      txtBox.width += 2 * padding;
      txtBox.height += 2 * padding;

      var dBoxLeftVx = vx - txtBox.x, dBoxLeftVy = vy - txtBox.y;

      var newBoxYMiddle = vy;
      var oldBoxYMiddle = txtBox.y + txtBox.height / 2;
      var deltaY = newBoxYMiddle - oldBoxYMiddle;

      var newBoxXLeft = vx + this.pointTitleMargin();
      var oldBoxXLeft = txtBox.x;
      var deltaX = newBoxXLeft - oldBoxXLeft;

      var titleRc = WAVE.Geometry.toRectWH(txtBox.x + deltaX, txtBox.y + deltaY, txtBox.width, txtBox.height);

      var sZone = this.chart().sZone();

      var minPenalty = Number.MAX_VALUE;
      for (var rad = 0; rad < 2 * Math.PI; rad+=Math.PI / 4) {
        var rc = WAVE.Geometry.rotateRectAroundCircle(vx, vy, 15, titleRc.width(), titleRc.height(), rad);

        var penalty = sZone.getTitleRcPenalty(rc, area);

        if(penalty === 0) {
          titleRc = rc;
          break;
        }

        if (penalty < minPenalty) {
          minPenalty = penalty;
          titleRc = rc;
        }
      }

      sZone.addVTitle(titleRc);

      var rcEl = WAVE.SVG.createRect(titleRc.left(), titleRc.top(), titleRc.width(), titleRc.height(), {rx: 4, ry: 4});

      pointTxtEl.setAttribute("x", titleRc.left() + dBoxLeftVx);
      pointTxtEl.setAttribute("y", titleRc.top() + dBoxLeftVy);
      
      var self = this;
      g.addEventListener("mouseover", function(evt) { self.chart().onSeriesPointTitleMouseOver({originalEvt: evt, vx: vx, vy: vy, dx: dx, dy: dy, title: title, series: self});} );
      g.addEventListener("mouseout", function(evt) { self.chart().onSeriesPointTitleMouseOut({originalEvt: evt, vx: vx, vy: vy, dx: dx, dy: dy, title: title, series: self});} );
      g.addEventListener("click", function(evt) { self.chart().onSeriesPointTitleClick({originalEvt: evt, vx: vx, vy: vy, dx: dx, dy: dy, title: title, series: self});} );

      g.insertBefore(rcEl, pointTxtEl);

      if (this.showPointTitleLeg()) {
        var theta = (new WAVE.Geometry.Point(vx, vy)).toPolarPoint(titleRc.centerPoint()).theta();
        var ps = WAVE.Geometry.findRayFromRectangleCenterSideIntersection(titleRc, theta);
        var titleLine = WAVE.SVG.createLine(ps.x(), ps.y(), vx, vy);
        g.insertBefore(titleLine, rcEl);
      }

      parentEl.appendChild(g);
    }
  };

  Series.prototype.calcTitlePos = function(vx, vy, txtBox, area) {
    var pointMargin = 5;
    var left = null, top = null, x, y;
    var txtBoxS = txtBox.width * txtBox.height;

    x = vx + pointMargin; y = vy - pointMargin - txtBox.height;
    var overlapS = WAVE.Geometry.overlapAreaWH(area.left(), area.top(), area.width(), area.height(), x, y, txtBox.width, txtBox.height);
    if (overlapS !== txtBoxS) {
      x = vx - pointMargin - txtBox.width; y = vy - pointMargin - txtBox.height;
      overlapS = WAVE.Geometry.overlapAreaWH(area.left(), area.top(), area.width(), area.height(), x, y, txtBox.width, txtBox.height);
      if (overlapS !== txtBoxS) {
        x = vx + pointMargin; y = vy + pointMargin;
        overlapS = WAVE.Geometry.overlapAreaWH(area.left(), area.top(), area.width(), area.height(), x, y, txtBox.width, txtBox.height);
        if (overlapS !== txtBoxS) {
          x = vx - pointMargin - txtBox.width; y = vy + pointMargin;
          overlapS = WAVE.Geometry.overlapAreaWH(area.left(), area.top(), area.width(), area.height(), x, y, txtBox.width, txtBox.height);
          if (overlapS === txtBoxS) {
            left = x; top = y;
          }
        } else {
          left = x; top = y;
        }  
      } else {
        left = x; top = y;
      }  
    } else {
      left = x; top = y;
    }

    return new WAVE.Geometry.Point(left, top);
  };

  Series.prototype.drawPointCircle = function(x, y, dx, dy) {
    var radius = this.pointSize();
    var pointEl = WAVE.SVG.createCircle(x, y, radius, { class: this.pointClass() });

    this.chart().sZone().addVPoint( WAVE.Geometry.toRectWH(x-radius, y-radius, 2 *radius, 2 *radius));

    return pointEl;
  };

  Series.prototype.drawPointRect = function(x, y, dx, dy) {
    var rectSide = this.pointSize();
    var pointEl = WAVE.SVG.createRect(x-rectSide, y-rectSide, rectSide*2, rectSide*2, { class: this.pointClass() });

    this.chart().sZone().addVPoint( WAVE.Geometry.toRectWH(x-rectSide, y-rectSide, 2 *rectSide, 2 *rectSide));

    return pointEl;
  };

  Series.prototype.drawPointTriangle = function(x, y, dx, dy) {
    var triSide = this.pointSize();
    var d = "M" + (x-triSide) + "," + (y+triSide) + " H" + (x+triSide) + " L" + x + "," + (y-triSide) + " Z";
    var pointEl = WAVE.SVG.createPath({d: d, class: this.pointClass() });

    this.chart().sZone().addVPoint( WAVE.Geometry.toRectWH(x-triSide, y-triSide, 2 *triSide, 2 *triSide));

    return pointEl;
  };

  Series.prototype.toString = function () {
    return "Series " + this.title();
  };

  published.DataSet = function () {
    var self = this;

    WAVE.extend(self, WAVE.EventManager);

    var fPoints = [];
    var fPointWalkable = WAVE.arrayWalkable(fPoints);

    this.feedWalkable = function(walkable, qtyLimit) {
      if (!qtyLimit || qtyLimit > self.MAX_POINTS_QTY) qtyLimit = self.MAX_POINTS_QTY;
      self.removeAllPoints();
      var walker = walkable.getWalker();
      var i = 0;
      while(walker.moveNext() && i++ < qtyLimit) {
        var curr = walker.current();
        fPoints.push({x: curr.x, y: curr.y, t: curr.t});  
      }
      self.eventInvoke(EVT_CHANGED);
    };

    this.addPoint = function (x, y, title) {
      fPoints.push({x: x, y: y, title: title});
      self.eventInvoke(EVT_CHANGED);
    };

    this.forEach = function(action) {
      fPointWalkable.wEach(action);
    };

    this.removeAllPoints = function() {
      if (fPoints.length > 0) {
        fPoints.splice(0, fPoints.length);
        self.eventInvoke(EVT_CHANGED);
      }
    };

    this.removePointIf = function(condition) {
      var deleted;
      do {
        deleted = false;
        for(var i=0; i<fPoints.length; i++) {
          var p = fPoints[i];
          if (condition(p)) {
            fPoints.splice(i, 1);
            deleted = true;
            break;
          }
        }
      } while(deleted);
      self.eventInvoke(EVT_CHANGED);
    };

    this.removePointByX = function (x) {
      for (var i = 0; i < fPoints.length; i++) {
        var p = fPoints[i];
        if (p.x === x) {
          fPoints.splice(i, 1);
          break;
        }
      }
    };

    this.length = function () {
      return fPoints.length;
    };

    this.getPoint = function (idx) {
      return fPoints[idx];
    };

    this.getPointByX = function (x) {
      for (var i = 0; i < fPoints.length; i++) {
        var p = fPoints[i];
        if (p.x === x)
          return p;
      }
    };

    this.getMinX = function () {
      return fPointWalkable.wSelect(function(p) { return p.x;}).wMin();
    };

    this.getMaxX = function () {
      return fPointWalkable.wSelect(function(p) { return p.x;}).wMax();
    };

    this.getMinY = function () {
      return fPointWalkable.wSelect(function(p) { return p.y;}).wMin();
    };

    this.getMaxY = function () {
      return fPointWalkable.wSelect(function(p) { return p.y;}).wMax();
    };

    this.getNearestNeighbours = function(x) {
      
      if (fPoints.length === 0) { return null; }

      var pFirst = fPoints[0], pLast = fPoints[fPoints.length-1];

      if(x < pFirst.x) return {left: null, right: pFirst};

      if(x > pLast.x) return {left: pLast, right: null};

      var rightNeighbourIdx = fPointWalkable.wFirstIdx(function(p) { return x < p.x; });
      return {left: fPoints[rightNeighbourIdx-1], right: fPoints[rightNeighbourIdx]};
    };
  };

  published.DataSet.prototype.MAX_POINTS_QTY = 44100;

  var chartIdSeed = 0;

  published.Chart = function (svgEl) {
    var self = this;

    WAVE.extend(self, WAVE.EventManager);

    var fID = ++chartIdSeed;
    this.id = function() { return fID; };

    var fSvgEl = svgEl;
    this.svgEl = function() { return fSvgEl; };

    $(window).resize(function(){
      self.draw();
    });

    this.svgEl = function(val) {
      if (typeof(val) !== tUNDEFINED && val !== fSvgEl) { 
        fSvgEl = val;
        
        self.draw();
      }
      return fSvgEl;
    };

    var fXZone = new published.XZone(this);
    fXZone.eventBind(EVT_CHANGED, onChanged);
    this.xZone = function () { return fXZone; };

    var fYZone = new published.YZone(this);
    fYZone.eventBind(EVT_CHANGED, onChanged);
    this.yZone = function () { return fYZone; };

    var fSZone = new published.SZone(this);
    fSZone.eventBind(EVT_CHANGED, onChanged);
    this.sZone = function () { return fSZone; };

    var fLZone = new published.LZone(this);
    fLZone.eventBind(EVT_CHANGED, onChanged);
    this.lZone = function () { return fLZone; };

    var fXAxis = new published.XAxis(self);
    fXAxis.eventBind(EVT_CHANGED, onChanged);
    this.xAxis = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fXAxis) {
        fXAxis = val;
        self.draw();
      }
      return fXAxis;
    };

    var fYAxis = new published.YAxis(self);
    fYAxis.eventBind(EVT_CHANGED, onChanged);
    this.yAxis = function (val) {
      if (typeof (val) !== tUNDEFINED && val !== fYAxis) {
        fYAxis = val;
        self.draw();
      }
      return fYAxis;
    };

    var fUpdateQty = 0;
    var fIsChanged = true;

    this.beginUpdate = function() { fUpdateQty++; };

    this.endUpdate = function() {
      if (fUpdateQty > 0) fUpdateQty--;
      drawIfChanged();
    };

    this.beginUpdate();

    var fSeriesList = [];
    var fSeriesListWalkable = WAVE.arrayWalkable(fSeriesList);

    this.seriesListWalkable = function() { return fSeriesListWalkable; };

    this.getSeries = function(id) {
      return fSeriesListWalkable.wFirst(function(s) { return s.id() === id; });
    };

    this.addSeries = function(seriesCfg) {
      var s = new Series(self, seriesCfg);
      s.eventBind(EVT_CHANGED, onChanged);
      fSeriesList.push(s);
      onChanged();
      return s;
    };

    this.removeSeries = function(s) {
      var sIdx = fSeriesListWalkable.wFirstIdx(function(e) { return e === s; });
      if (sIdx !== -1) {
        fSeriesList.splice(sIdx, 1);
        s.eventUnbind(EVT_CHANGED, onChanged);
        onChanged();
      }
    };

    this.clearSeries = function() {
      if (fSeriesList.length === 0) return;
      for(var i in fSeriesList)
        fSeriesList[i].eventUnbind(EVT_CHANGED, onChanged);
      fSeriesList.splice(0, fSeriesList.length);
      onChanged();
    };

    this.draw = function () {
      var clientRc = fSvgEl.getBoundingClientRect();
      var width = clientRc.width, height = clientRc.height;

      var zoneAreas;
      try {
        zoneAreas = self.splitArea(fSvgEl, width, height);
      } catch (err) {
        console.error(err.message);
        return;
      }

      removeChildNodes(fSvgEl, true);

      var bkgrRc = WAVE.SVG.createRect(0, 0, width, height, { class: UICls.CSS_CLASS_SVG_BKGR });
      fSvgEl.appendChild(bkgrRc);

      self.xZone().draw(fSvgEl, zoneAreas.xZone);
      self.yZone().draw(fSvgEl, zoneAreas.yZone);
      self.sZone().draw(fSvgEl, zoneAreas.sZone);
      self.lZone().draw(fSvgEl, zoneAreas.lZone);

      var sZoneClientRc = WAVE.Geometry.toRectWH(
        zoneAreas.sZone.left() + clientRc.left,
        zoneAreas.sZone.top() + clientRc.top,
        zoneAreas.sZone.width(), zoneAreas.sZone.height());

      var rulerScopes = self.sZone().rulerScopes();
      if (rulerScopes.length > 0) {
        for(var irs in rulerScopes) {
          var rulerScope = rulerScopes[irs];

          // {element: , elementCfg: , scopeName: , scopeCfg: , cfg: {}}
          WAVE.GUI.rulerSet({element: fSvgEl, scope: rulerScope, elementCfg: { // {getTxt: , getMasterInfo: , getSlaveInfo: }
            getTxt: function (e) { // {clientPoint: , divHint: }
              var xAxis = self.xAxis();
              var yAxis = self.yAxis();
              var sZone = self.sZone();
              var lastArea = sZone.lastArea();

              if (sZoneClientRc.contains(e.clientPoint)) {
                var sZoneVX = e.clientPoint.x() - clientRc.left;
                var sZoneVY = e.clientPoint.y() - clientRc.top;

                var dataX = xAxis.v2d(sZoneVX);
                var dataY = yAxis.v2d(sZoneVY);

                e.divHint.style.visibility = "visible";
                return xAxis.labelValToStr(dataX) + "; " + yAxis.labelValToStr(dataY);
              } else {
                e.divHint.style.visibility = "hidden";
              }
            },

            getMasterInfo: function (e) { // {clientPoint: }
              var xAxis = self.xAxis();
              var yAxis = self.yAxis();
              var sZone = self.sZone();

              var sZoneVX = e.clientPoint.x() - clientRc.left;
              var sZoneVY = e.clientPoint.y() - clientRc.top;

              var dataX = xAxis.v2d(sZoneVX);
              var dataY = yAxis.v2d(sZoneVY);

              var r = { dataPoint: new WAVE.Geometry.Point(dataX, dataY) };
              r.isInParentRc = sZoneClientRc.contains(e.clientPoint);

              return r;
            },

            // e is {masterRes: {dataPoint: , isInParentRc: }}, 
            // returns {clientPoint: , isInParentRc: }
            getSlaveInfo: function (e) {
              var xAxis = self.xAxis();
              var yAxis = self.yAxis();
              var sZone = self.sZone();

              var dataX = e.masterRes.dataPoint.x(), dataY = e.masterRes.dataPoint.y();
              var cVX = xAxis.d2v(dataX) + clientRc.left, cVY = yAxis.d2v(dataY) + clientRc.top;
              var r = { clientPoint: new WAVE.Geometry.Point(cVX, cVY) };
              r.isInParentRc =
                (!sZone.rulerCheckXContains() || (r.clientPoint.x() >= sZoneClientRc.left() && r.clientPoint.x() <= sZoneClientRc.right())) &&
                (!sZone.rulerCheckYContains() || (r.clientPoint.y() >= sZoneClientRc.top() && r.clientPoint.y() <= sZoneClientRc.bottom()));
              return r;
            },

            parentRc: sZoneClientRc
          }});

        }
      } else {
        WAVE.GUI.rulerUnset({element: fSvgEl});
      }
    };

    function drawIfChanged() {
      if (fIsChanged && fUpdateQty === 0) {
        self.draw();
        fIsChanged = false;
      }
    }

    function onChanged() {
      fIsChanged = true;
      drawIfChanged();
    }

    function removeChildNodes(el) {
      for(var i=0; i<el.childNodes.length;) {
       var child = el.childNodes[i];
        if (child.nodeName !== "defs")
          el.removeChild(child);
        else
          i++;
      }
    }

    this.splitArea = function (svgEl, width, height) {
      var xZoneHeight = self.xZone().getHeight(svgEl);
      var yZoneWidth = self.yZone().getWidth(svgEl);

      var leftTop = new geo.Point(0, 0);
      var rightBottom = new geo.Point(Math.round(width), Math.round(height));

      if (xZoneHeight.top)
        leftTop.y(leftTop.y() + xZoneHeight.top);

      if (xZoneHeight.bottom)
        rightBottom.y(rightBottom.y() - xZoneHeight.bottom);

      if (yZoneWidth.left)
        leftTop.x(leftTop.x() + yZoneWidth.left);

      if (yZoneWidth.right)
        rightBottom.x(rightBottom.x() - yZoneWidth.right);

      var xZone = {
        top:    new geo.Rectangle(new geo.Point(leftTop.x(), 0),               new geo.Point(rightBottom.x(), leftTop.y())),
        bottom: new geo.Rectangle(new geo.Point(leftTop.x(), rightBottom.y()), new geo.Point(rightBottom.x(), height))
      };

      var yZone = {
        left:  new geo.Rectangle(new geo.Point(0,               leftTop.y()), new geo.Point(leftTop.x(),     rightBottom.y())),
        right: new geo.Rectangle(new geo.Point(rightBottom.x(), leftTop.y()), new geo.Point(width, rightBottom.y()))
      };

      var sZone = new geo.Rectangle(leftTop, rightBottom);

      if (leftTop.x() > 0) { 
        sZone.corner1().x(sZone.left() + 1); 
        xZone.top.corner1().x(sZone.left());
        xZone.bottom.corner1().x(sZone.left());
      }

      if (leftTop.y() > 0) { 
        sZone.corner1().y(sZone.top() + 1);
        yZone.left.corner1().y(sZone.top());
        yZone.right.corner1().y(sZone.top());
      }

      if (rightBottom.x() < width) {
        sZone.corner2().x(sZone.right() - 1);
        xZone.top.corner2().x(sZone.right());
        xZone.bottom.corner2().x(sZone.right());
      }

      if (rightBottom.y() < height) {
        sZone.corner2().y(sZone.bottom() - 1);
        yZone.left.corner2().y(sZone.bottom());
        yZone.right.corner2().y(sZone.bottom());
      }

      var lSize = self.lZone().getSize(svgEl);

      var lZoneX, lZoneY;
      switch (self.lZone().corner()) {
        case published.RectCorner.LEFTTOP:
          lZoneX = sZone.left() + self.lZone().legendMargin();
          lZoneY = sZone.top() + self.lZone().legendMargin();
          break;
        case published.RectCorner.RIGHTTOP:
          lZoneX = sZone.right() - self.lZone().legendMargin() - lSize.w;
          lZoneY = sZone.top() + self.lZone().legendMargin();
          break;
        case published.RectCorner.LEFTBOTTOM:
          lZoneX = sZone.left() + self.lZone().legendMargin();
          lZoneY = sZone.bottom() - lSize.h - self.lZone().legendMargin();
          break;
        case published.RectCorner.RIGHTBOTTOM:
          lZoneX = sZone.right() - self.lZone().legendMargin() - lSize.w;
          lZoneY = sZone.bottom() - lSize.h - self.lZone().legendMargin();
          break;
      }

      var lZone = new geo.Rectangle(new geo.Point(lZoneX, lZoneY), new geo.Point(lZoneX + lSize.w, lZoneY + lSize.h));

      var r = { xZone: xZone, yZone: yZone, sZone: sZone, lZone: lZone };

      return r;
    };

    this.endUpdate();
    
  };

  published.Chart.prototype.onSeriesMouseOver = function(seriesEvt) {
    this.eventInvoke(published.EVT_SERIES_POINT_MOUSEOVER, seriesEvt);
  };

  published.Chart.prototype.onSeriesMouseOut = function(seriesEvt) {
    this.eventInvoke(published.EVT_SERIES_POINT_MOUSEOUT, seriesEvt);
  };

  published.Chart.prototype.onSeriesClick = function(seriesEvt) {
    this.eventInvoke(published.EVT_SERIES_POINT_CLICK, seriesEvt);
  };

  published.Chart.prototype.onSeriesPointTitleMouseOver = function(seriesEvt) {
    this.eventInvoke(published.EVT_SERIES_POINT_TITLE_MOUSEOVER, seriesEvt);
  };

  published.Chart.prototype.onSeriesPointTitleMouseOut = function(seriesEvt) {
    this.eventInvoke(published.EVT_SERIES_POINT_TITLE_MOUSEOUT, seriesEvt);
  };

  published.Chart.prototype.onSeriesPointTitleClick = function(seriesEvt) {
    this.eventInvoke(published.EVT_SERIES_POINT_TITLE_CLICK, seriesEvt);
  };

  return published;
}());
