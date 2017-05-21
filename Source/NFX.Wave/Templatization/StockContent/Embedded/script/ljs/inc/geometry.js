var geometry = {
  EARTH_RADIUS_KM: 6371
};

var pi = 3.14159265;
var pi2 = 6.28318531;

geometry.PI = pi;
geometry.PI2 = pi2;

geometry.MapDirection = {
  North:     {Name: "North"},
  NorthEast: {Name: "NorthEast"},
  East:      {Name: "East"},
  SouthEast: {Name: "SouthEast"},
  South:     {Name: "South"},
  SouthWest: {Name: "SouthWest"},
  West:      {Name: "West"},
  NorthWest: {Name: "NorthWest"}
};

// Returns pixel distance between two points
geometry.distance = function(x1, y1, x2, y2) { return Math.sqrt(Math.pow(x1 - x2, 2) + Math.pow(y1 - y2, 2)); };

// Returns pixel distance between two points
geometry.distancePoints = function(p1, p2) {  return geometry.distance(p1.x(), p1.y(),  p2.x(), p2.y()); };


// Converts radians to degrees
geometry.radToDeg = function(rad){ return (rad / pi) * 180; };

// Converts defrees to rads
geometry.degToRad = function(deg){ return (deg / 180) * pi; };


// Returns azimuth angle (theta) in radians
geometry.azimuthRad = function(xc, yc, xd, yd) {
  var angle = Math.atan2(yd - yc, xd - xc);
  if (angle < 0) angle = pi2 + angle;
  return angle;
};

// Returns azimuth angle (theta) in radians for two points: center and destination
geometry.azimuthRadPoints = function(pc, pd) {  return geometry.azimuthRad(pc.x(), pc.y(),  pd.x(), pd.y()); };

// Returns azimuth angle (theta) in degrees
geometry.azimuthDeg = function(xc, yc, xd, yd){ return (geometry.azimuthRad(xc, yc, xd, yd) / pi2) * 360; };

// Returns azimuth angle (theta) in degrees for twho points: center and destination
geometry.azimuthDegPoints = function(pc, pd){ return geometry.azimuthDeg(pc.x(), pc.y(),  pd.x(), pd.y()); };

// Returns azimuth angle (theta) in radix units
geometry.azimuthOfRadix = function(xc, yc, xd, yd, radix) {
  if (radix < 2) radix = 2;
  var angle = geometry.azimuthRad(xc, yc, xd, yd);
  var half = pi / radix;
  angle = geometry.wrapAngle(angle, half);
  return Math.floor((angle / pi2) * radix);
};

// Returns azimuth angle (theta) in in radix units for twho points: center and destination
geometry.azimuthOfRadixPoints = function(pc, pd, radix){ return geometry.azimuthOfRadix(pc.x(), pc.y(),  pd.x(), pd.y(), radix); };

//Returns rectangle from coordinate pairs
geometry.toRectXY = function(x1,y1, x2,y2) {
  return new geometry.Rectangle( new geometry.Point(x1, y1), new geometry.Point(x2, y2) );
};

// Returns rectangle from coordinats and dimensions
geometry.toRectWH = function(x1,y1, w,h) {
  return new geometry.Rectangle( new geometry.Point(x1, y1), new geometry.Point(x1 + w, y1 + h) );
};

  // Returns area of overlap between two rectangles
geometry.overlapAreaRect = function(rect1, rect2) {
  var tl1 = rect1.topLeft();
  var tl2 = rect2.topLeft();
  return geometry.overlapAreaWH(
    tl1.x(), tl1.y(), rect1.width(), rect1.height(),
    tl2.x(), tl2.y(), rect2.width(), rect2.height()
  );
};

// Returns area of overlap between two rectangles expressed as top-left/width-height pairs
geometry.overlapAreaWH = function(x1,y1, w1,h1, x2,y2, w2,h2) {
  var ix;
  var iy;

  if (w2>=w1) {
    if (x1<=x2) ix = (x1+w1) - x2;
    else if ((x1+w1)>=(x2+w2)) ix = (x2+w2)-x1;
    else ix = w1;
  } else {
    if (x2<=x1) ix = (x2+w2) - x1;
    else if ((x2+w2)>=(x1+w1)) ix = (x1+w1)-x2;
    else ix = w2;
  }

  if (h2>=h1) {
    if (y1<=y2) iy = (y1+h1) - y2;
    else if ((y1+h1)>=(y2+h2)) iy = (y2+h2)-y1;
    else iy = h1;
  } else {
    if (y2<=y1) iy = (y2+h2) - y1;
    else if ((y2+h2)>=(y1+h1)) iy = (y1+h1)-y2;
    else iy = h2;
  }

  if (ix<0) ix = 0;
  if (iy<0) iy = 0;

  return ix*iy;
};

// Returns area of overlap between two rectangles expressed as top-left/bottom-right pairs
geometry.overlapAreaXY = function(left1, top1, right1, bott1, left2, top2, right2, bott2) {
  return geometry.overlapAreaWH(left1, top1, right1-left1, bott1-top1, left2, top2, right2-left2, bott2-top2);
};

// defines if line has common points with rect
geometry.lineIntersectsRect = function(rx1, ry1, rx2, ry2, lx1, ly1, lx2, ly2) {
  var a = ly1 - ly2,
      b = lx2 - lx1,
      c = -(a * lx1 + b * ly1);

  var r = a * rx1 + b * ry1 + c;
  var sign = r === 0 ? 0 : (r > 0 ? 1 : -1);

  r = a * rx1 + b * ry2 + c;
  if ((r === 0 ? 0 : (r > 0 ? 1 : -1)) !== sign) return true;

  r = a * rx2 + b * ry1 + c;
  if ((r === 0 ? 0 : (r > 0 ? 1 : -1)) !== sign) return true;

  r = a * rx2 + b * ry2 + c;
  if ((r === 0 ? 0 : (r > 0 ? 1 : -1)) !== sign) return true;

  return false;
};

//Returns area of line overlap with rect. Normalizes coordinate direction
geometry.intersectionLengthRectLineXY = function(rx1, ry1, rx2, ry2, lx1, ly1, lx2, ly2) {
  if (!geometry.lineIntersectsRect(rx1, ry1, rx2, ry2, lx1, ly1, lx2, ly2)) return 0;

  var x1, y1, x2, y2;

  if (lx1 < rx1) x1 = rx1; else if (lx1 > rx2) x1 = rx2; else x1 = lx1;
  if (ly1 < ry1) y1 = ry1; else if (ly1 > ry2) y1 = ry2; else y1 = ly1;
  if (lx2 < rx1) x2 = rx1; else if (lx2 > rx2) x2 = rx2; else x2 = lx2;
  if (ly2 < ry1) y2 = ry1; else if (ly2 > ry2) y2 = ry2; else y2 = ly2;

  return geometry.distance(x1, y1, x2, y2);
};

// Modifies an angle by delta value ensuring that resulting angle is always between 0 and 2pi
geometry.wrapAngle = function(angle, delta) {
  delta = delta % pi2;
  if (delta<0) delta = pi2 + delta;
  var result = angle + delta;
  return result % pi2;
};

//Converts map direction to angular coordinate in radians
geometry.mapDirectionToAngle = function(direction) {
  switch (direction) {
    case geometry.MapDirection.North: return 4/16 * pi2;
    case geometry.MapDirection.South: return 12/16 * pi2;
    case geometry.MapDirection.East:  return 0.0;
    case geometry.MapDirection.West:  return 8/16 * pi2;

    case geometry.MapDirection.NorthEast: return 2/16 * pi2;
    case geometry.MapDirection.NorthWest: return 6/16 * pi2;
    case geometry.MapDirection.SouthEast: return 14/16 * pi2;
    case geometry.MapDirection.SouthWest: return 10/16 * pi2;

    default: return 0.0;
    }
};

// Converts a radian angular coordinate into map direction
geometry.angleToMapDirection = function(angle) {
  angle = geometry.wrapAngle(angle, 0);
  if ((angle >= 0.0) && (angle < pi2 * 1/16)) return geometry.MapDirection.East;
  else if ((angle >= pi2 * 1/16) && (angle < pi2 * 3/16)) return geometry.MapDirection.NorthEast;
  else if ((angle >= pi2 * 3 / 16) && (angle < pi2 * 5 / 16)) return geometry.MapDirection.North;
  else if ((angle >= pi2 * 5 / 16) && (angle < pi2 * 7 / 16)) return geometry.MapDirection.NorthWest;
  else if ((angle >= pi2 * 7 / 16) && (angle < pi2 * 9 / 16)) return geometry.MapDirection.West;
  else if ((angle >= pi2 * 9 / 16) && (angle < pi2 * 11 / 16)) return geometry.MapDirection.SouthWest;
  else if ((angle >= pi2 * 11 / 16) && (angle < pi2 * 13 / 16)) return geometry.MapDirection.South;
  else if ((angle >= pi2 * 13 / 16) && (angle < pi2 * 15 / 16)) return geometry.MapDirection.SouthEast;
  else return geometry.MapDirection.East;
};

// Calculates a relative area of an inner rectangle that violates outside perimeter.
// The area is not 100% geometrically accurate - may be used for relative comparisons only
geometry.perimeterViolationArea = function(perimeter, inner) {
  var ix = 0;
  var iy = 0;

  if (inner.left()<perimeter.left()) ix = perimeter.left() - inner.left();
  else if (inner.right()>perimeter.right()) ix = inner.right() - perimeter.right();

  if (inner.top() < perimeter.top()) iy = perimeter.top() - inner.top();
  else if (inner.bottom() > perimeter.bottom()) iy = inner.bottom() - perimeter.bottom();

  return (ix * inner.height()) + (iy * inner.width());
};

// Returns a point of intersection between a ray cast from the center of a rectangle
// under certain polar coordinate angle and a rectangle side
geometry.findRayFromRectangleCenterSideIntersection = function(rect, theta) {
  var center = new geometry.Point(rect.left() + rect.width() / 2, rect.top() + rect.height() / 2);

  //make ray "infinite" in comparison to rect
  var rayLength = rect.width() > rect.height() ? rect.width() : rect.height();

  if (rayLength < 100) rayLength = 100; //safeguard

  var rayEnd = new geometry.PolarPoint(rayLength, theta);//create ray "end" point
  var rayEndPoint = rayEnd.toPoint();

  //get line incline	aka. y = kx
  var k = (rayEndPoint.x() !== 0)? (rayEndPoint.y()) / (rayEndPoint.x()) : 0;

  var x = 0;
  var y = 0;

  var lst = [];

  //north
  x = center.x() + ((k !== 0) ? ((rect.top() - center.y()) / k) : 0);
  y = rect.top();
  if ((x >= rect.left()) && (x <= rect.right())) lst.push(new geometry.Point(x, y));

  //south
  x = center.x() + ((k !== 0) ? ((rect.bottom() - center.y()) / k) : 0);
  y = rect.bottom();
  if ((x >= rect.left()) && (x <= rect.right())) lst.push(new geometry.Point(x, y));

  //east
  x = rect.right();
  y = center.y() + k * (rect.right() - center.x());
  if ((y >= rect.top()) && (y <= rect.bottom())) lst.push(new geometry.Point(x, y));

  //west
  x = rect.left();
  y = center.y() + k * (rect.left() - center.x());
  if ((y >= rect.top()) && (y <= rect.bottom())) lst.push(new geometry.Point(x, y));

  var minPoint = new geometry.Point(1000000, 1000000);
  var minDistance = 1000000;

  var re = rayEnd.toPoint(); //rayEnd is relative to absolute 0,0
  re.offset(center.x(), center.y()); // need to make relative to rectangle center

  //find closest point
  for(var i in lst) {
    var p = lst[i];
    var dst = geometry.distancePoints(p, re);
    if (dst < minDistance) {
      minPoint = p;
      minDistance = dst;
    }
  }
  return minPoint;
};

// Calculates coordinates of rectangle given by width/height ('rw'/'rh')
// which center is rotated by 'angle' (in radians) relatively to point ('cx'/'cy')
// respecting reatangle size and 'margin'.
// Returns coordinates of rotated rectangle (geometry.Rectangle object)
geometry.rotateRectAroundCircle = function (cx, cy, cMargin, rw, rh, angle) {
  var halfRw = rw / 2, halfRh = rh / 2;
  var rcX = cx + Math.cos(angle) * (halfRw + cMargin);
  var rcY = cy + Math.sin(angle) * (halfRh + cMargin);
  return new geometry.toRectXY(rcX - halfRw, rcY - halfRh, rcX + halfRw, rcY + halfRh);
};

// Returns 2D bounding box (with  for a set of points (array of {x, y})
geometry.getBBox = function(points) {
  if (!points) return null;
  var length = points.length;
  if (length === 0) return null;
  var minX = Number.POSITIVE_INFINITY, maxX=Number.NEGATIVE_INFINITY, minY=Number.POSITIVE_INFINITY, maxY=Number.NEGATIVE_INFINITY;
  for(var i=0; i<length; i++) {
    var p = points[i];
    var x = p.x, y = p.y;
    if(x < minX) minX = x;
    if(x > maxX) maxX = x;
    if(y < minY) minY = y;
    if(y > maxY) maxY = y;
  }
  return geometry.toRectXY(minX, minY, maxX, maxY);
};

// Point class represents x,y pair on a cartesian plane
geometry.Point = function(x, y)
  {
  var fX = x;
  this.x = function(val){
          if (typeof(val)!==tUNDEFINED) fX = val;
          return fX;
      };

  var fY = y;
  this.y = function(val){
          if (typeof(val)!==tUNDEFINED) fY = val;
          return fY;
      };

  // Changes point coordinates
  this.offset = function(dx, dy) { fX += dx; fY += dy; };

}; // Point


// Returns point as polar point relative to the specified center
geometry.Point.prototype.toPolarPoint = function(center) {
    var dist = geometry.distancePoints(center, this);
    var angle = Math.atan2(this.y() - center.y(), this.x() - center.x());

    if (angle < 0)	angle = pi2 + angle;

    return new geometry.PolarPoint(dist, angle);
};


//Determines whether the two points contain equivalent coordinates
geometry.Point.prototype.isEqual = function(other){
  return (this.x() === other.x()) && (this.y() === other.y());
};

geometry.Point.prototype.toString = function()
{
  return "(" + this.x().toString() + " , " + this.y().toString() + ")";
};


// Polar Point class represents point with polar coordinates
geometry.PolarPoint = function(radius, theta)
{
  var fRadius = radius;
  this.radius = function(val){
        if (typeof(val)!==tUNDEFINED) fRadius = val;
        return fRadius;
  };

  var fTheta = checkangle(theta);
  this.theta = function(val){
        if (typeof(val)!==tUNDEFINED) fTheta = checkangle(val);
        return fTheta;
  };

  //Returns polar point as simple x,y point
  this.toPoint = function(){
      var x = fRadius * Math.cos(fTheta);
      var y = fRadius * Math.sin(fTheta);
      return new geometry.Point(x, y);
  };

  function checkangle(angle){
    if ((angle < 0) || (angle > pi2))
        throw "Invalid polar coordinates angle";
    return angle;
  }


};//PolarPoint

geometry.PolarPoint.prototype.isEqual = function(other){
    return this.radius() === other.radius() && this.theta() === other.theta();
};

geometry.PolarPoint.prototype.toString = function(){
  return "(" + this.radius().toString() + " , " + geometry.radToDeg(this.theta()).toString() + "°)";
};

// Represents a rectangle
geometry.Rectangle = function(corner1, corner2)
{
  var self = this;

  var fCorner1 = corner1;
  var fCorner2 = corner2;

  this.corner1 = function(val){
      if (typeof(val)!==tUNDEFINED) fCorner1 = val;
      return fCorner1;
  };

  this.corner2 = function(val){
      if (typeof(val)!==tUNDEFINED) fCorner2 = val;
      return fCorner2;
  };

  // Returns top left corner point per natural axis orientation when x increases from left to right, and y increases from top to bottom
  this.topLeft = function(){
      var lx = fCorner1.x();
      var other = fCorner2.x();

      if (other < lx) lx = other;

      var ty = fCorner1.y();
      other = fCorner2.y();

      if (other < ty) ty = other;
      return new geometry.Point(lx, ty);
    };

  // Returns bottom right corner point per natural axis orientation when x increases from left to right, and y increases from top to bottom
  this.bottomRight = function(){
      var rx = fCorner1.x();
      var other = fCorner2.x();

      if (other > rx) rx = other;

      var by = fCorner1.y();
      other = fCorner2.y();

      if (other > by) by = other;

      return new geometry.Point(rx, by);
  };

  // Return rectangle width
  this.width = function(){  return Math.abs(fCorner1.x() - fCorner2.x()); };

  // Return rectangle height
  this.height = function(){  return Math.abs(fCorner1.y() - fCorner2.y()); };

  // Returns left-most edge coordinate
  this.left = function() {
    var lx = fCorner1.x();
    var other = fCorner2.x();

    if (other < lx) lx = other;

    return lx;
  };

  // Returns right-most edge coordinate
  this.right = function(){
    var rx = fCorner1.x();
    var other = fCorner2.x();

    if (other > rx) rx = other;

    return rx;
  };

  // Returns top-most edge coordinate
  this.top = function(){
    var ty = fCorner1.y();
    var other = fCorner2.y();

    if (other < ty) ty = other;

    return ty;
  };

  // Returns bottom-most edge coordinate
  this.bottom = function(){
    var by = fCorner1.y();
    var other = fCorner2.y();

    if (other > by) by = other;

    return by;
  };

  // Returns the center point
  this.centerPoint = function(){
    return new geometry.Point(self.left() + self.width() / 2, self.top() + self.height() / 2);
  };

  // Returns rectangle square
  this.square = function () { return self.width() * self.height(); };

  // Tests if point lies within this reactangle
  this.contains = function (point) {
    var topLeft = self.topLeft(), bottomRight = self.bottomRight();
    var px = point.x(), py = point.y();
    return px >= topLeft.x() && px <= bottomRight.x() && py >= topLeft.y() && py <= bottomRight.y();
  };
};//Rectangle class

geometry.Rectangle.prototype.isEqual = function(other){
    return this.left() === other.left() && this.top() === other.top() && this.width() === other.width() && this.height() === other.height();
};

geometry.Rectangle.prototype.toString = function(){
  return "(" + this.left().toString() + "," + this.top().toString() + " ; "+ this.width().toString() + "x" + this.height().toString()+")";
};


/**
* Calculates callout balloon vertices suitable for contour curve drawing
*
* @param {Rectangle} body Balloon body coordinates
* @param {Point} target Balloon leg attachment point
* @param {double} legSweep Length of balloon leg attachment breach at balloon body edge, expressed in radians (arc length). A value such as PI/16 yields good results
* @returns {array} An array of vertex points
*/
geometry.vectorizeBalloon = function(body, target, legSweep)
{
  var result = [];

  var center = new geometry.Point(body.left() + body.width() / 2, body.top() + body.height() / 2);
  var trg = target.toPolarPoint(center);

  var legStart = geometry.findRayFromRectangleCenterSideIntersection(
                              body,
                              geometry.wrapAngle(trg.theta(), -legSweep / 2));

  var legEnd = geometry.findRayFromRectangleCenterSideIntersection(
                              body,
                              geometry.wrapAngle(trg.theta(), +legSweep / 2));

  //build vertexes
  result.push(new geometry.Point(body.left(), body.top()));

  result.push(new geometry.Point(body.right(), body.top()));

  result.push(new geometry.Point(body.right(), body.bottom()));

  result.push(new geometry.Point(body.left(), body.bottom()));

  result.push(legStart);
  result.push(target);
  result.push(legEnd);

  //reorder points by azimuth so the curve can close and look like a balloon
  result.sort(function(p1, p2)
                  {
                    var pp1 = p1.toPolarPoint(center);
                    var pp2 = p2.toPolarPoint(center);

                    if (pp1.theta() > pp2.theta())
                      return -1;
                    else
                      return +1;
                  }
              );

  return result;
};


  published.Geometry = geometry;