# WAVE.Geometry

## Properties

* EARTH_RADIUS_KM : 6371
* PI : 3.14159265
* PI2 : 6.28318531
* MapDirection:   
{
  North:     {Name: "North"},
  NorthEast: {Name: "NorthEast"},
  East:      {Name: "East"},
  SouthEast: {Name: "SouthEast"},
  South:     {Name: "South"},
  SouthWest: {Name: "SouthWest"},
  West:      {Name: "West"},
  NorthWest: {Name: "NorthWest"}
}


## Functions

##### angleToMapDirection(double angle): MapDirection
Converts a radian angular coordinate into map direction
##### azimuthDeg(double xc, double yc, double xd, double yd): double
Returns azimuth angle (theta) in degrees.
##### azimuthDegPoints(Point pc, Point pd)
Returns azimuth angle (theta) in degrees for two points: center and destination.
##### azimuthOfRadix(double xc, double yc, double xd, double yd, int radix): int
Returns azimuth angle (theta) in radix units.
##### azimuthOfRadixPoints(Point pc, Point pd, int radix): int
Returns azimuth angle (theta) in in radix units for two points: center and destination . 
##### azimuthRad(double xc, double yc, double xd, double yd): double
Returns azimuth angle (theta) in radians.
##### azimuthRadPoints(Point pc, Point pd)
Returns azimuth angle (theta) in radians for two points: center and destination.
##### degToRad(int deg): double
Converts degrees to rads.
##### distance(double x1, double y1, double x2, double y2): double
Returns pixel distance between two points.
##### distancePoints(Point p1, Point p2): double
Returns pixel distance between two points.
##### findRayFromRectangleCenterSideIntersection(Rectangle rect, double theta): Point
Returns a point of intersection between a ray cast from the center of a rectangle under certain polar coordinate angle and a rectangle side.
##### getBBox(points): Rectangle
Returns 2D bounding box (with for a set of points (array of {x, y}).
##### lineIntersectsRect(double rx1, double ry1, double rx2, double ry2, double lx1, double ly1, double lx2, double ly2): bool
Defines if line has common points with rectangle (top-left/bottom-right coordinates).
##### mapDirectionToAngle(MapDirection direction): double
Converts map direction to angular coordinate in radians.
##### overlapAreaRect(Rectangle rect1, Rectangle rect2): double
Returns area of overlap between two rectangles.
##### overlapAreaWH(double x1, double y1, double w1, double h1, double x2, double y2, double w2, double h2): double
Returns area of overlap between two rectangles expressed as top-left/width-height pairs.
##### overlapAreaXY(double left1, double top1, double right1, double bott1, double left2, double top2, double right2, double bott2): double
Returns area of overlap between two rectangles expressed as top-left/bottom-right pairs.
##### perimeterViolationArea(Rectangle perimeter, Rectangle inner): double
Calculates a relative area of an inner rectangle that violates outside perimeter. The area is not 100% geometrically accurate - may be used for relative comparisons only.
##### radToDeg(double rad): double
Converts radians to degrees.
##### rotateRectAroundCircle(double cx, double cy, double cMargin, double rw, double rh, double angle): Rectangle
Calculates coordinates of rectangle given by width/height (`rw/`rh`) which center is rotated by `angle` (in radians) relatively to point (`cx`/`cy`) respecting rectangle size and `cMargin`. Returns coordinates of rotated rectangle.
##### toRectWH(double x1, double y1, double w, double h): Rectangle
Returns rectangle from coordinates and dimensions (width and height).
##### toRectXY(double x1, double y1, double x2, double y2): Rectangle
Returns rectangle from coordinate pairs.
##### vectorizeBalloon(Rectangle body, Point target, double legSweep)
Calculates call out balloon vertices suitable for contour curve drawing. Returns an array of vertex points.
* body - Balloon body coordinates.
* target - Balloon leg attachment point
* legSweep - Length of balloon leg attachment breach at balloon body edge, expressed in radians (arc length). A value such as PI/16 yields good results.
##### wrapAngle(double angle, double delta): double
Modifies an angle by delta value ensuring that resulting angle is always between 0 and 2pi.


## Classes

### Point
Represents x,y pair on a Cartesian plane.
##### Point(double x, double y)
Constructor.
##### isEqual(Point p): bool
Determines whether the two points contain equivalent coordinates.
##### offset(double dx, double dy)
Changes point coordinates.
##### toPolarPoint(Point center): PolarPoint
Returns point as polar point relative to the specified center.
##### toString(): string
Returns coordinates in string like: `(10 , 12)`.
##### x(double val): double
Sets/returns X coordinate.
##### y(double val): double
Sets/returns Y coordinate.

### PolarPoint
Represents point with polar coordinates.
##### PolarPoint(double radius, double theta)
Constructor.
##### isEqual(PolarPoint p): bool
Determines whether the two points contain equivalent coordinates.
##### radius(double val): double
Sets/returns radius of point.
##### theta(double val): double
Sets/returns angle theta of point.
##### toPoint(): Point
Returns polar point as simple Cartesian (x,y) point.
##### toString(): string
Returns coordinates in string like: `(1 , 45°)`.

### Rectangle
Represents a rectangle.
##### Rectangle(Point corner1, Point corner2)
Constructor.
##### bottom(): double
Returns bottom-most edge coordinate.
##### bottomRight(): Point
Returns bottom right corner point per natural axis orientation when X increases from left to right, and Y increases from top to bottom.
##### centerPoint(): Point
Returns center point.
##### contains(Point p): bool
Tests if point lies within this rectangle.
##### corner1(Point val): Point
Sets/returns top left corner of rectangle.
##### corner2(Point val): Point
Sets/returns bottom right corner of rectangle.
##### height(): double
Returns rectangle height.
##### left(): double
Returns left-most edge coordinate.
##### right(): double
Returns right-most edge coordinate.
##### square(): double
Returns rectangle square.
##### top(): double
Returns top-most edge coordinate.
##### topLeft(): Point
Returns top left corner point per natural axis orientation when X increases from left to right, and Y increases from top to bottom.
##### width(): double
Returns rectangle width.


