published.EVT_SWIPE_LEFT = "wv-swipe-left";
published.EVT_SWIPE_RIGHT = "wv-swipe-right";
published.EVT_SWIPE_UP = "wv-swipe-up";
published.EVT_SWIPE_DOWN = "wv-swipe-down";

published.Swipe = function (init) {

  var swipe = this,
      xDown = null,
      yDown = null;

  WAVE.extend(swipe, WAVE.EventManager);

  function handleTouchStart(evt) {
    xDown = evt.touches[0].clientX;
    yDown = evt.touches[0].clientY;
  }

  function handleTouchMove(evt) {
    if (!xDown || !yDown)
      return;

    var xUp = evt.touches[0].clientX;
    var yUp = evt.touches[0].clientY;

    var xDiff = xDown - xUp;
    var yDiff = yDown - yUp;

    if (Math.abs(xDiff) > Math.abs(yDiff)) {
      if (xDiff > 0)
        swipe.eventInvoke(published.EVT_SWIPE_LEFT, { phase: published.EVT_PHASE_AFTER });
      else
        swipe.eventInvoke(published.EVT_SWIPE_RIGHT, { phase: published.EVT_PHASE_AFTER });
    } else {
      if (yDiff > 0)
        swipe.eventInvoke(published.EVT_SWIPE_UP, { phase: published.EVT_PHASE_AFTER });
      else
        swipe.eventInvoke(published.EVT_SWIPE_DOWN, { phase: published.EVT_PHASE_AFTER });
    }
    xDown = null;
    yDown = null;
  }

  /*init*/
  WAVE.addEventHandler(document, 'touchstart', handleTouchStart);
  WAVE.addEventHandler(document, 'touchmove', handleTouchMove);

  return swipe;
}
