published.curtainOn = function(cls, makeBodyUnscrollable, notBlur){
  if (!notBlur) try{ document.activeElement.blur(); } catch(e){}

  var div = document.createElement("div"),
      mbu = WAVE.strAsBool(makeBodyUnscrollable, true);
  div.style.backgroundColor = "rgba(127,127,127,0.45)";
  div.className = published.CLS_CURTAIN;
  if (!WAVE.strEmpty(cls))
    div.className += " " + cls;
  div.style.position = "fixed";
  div.style.left = "0";
  div.style.top = "0";
  div.style.width = "100%";
  div.style.height = "100%";
  div.style.zIndex = CURTAIN_ZORDER + fCurtains.length;
  div.mbu = mbu;
  document.body.appendChild(div);

  fCurtains.push(div);
  if (mbu) tryMakeBodyUnscrollable();
  return div;
};

published.curtainOff = function(){
  if (fCurtains.length===0) return;
  var div = fCurtains[fCurtains.length-1];
  if (typeof(div.__DIALOG)!==tUNDEFINED){
    div.__DIALOG.cancel();
    return;
  }

  document.body.removeChild(div);
  WAVE.arrayDelete(fCurtains, div);
  if (div.mbu === true) tryRetrunScrollingToBody();
};

published.isCurtain = function(){ return fCurtains.length>0; };