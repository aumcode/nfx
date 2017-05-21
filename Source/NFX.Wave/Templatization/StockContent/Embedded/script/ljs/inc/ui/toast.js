var TOAST_ZORDER = 1000000;

var fToastCount = 0;

published.toast = function(msg, type, duration){
  var self = {};
  if (!WAVE.intValidPositive(duration)) duration = 2500;

  var div = document.createElement("div");
  var t = WAVE.strEmpty(type)? "" : "_"+type;
  div.className = published.CLS_TOAST + t;
  div.style.zIndex = TOAST_ZORDER;
  div.innerHTML = msg;
  div.style.left = 0;
  document.body.appendChild(div);
  var ml = Math.round(div.offsetWidth / 2);
  div.style.left = "50%";
  div.style.marginLeft = - ml + "px";
  fToastCount++;
  if (fToastCount>1){
    div.style.marginLeft =  - (ml + (fToastCount *  4)) + "px";
    div.style.marginTop = - (fToastCount *  4) + "px";
  }

  var fClosed = false;

  self.close = function(){
    if (fClosed) return false;
    document.body.removeChild(div);
    fToastCount--;
    fClosed = true;
    return true;
  };

  self.closed = function() { return fClosed;};

  setTimeout(function(){ self.close(); }, duration);
  return self;
};