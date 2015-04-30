WAVE.Ideas = (function() {

  var published = {};

  var geometry = {};

  var undefined = "undefined";
  var pi = 3.14159265;
	var pi2 = 6.28318531;

  geometry.LineSegment = function(_x1, _y1, _x2, _y2) {
    var self = this;

    var fX1 = _x1;
    this.x1 = function(val) {
      if (typeof(val) !== undefined && val != fX1) {
        fX1 = val;
        sync();
      }
      return fX1;
    }

    var fY1 = _y1;
    this.y1 = function(val) {
      if (typeof(val) !== undefined && val != fY1) {
        fY1 = val;
        sync();
      }
      return fY1;
    }

    var fX2 = _x2;
    this.x2 = function(val) {
      if (typeof(val) !== undefined && val != fX2) {
        fX2 = val;
        sync();
      }
      return fX2;
    }

    var fY2 = _y2;
    this.y2 = function(val) {
      if (typeof(val) !== undefined && val != fY2) {
        fY2 = val;
        sync();
      }
      return fY2;
    }

    var fDx;
    this.dX = function() { return fDX; }

    var fDy;
    this.dX = function() { return fDY; }

    sync();

    var fDistance; // AKA ρ in polar coordinate system
    this.distance = function() { return fDistance; }

    var fTheta; // AKA θ in polar coordinate system
    this.theta = function() { return fTheta; }

    var fA;
    this.a = function() { return fA; }

    var fB;
    this.b = function() { return fB; }

    var fC;
    this.c = function() { return fC; }

    var fNA;
    this.nA = function() { return fNA; }

    var fNB;
    this.nB = function() { return fNB; }

    var fNC;
    this.nC = function() { return fNC; }


    this.parallelWith = function(other) { 
      var otherTheta = other.theta();
      return areEqual(fTheta, otherTheta) || areEqual(WAVE.Geometry.wrapAngle(fTheta, Math.PI), otherTheta);
    }

    this.onSameLineAs = function(other) { 
      return self.parallelWith(other) && fNC == other.nC();
    }

    this.onSameLineAndCollinearWith = function(other) { 
      return areEqual(fTheta, other.theta()) && fNC == other.nC();
    }

    // Can return:
    //  - Geometry.Point if segments intersects
    //  - Geometry.Segment if segments are on the same line (itself) and have common points
    //  - null if segments have no common points
    this.getSegmentsIntersection = function(other) {

      if (self.onSameLineAndCollinearWith(other)) {
          
        return s1;
      }

      if (s2.parallelWith(s2)) return null;

    }

    // assume that point belongs to the same line as this segment does
    function inSegment(xP, yP) {
    }

    function sync() {
      calcDeltas();
      calcABC();
      calcDistance();
      normilize();
    }

    function calcDeltas() {
      fDx = fX2 - fX1;
      fDy = fY2 - fY1;
    }

    function calcDistance() {
      fDistance = Math.sqrt(fDx * fDx + fDy * fDy);

      fTheta = Math.atan2(fDy, fDx);
			if (fTheta < 0)	fTheta += pi2;
    }

    function calcABC() {
      fA = fY1 - fY2;
      fB = fX2 - fX1;
      fC = -(fA * fX1 + fB * fY1);
    }

    function normilize() {
      var k = Math.sqrt(fA*fA + fB*fB);
      fNA = fA / k;
      fNB = fB / k;
      fNC = fC / k;
    }

    // Compares two numbers in respect that according to float representation precision 
    function areEqual(a, b) { 
      var r = a - b;
      return (r >= 0 && r < .00001) || (r < 0 && r > -.00001); 
    }
  }

  geometry.LineSegment.prototype.toString = function() {
    return "[(" + this.x1() + ", " + this.y1() + ") (" + this.x2() + ", " + this.y2() + ")]" 
      + " (" + this.a() + ", " + this.b() + ", " + this.c() + ")"
      + " (" + this.nA() + ", " + this.nB() + ", " + this.nC() + ")"
      + " (" + this.distance() + ")";
  }

  // represents matrix of 2D conversion.
  // initialy matrix doesn't contain any conversions
  geometry.Matrix = function() {
    var self = this;

    var a00 = 1, a01 = 0, a02 = 0, 
        a10 = 0, a11 = 1, a12 = 0, 
        a20 = 0, a21 = 0, a22 = 1;

    // calculate coordinates of incoming point "p" to coodinates described by this matrix
    this.convert = function(p) {
      var x = p.x(), y = p.y();

      var x1 = a00 * x + a01 * y + a02,
          y1 = a10 * x + a11 * y + a12;

      return new WAVE.Geometry.Point(x1, y1);
    }

    // offset matrix (new center of coordinates is (dx,dy))
    this.translate = function(dx, dy) {
      var c00 = a00;
      var c01 = a01;
      var c02 = a00 * dx + a01 * dy + a02;
      var c10 = a10;
      var c11 = a11;
      var c12 = a10 * dx + a11 * dy + a12;
      var c20 = a20;
      var c21 = a21;
      var c22 = a20 * dx + a21 * dy + a22;

      a00 = c00; a01 = c01; a02 = c02;
      a10 = c10; a11 = c11; a12 = c12;
      a20 = c20; a21 = c21; a22 = c22;

      return self;
    }

    // rotate matrix by theta radians relative to center of coordinates (0,0) 
    this.rotate = function(theta) {
      var sin = Math.sin(theta);
      var cos = Math.cos(theta);

      var c00 = a00 * cos + a01 * sin;
      var c01 = -a00 * sin + a01 * cos;
      var c02 = a02;
      var c10 = a10 * cos + a11 * sin;
      var c11 = -a10 * sin + a11 * cos;
      var c12 = a12;
      var c20 = a20 * cos + a21 * sin;
      var c21 = -a20 * sin + a21 * cos;
      var c22 = a22;

      a00 = c00; a01 = c01; a02 = c02;
      a10 = c10; a11 = c11; a12 = c12;
      a20 = c20; a21 = c21; a22 = c22;

      return self;
    }

    this.relativeRotate = function(theta, cx, cy) {
      self.translate(cx,cy);
      self.rotate(theta);
      self.translate(-cx,-cy);

      return self;
    }

    this.toString = function() {
      return a00 + " " + a01 + " " + a02 + "<br>" + a10 + " " + a11 + " " + a12 + "<br>" + a20 + " " + a21 + " " + a22 + "<br>";
    }
  };

  published.Geometry = geometry;

  return published;

} ());    

