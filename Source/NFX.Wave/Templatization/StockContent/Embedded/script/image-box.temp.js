// constants

// classes               
var IBOX_FRAMEDIV = "wvIBoxFrameDiv";
var IBOX_MAINIMAGEDIV = "wvIBoxMainImageDiv";
var IBOX_THUMBCONTAINERDIV = "wvIBoxThumbContainerDiv";
var IBOX_THUMBNAVIGATIONDIV = "wvIBoxThumbNavigationDiv";
var IBOX_THUMBIMAGESCONTAINERDIV = "wvIBoxThumbImagesContainerDiv";
var IBOX_THUMBIMAGEDIV = "wvIBoxThumbImageDiv";
var IBOX_THUMBIMAGEDIV_MOUSEOVER = "wvIBoxThumbImageDivMouseOver";
var IBOX_THUMBIMAGEDIV_MOUSEOUT = "wvIBoxThumbImageDivMouseOut";

// events
var EVT_IMAGE_CHANGED = 'image-changed';
var EVT_MAINIMAGE_MOUSEOVER = 'main-image-mouse-over';
var EVT_MAINIMAGE_MOUSEOUT = 'main-image-mouse-out';
var EVT_THUMBIMAGE_MOUSEOVER = 'thumb-image-mouse-over';
var EVT_THUMBIMAGE_MOUSEOUT = 'thumb-image-mouse-out';

// defaults
var DEFAULT_THUMB_IMG =
  "data:image/png;base64," +
  "iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAYAAADDPmHLAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJQ" +
  "AAFiUBSVIk8AAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAABG1JREFUeF7tnQtu2zAQ" +
  "RN3eMCfNuXKK1ttGAEOQsijvjzvzgCCNK1PizuPq0xj99fX19edBYPn9/Z2AQgHAoQDgUABweBG4CR8fH9" +
  "9/ejw+Pz//fR+9tgoFeNIW8uBuQa24KsDqXHgKAIcCdMhqybb6V1g9fgoADq8BnpxdTI3OqcKxXf/efvv+" +
  "tdXxD0ZjXB23/7sWdoATZuHMGG3fv9b+fDb+6r7vQgEm9AHIKjpbSQejba6+r9/OQwIKcJEjjFFQB6PXZ9" +
  "sKx1jtNmfbW0ABJoyCEAm0V+UxZvvlCQU4YbYatULyDnsEBXjB0aJ7Gd4Nr3//bD/WUIAJElAbktdq9drP" +
  "AQV4QS/CgfZKne3HGgpwA43wZ2N4nwL4JPBJu/K8A7Di6pzYATqiWrEWq8dPAcDhKQAcdgBwKAA4FAAcCg" +
  "AOBQCHAoBDAcChAN/s/gTwLhTgSRs8mgTwAowCR5IAWoCzoFEkgBXgSsAIEkAKsBJsdQngBLgTaGUJoAR4" +
  "J8iqEsAIoBFgRQlgLwLJfyAE0Fy51boAOwA45QWwWLGVugA7ADgUABwKAA4FAIcCgEMBwEn70bCzW62VT/" +
  "Ba3bJpHUP0p5FTCXAnrFcFjBLAYi4WhAugFdCseN4CWM9Hm7BrACmUZjhWQa+gPR+POYUIYDWx0bgWK2k0" +
  "puecNCl3F+Cxanoi9qlFydvAPhDNLtCPtXP4QtnnAB7B7B6+UFYAoQ1Iowu0Y1QIXwgRQLMlv0JLgqjwrW" +
  "tVugOM6AsqP4++WvqfKxH2IMi7hWqFuOtxzwjrAJVXlRYeNYI7BZCfhArALjDHqzYQHUCzmNWkDRfAuqAW" +
  "4+94zDNKdwDLQnqGZEkKAXZcpcKux92SpgNUWVHvEFGDVKcArQJ4FnLHY25Jdw0ghYgqRgTR803/H0asPn" +
  "qNKuYux9mT/i4geoVok20+6TsAsSV9ByC2bNMBrp5jo9pr9uObkb4DSGFXL7Ayk20+aTvAO0XyXmU7HWtP" +
  "ug6QbYVYEz3fVAJoFcKzoDsec0saASJXQRYiapBCAIuJexRz1+NuSX8X8A6WxfQOyopwAawLueMq9ZSrdA" +
  "c40CyoZzgehApQrZiaeNUGogOQOWECcPW/xqNGEB2gf9wqhT2+Zsy2yfaPOe8C8eHQNrS7+9UY4w7WwoV0" +
  "gN3CF9r3enYB61qVPgVor9ooCSwpK4BHQBUkKClAH4xmG+3H2l2CcgJEBLKzBCECWBVsNK7FRdRoTM85aR" +
  "LWAWRimpOzLtQVtOfjMadUvxN4Z7W+KpJFBxAs9usReM+WvxS6UqgoAVq05mLBNp8LuEsGATJT9jkAuQYF" +
  "AIcCgEMBwKEA4FAAcMoLYHG7VuUWUGAHAAdCAM0VW2n1C+wA4MAIoLFyq61+AaoDvBNgxfAFuFPAnSCrhi" +
  "9AXgOsBFo5fAH2IvBKsNXDF2AFEM4CRghfgBZAGAWNEr4AL4DQBo4UvlD+V8LIOewA4FAAcCgAOBQAHAoA" +
  "DgUAhwKAQwGgeTz+AsZuM9oZmv+HAAAAAElFTkSuQmCC";
var DEFAULT_MAIN_IMG =
  "data:image/png;base64," +
  "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJQ" +
  "AAFiUBSVIk8AAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAACL1JREFUeF7t3AFyIjcU" +
  "BNDdXHHP6YP5FJuQmBRmYRiGYaT/+70qV1yVtZmR1C2BbX5+fn7+/gFE+uvrv0AgBQDBFAAEUwAQTAFAMA" +
  "UAwRQABFMAEEwBQDC/CUg5v379+vrsu4+Pj6/P9nHrcfZ+jNEUAEPdC/O1y+AdUQBL13V0CWwZo7U8BYBg" +
  "CgCCKQAI5jUAhtryQtsRrwGcbLm2o+x1bU4AcMcpUNcf3SgACKYAIJgCgGBeBGSod70IeO/fnKx5Ln/U12" +
  "+5/5OtX3fNCYBWTsFYCt/Jmn+z1ejHf5YCINbeIZwl1M9QALSxJYB7hXbkY79CAdDW6Tnx+WOkWa7jFgVA" +
  "S9dhWwrfqzvx0tc/cx0jKADauReyo8M3y3UsUQAQTAHQyqPd9ajdd5breEQBQDAFAMEUAARTALTy6Ed6R/" +
  "3yzSzX8YgCgGAKgHbu7a5H77qzXMcSBUBL1yFbCt2rP5Jb+vpnrmMEBUBbp7CdP0aa5TpuUQBE2+sXcmb5" +
  "xZ5nKQDaeDaEe4d29ONvoQBo5RSqR8Fa82+2Gv34z/KegAx163nxLOE4wtb732vcnABgkBleFHQCYKi1IZ" +
  "j9VHC+j7XXuXTf19/jnWPkBAA7OoV1KbCP/v/RRacA4A3OQb/+mI0CgEmMeJrjNQDYwau7+4jwnzgBwGCj" +
  "wn/iBAA7WnsSGBn6SwoAgnkKAMEUAARTABBMAUAwBQDBFAB/mPXXVtmfAuCby+Argf4UAP+7FXgl0JsC4F" +
  "9LQVcCfSkAVgVcCfSkAMI9E2wl0I8CCLYl0EqgFwUQ6pUgK4E+FECgPQKsBHpQAGH2DK4SqE8BBHlHYJVA" +
  "bQogxDuDqgTqUgABjgioEqhJATR3ZDCVQD0KoLERgVQCtSiApkYGUQnUoQAamiGASqAGBdDMTMFTAvNTAB" +
  "BMATQy447rFDA3BQDBFEATM++0TgHzUgAQTAE0UGGHdQqYkwKAYAqguEo7q1PAfBQABFMAEEwBQDAFAMEU" +
  "AARTABBMAUAwBQDBFAAEUwAQTAFAMAUAwRQABPv5+fn5++tzVtj6F20fHx9fn+2r2l/YzTYO77qeKhTACn" +
  "uHbM9Fl1wAM89LFQrgjqOC9eqiSyuAKvNShQK4MDJMWxdcSgFUnJsK4gtgtgA9u9i6F0D1+Zld7E8BTgtr" +
  "xvBUC/Q7zTo/neYorgAqTGCnBbZVhTnqME9RBVBpwpJLwDwdJ6YAKk7Ummuu9Jx0zbV2nadZ+U3AyVXfYZ" +
  "6RdK+zUAAFPApGhVPAo2sU/jEUQBGdAyL84yiAQpaCMvMpYOnahH8sBVBMp8AI/3gKoKB7wZnxFHDvmoR/" +
  "DgqgqMoBEv55xBTAzM+Rt7oVpJnu89a1dAx/5bXlBFDcrCWQEv7qFEADs5WA8NehABobUQIznD5YL+r9AL" +
  "rvQvfC9+i+14Z26/dJHfcKok4AqbvT0n0/MyZ7fZ9Oqt933DsC2Y2OZbzn5jUACBZXAKlHVfbXYS05AUCw" +
  "yAJwCuBVXdaQEwAEiy0ApwC26rR2nAAgWHQBdDsFzHg/xnhuTgBNzLwwu4Wmk/gC6LA4K9yDcZ6TE0BxlR" +
  "ZlxwBVpwD+UXVhVrxuYz0XBVBU5QXZNUwVKYAvlRZlhwAZ7zkogAsVJrrTYjTe4ymAK90nnPUS1oICuEEJ" +
  "kLIGFMAdMy6AjovSOI+lABYkLQT+kzbnce8JuNXo97brvjCN7xhOACudFogTQT/p8+oEsNGRO1bKAjWmx3" +
  "MC2Ch956jKvH3nBLCTd+1eaYvVOB7LCWAndpY5mZdlTgAQzAkAgikACKYAIJjXAHa296vYaS9gGb9jOQHs" +
  "5LRw3/UjLLYzL8ucAF50xOJK2cWM5fGcADays9Rk3r5zAnjSqMXTfecyrmM4Aaxk5+gpfV6dAB6YaXF03a" +
  "2M8ThOAAvs+HnS5lwB3DHjQui4OI3zWArghrRdgD+lrAEFcEX4OUtYCwrgQoUJ77Qojfd4CuBLpYnusCiN" +
  "9xwUQFGVF2XnQFWjAP5RdUFWvG5jPRcFUFylhdk1RJXFF0CHRVnhHozznJwAmph5cXYMThfRBdBtYc54P8" +
  "Z4bk4AECy2ABxL2arT2nECgGCRBWD351Vd1pATAASLKwC7P3vpsJacACBY3HsCdj4BLL2f3b37fvY98LZ8" +
  "n9QxryDqBNB5IS5Zuu9nxmSv79NJ9fuOOgF0XqT3dqKj73mW6zhS5VOA1wAaGxG6zkHvSAE0cGsHGhnEW4" +
  "9d/blyVwqguNnCf6YEaogpgI5H01nDf5ZSApXXlhNAUZWD5CQwDwVQ0L0AzbgT3bsmJTAHBVBMp+AogfEU" +
  "QCFLgZn5eejStSmBsRRAEZ2DogTGUQAFPApIhVehH12jEhhDAUwuKRhK4HgxBVBxca255gq7/9maa+06T7" +
  "OKOgFUmqjKi+pV5uk4cU8BThM2+6Qlh/+swhx1mKfY1wBmncAOi2ovs85PpzmKe0egJSOfT29dVJVeAzip" +
  "eJ+dAn9NAdxx1IJ7dXGlFMBZlXmpQgGssPei23NxpRXApZnnpQoF8KSti+5diyu5AC7NNi9VKIDiFACv8J" +
  "uAEEwBQDAFAMEUAARTABBMAUAwBQDBFAAEUwAQTAFAMAUAwRQABFMAEEwBFFfpr+v8JeB8FAAEUwANVNhZ" +
  "7f5zUgAQTAE0MfMOa/eflwKAYAqgkRl3Wrv/3BQABFMAzcy049r956cAGpoheMJfgwJoamQAhb8OBdDYiC" +
  "AKfy0KoLkjAyn89SiAAEcEU/hrUgAh3hlQ4a9LAQR5R1CFvzYFEGbPwAp/fQog0B7BFf4eFECoVwIs/H0o" +
  "gGBbgiz8vSiAcM8EWvj7UQCsCrbw96QA+NdSwIW/LwXA/24FXfh7UwB8cxl44e/v5+fn5++vz4EwTgAQTA" +
  "FAMAUAwRQABFMAEEwBQDAFAMEUAARTABBMAUAwBQDBFAAEUwAQTAFAMAUAwRQABFMAEEwBQDAFAMEUAART" +
  "ABBMAUCsHz/+Bl9F0uTx0fb0AAAAAElFTkSuQmCC";
var DEFAULT_UP_NAVIGATION_IMG =
  "data:image/png;base64," +
  "iVBORw0KGgoAAAANSUhEUgAAACAAAAAQCAYAAAB3AH1ZAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJA" +
  "AAFiQBmxXGFAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAIlJREFUSEu9zlsOgCAQ" +
  "Q1EXxv63NQaTSYZ6QV7ycT6s0HKZ2bSUkmWaj8Cwh4+vPgLDLzru9FwPDFtoONLzXzCsoUGi91owJDTUov" +
  "drMFQ00EN7CIYRFY/QPoWho8IZ2hthmFHRCu13GFLBDrqTvQK6uJPuFR904Q9x8/i4Kx5AB054HkA/zkl2" +
  "A3fzm51wX0ZlAAAAAElFTkSuQmCC";
var DEFAULT_DOWN_NAVIGATION_IMG =
  "data:image/png;base64," +
  "iVBORw0KGgoAAAANSUhEUgAAACAAAAAQCAYAAAB3AH1ZAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJA" +
  "AAFiQBmxXGFAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAIxJREFUSEvFzkEOgCAM" +
  "RFEOxv2vVVOTGhi/gIJh8RZMaKcp52w7JTPbdoR3nwfsOCJ6rwMcffxD2Vkd4GhgJe2rHoEGV9AedwsCLZ" +
  "ih+wOGgRZ9oXtLGJZo4Ru6T2GoaPEI3UMwJFTQovNPMHxCRUTnWjBsocKS/u/BsIeKnf4bgeGIFeUOw1Gz" +
  "5WaWDtsHm52ye7DmAAAAAElFTkSuQmCC";
var DEFAULT_LEFT_NAVIGATION_IMG =
  "data:image/png;base64," +
  "iVBORw0KGgoAAAANSUhEUgAAABAAAAAgCAYAAAAbifjMAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJA" +
  "AAFiQBmxXGFAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAIZJREFUSEut0EEKxEAI" +
  "RNE5mPe/lqEWgjE/kLZm8VbtL0h+mbkWEfzwheL1QMWrgR4fD8z4aIBiweOJwoJBR1GHUaFgwlDomFixWL" +
  "FYsVixWLFYsfgD9if85Sc6I7eBzchj4HQEB4SOCcaFggnDjqIOo4nCggGhWPD4jT0g9oDYA2IPSETkBTnK" +
  "m53Tz5HlAAAAAElFTkSuQmCC";
var DEFAULT_RIGHT_NAVIGATION_IMG =
 "data:image/png;base64," +
 "iVBORw0KGgoAAAANSUhEUgAAABAAAAAgCAYAAAAbifjMAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJAA" +
 "AFiQBmxXGFAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAJVJREFUSEvFz0sOgCAQBF" +
 "EOxv2vpbYJBEo+w8xCk9ro9EtMOefreZK3F4ggFfAiHeBBPsApMgQUD2dNAcXjUUtAccC2gOKozQQoDktmQ" +
 "HGsjgAVBlQYUGFAhQH1LxD6BY3dQBm7gHZ8DHCszACHJRPAUdsW4IAtAR6PmgI8nDUEeLTqA/BgVwfwo6UK" +
 "8IO1F+BLe1e6AdOmm5054q0eAAAAAElFTkSuQmCC";
var DEFAULT_FRAME_STYLE = "display: flex; flex-flow: column;";
var DEFAULT_DIV_MAIN_IMAGE_STYLE = "flex: 1 0 auto; order: 0; height: 100%; width: 100%; overflow: hidden;";
var DEFAULT_THUMB_CONTAINER_STYLE = "flex: 0 0 auto; display: flex; flex-flow: row; order: 1;";
var DEFAULT_THUMB_IMAGE_CONTAINER_STYLE = "flex: 1; display: flex; flex-flow: row; overflow: hidden;";
var DEFAULT_THUMB_SCROLL_LR_STYLE = "flex: 0 0 auto; width: 10px; height: 16px;";
var DEFAULT_THUMB_SCROLL_L_STYLE = "margin: 0 4px 0 0;";
var DEFAULT_THUMB_SCROLL_R_STYLE = "margin: 0 0 0 4px;";
var DEFAULT_THUMB_SCROLL_UD_STYLE = "flex: 0 0 auto; height: 10px; width: 16px;";
var DEFAULT_THUMB_SCROLL_U_STYLE = "margin: 0 0 4px 0;";
var DEFAULT_THUMB_SCROLL_D_STYLE = "margin: 4px 0 0;";
var DEFAULT_THUMB_IMAGEDIV_STYLE = "flex: 0 0 auto; overflow: hidden; margin: 4px;";
var DEFAULT_IMAGE_STYLE = "position: relative;";
var DEFAULT_STRETCH_IMAGE_STYLE = "position: relative; height: 100%; width: 100%;";
var DEFAULT_SCROLL_SPEED = 300;
var DEFAULT_THUMBS_POSITION = "bottom";
var DEFAULT_IMAGE_FADEIN_TIME = 500;
var DEFAULT_IMAGE_FADEOUT_TIME = 0;

// json data parameter names
var PARAM_THUMBS_POSITION = "thumbsPosition";
var PARAM_DEF_IMG_SRC = "defaultImageSrc";
var PARAM_DEF_THUMB_SRC = "defaultThumbSrc";
var PARAM_THUMB_SRC = "thumbSrc";
var PARAM_THUMBS = "thumbs";
var PARAM_IMG_SRC = "imageSrc";
var PARAM_HEIGHT = "h";
var PARAM_WIDTH = "w";
var PARAM_LEFT = "l";
var PARAM_TOP = "t";




var IBox = function (container, init) {

  // fields

  if (WAVE.isStringType(typeof (container)))
    container = WAVE.id(container);

  if (!WAVE.isObject(init))
    init = {};

  var ibox = this;
  WAVE.extend(ibox, WAVE.EventManager);

  var selectedThumb;
  var allThumbs = [];
  var isMainImageMouseDown;

  var divFrame;
  var divMainImage;
  var imgMainImage;
  var divThumbContainer;
  var divThumbsScrollBack;
  var divThumbsImagesContainer;
  var divThumbsScrollForth;

  // public

  this.getSelectedThumb = function () {
    return selectedThumb;
  }

  this.getAllThumbs = function () {
    return allThumbs;
  }

  this.setVisible = function (isVisible) {
    if (isVisible)
      divFrame.style.visibility = "visible";
    else
      divFrame.style.visibility = "hidden";
  }

  // private

  var fitImageToContainer = function (imageElement, containerElement) {
    var iw = imageElement.naturalWidth;
    var ih = imageElement.naturalHeight;
    var w = containerElement.clientWidth;
    var h = containerElement.clientHeight;

    if ((w * ih) / (iw * h) > 1) {
      var width = h * iw / ih;
      imageElement.style.width = width + "px";
      imageElement.style.height = h + "px";
      imageElement.style.left = (w - width) / 2 + "px";
      imageElement.style.top = "0px";
    } else {
      var height = w * ih / iw;
      imageElement.style.width = w + "px";
      imageElement.style.height = height + "px";
      imageElement.style.left = "0px";
      imageElement.style.top = (h - height) / 2 + "px";
    }
  }

  var loadImage = function (imageElement, imageSrc, defaultImgSource, loadedEventName, loadedHandler, errorHandler) {
    var prevSrc = imageElement.src;
    if (imageSrc !== prevSrc) {
      var downloadingImage = new Image();
      downloadingImage.onload = function () {
        var src = this.src;
        $(imageElement).fadeOut(DEFAULT_IMAGE_FADEOUT_TIME, function () {
          imageElement.src = src;
        }).fadeIn(DEFAULT_IMAGE_FADEIN_TIME);
        if (typeof (loadedEventName) != WAVE.UNDEFINED) {
          ibox.eventInvoke(loadedEventName, imageSrc);
        }
        if (typeof (loadedHandler) != WAVE.UNDEFINED) {
          loadedHandler();
        }
      };
      downloadingImage.onerror = function () {
        imageElement.src = defaultImgSource;
        if (typeof (errorHandler) != WAVE.UNDEFINED) {
          errorHandler();
        }
      };
      downloadingImage.src = imageSrc;
    }
  }

  var thumbClickFactory = function (divThumbImage, mainImageSrc) {
    return function () {
      selectedThumb = divThumbImage;
      loadImage(imgMainImage,
                mainImageSrc,
                DEFAULT_MAIN_IMG,
                EVT_IMAGE_CHANGED,
                function () { fitImageToContainer(imgMainImage, divMainImage) },
                function () { fitImageToContainer(imgMainImage, divMainImage) });
    };
  }

  var eventInvokeFactory = function (eventSource, eventName, applyClass) {
    return function () {
      if (typeof (applyClass) != WAVE.UNDEFINED)
        eventSource.className = applyClass;
      ibox.eventInvoke(eventName, eventSource);
    }
  }

  var thumbLoadImageFactory = function (imgThumbImage, thumbImageSrc) {
    return function () {
      loadImage(imgThumbImage, thumbImageSrc, DEFAULT_THUMB_IMG);
    }
  }

  var isOverflowed = function (element) {
    return element.scrollHeight > element.clientHeight || element.scrollWidth > element.clientWidth;
  }

  var scrollThumbsBack = function (minMargin, sizeGetter, animation) {
    var summ = 0;
    for (var i = 0; i < allThumbs.length; i++) {
      if (summ + sizeGetter(allThumbs[i]) > minMargin - 1) break;
      summ += sizeGetter(allThumbs[i]);
    }
    var delta = minMargin - summ;
    $(divThumbsImagesContainer).animate(animation(delta), DEFAULT_SCROLL_SPEED);
  }

  var scrollThumbsForth = function (maxMargin, sizeGetter, animation) {
    var summ = 0;
    for (var i = 0; i < allThumbs.length; i++) {
      summ += sizeGetter(allThumbs[i]);
      if (summ > maxMargin + 1) break;
    }
    var delta = summ - maxMargin;
    $(divThumbsImagesContainer).animate(animation(delta), DEFAULT_SCROLL_SPEED);
  }

  var calculateImageZoomOffsets = function (ih, iw, h, w, x, y) {
    var delta, X, Y;
    var aspect = iw * h / (ih * w);
    if (aspect > 1) {
      delta = w * ih / iw;
      y = Math.min(Math.max(y, (h - delta) / 2), (h + delta) / 2);
      X = iw * x / w;
      Y = (y - (h - delta) / 2) * iw / w;
    } else {
      delta = h * iw / ih;
      x = Math.min(Math.max(x, (w - delta) / 2), (w + delta) / 2);
      X = (x - (w - delta) / 2) * ih / h;
      Y = ih * y / h;
    }
    var left = iw > w ? Math.min(Math.max(X - w / 2, 0), iw - w) : (iw - w) / 2;
    var top = ih > h ? Math.min(Math.max(Y - h / 2, 0), ih - h) : (ih - h) / 2;

    return { left: left, top: top };
  }

  var divMainImageMouseDown = function (e) {
    e.preventDefault();

    var iw = imgMainImage.naturalWidth;
    var ih = imgMainImage.naturalHeight;
    var w = divMainImage.clientWidth;
    var h = divMainImage.clientHeight;

    if (iw <= w && ih <= h)
      return;

    isMainImageMouseDown = true;
    imgMainImage.style.width = '';
    imgMainImage.style.height = '';
    imgMainImage.style.left = '';
    imgMainImage.style.top = '';

    var ex = e.pageX - $(divMainImage).offset().left;
    var ey = e.pageY - $(divMainImage).offset().top;
    var offset = calculateImageZoomOffsets(ih, iw, h, w, ex, ey);
    imgMainImage.style.left = -offset.left + "px";
    imgMainImage.style.top = -offset.top + "px";
  }

  var divMainImageMouseMove = function (e) {
    e.preventDefault();
    if (isMainImageMouseDown) {
      var iw = imgMainImage.naturalWidth;
      var ih = imgMainImage.naturalHeight;
      var w = divMainImage.clientWidth;
      var h = divMainImage.clientHeight;
      var ex = e.pageX - $(divMainImage).offset().left;
      var ey = e.pageY - $(divMainImage).offset().top;
      var offset = calculateImageZoomOffsets(ih, iw, h, w, ex, ey);
      imgMainImage.style.left = -offset.left + "px";
      imgMainImage.style.top = -offset.top + "px";
    }
  }

  var divMainImageMouseUp = function (e) {
    e.preventDefault();
    if (isMainImageMouseDown) {
      fitImageToContainer(imgMainImage, divMainImage);
      isMainImageMouseDown = false;
    }
  }

  // parse init json

  var defaultImageSrc = WAVE.strDefault(init[PARAM_DEF_IMG_SRC], DEFAULT_MAIN_IMG);
  var defaultThumbSrc = WAVE.strDefault(init[PARAM_DEF_THUMB_SRC], DEFAULT_THUMB_IMG);
  var thumbSrc = init[PARAM_THUMB_SRC];
  var thumbs = init[PARAM_THUMBS];
  var thumbsOrientation = WAVE.strDefault(init[PARAM_THUMBS_POSITION], DEFAULT_THUMBS_POSITION);

  // initialize control UI

  // control frame
  divFrame = document.createElement('div');
  divFrame.style.cssText = DEFAULT_FRAME_STYLE;
  divFrame.className = IBOX_FRAMEDIV;
  container.appendChild(divFrame);

  // main image div
  divMainImage = document.createElement('div');
  divMainImage.style.cssText = DEFAULT_DIV_MAIN_IMAGE_STYLE;
  divMainImage.className = IBOX_MAINIMAGEDIV;
  //$(divMainImage).bind('mousedown touchstart', divMainImageMouseDown);
  //$(divMainImage).bind('mousemove touchmove', divMainImageMouseMove);
  //$(document).bind('mouseup touchend', divMainImageMouseUp);
  $(divMainImage).mousedown(divMainImageMouseDown);
  $(divMainImage).mousemove(divMainImageMouseMove);
  $(document).mouseup(divMainImageMouseUp);

  // main image
  imgMainImage = document.createElement('img');
  imgMainImage.style.cssText = DEFAULT_IMAGE_STYLE;
  imgMainImage.src = DEFAULT_MAIN_IMG;
  $(imgMainImage).mouseenter(eventInvokeFactory(imgMainImage, EVT_MAINIMAGE_MOUSEOVER));
  $(imgMainImage).mouseleave(eventInvokeFactory(imgMainImage, EVT_MAINIMAGE_MOUSEOUT));

  // div container for thumbnails with navigation
  divThumbContainer = document.createElement('div');
  divThumbContainer.className = IBOX_THUMBCONTAINERDIV;
  divThumbContainer.style.cssText = DEFAULT_THUMB_CONTAINER_STYLE;

  // div container for thumbnail images
  divThumbsImagesContainer = document.createElement('div');
  divThumbsImagesContainer.className = IBOX_THUMBIMAGESCONTAINERDIV;
  divThumbsImagesContainer.style.cssText = DEFAULT_THUMB_IMAGE_CONTAINER_STYLE;
  divFrame.appendChild(divMainImage);
  divMainImage.appendChild(imgMainImage);
  divFrame.appendChild(divThumbContainer);
  divThumbContainer.appendChild(divThumbsImagesContainer);

  // thumbnails
  if (typeof (thumbs) != WAVE.UNDEFINED) {
    for (var i = 0; i < thumbs.length; ++i) {
      var thumb = thumbs[i];

      // div container for thumbnail
      var divThumbImage = document.createElement('div');
      divThumbImage.className = IBOX_THUMBIMAGEDIV;
      divThumbImage.style.cssText = DEFAULT_THUMB_IMAGEDIV_STYLE;
      $(divThumbImage).mouseenter(eventInvokeFactory(divThumbImage, EVT_THUMBIMAGE_MOUSEOVER, IBOX_THUMBIMAGEDIV + " " + IBOX_THUMBIMAGEDIV_MOUSEOVER));
      $(divThumbImage).mouseleave(eventInvokeFactory(divThumbImage, EVT_THUMBIMAGE_MOUSEOUT, IBOX_THUMBIMAGEDIV + " " + IBOX_THUMBIMAGEDIV_MOUSEOUT));

      // thumbnail image
      var imgThumbImage = document.createElement('img');
      imgThumbImage.style.cssText = DEFAULT_IMAGE_STYLE;
      imgThumbImage.src = DEFAULT_THUMB_IMG;
      divThumbImage.appendChild(imgThumbImage);

      var src;
      if (typeof (thumbSrc) !== WAVE.UNDEFINED) {
        divThumbImage.style.height = WAVE.strDefault(thumb[PARAM_HEIGHT], '0') + "px";
        divThumbImage.style.width = WAVE.strDefault(thumb[PARAM_WIDTH], '0') + "px";
        imgThumbImage.style.top = WAVE.strDefault(thumb[PARAM_TOP], '0') + "px";
        imgThumbImage.style.left = WAVE.strDefault(thumb[PARAM_LEFT], '0') + "px";
        src = thumbSrc;
      } else {
        divThumbImage.style.height = '50px';
        divThumbImage.style.width = '50px';
        imgThumbImage.style.cssText = DEFAULT_STRETCH_IMAGE_STYLE;
        src = defaultThumbSrc;
      }

      thumbLoadImageFactory(imgThumbImage, src)();

      var mainImageSrc = WAVE.strDefault(thumb[PARAM_IMG_SRC], defaultImageSrc);
      $(imgThumbImage).click(thumbClickFactory(divThumbImage, mainImageSrc));

      divThumbsImagesContainer.appendChild(divThumbImage);
      allThumbs.push(divThumbImage);
    }

    // load the 1st image
    var imageSrc = WAVE.strDefault(thumbs[0][PARAM_IMG_SRC], defaultImageSrc);
    thumbClickFactory(allThumbs[0], imageSrc)();
  };

  // set thumb container orientation
  switch (thumbsOrientation) {
    case "left":
      divMainImage.style.order = 1;
      divThumbContainer.style.order = 0;
      divFrame.style.flexFlow = "row";
      divThumbContainer.style.flexFlow = "column";
      divThumbsImagesContainer.style.flexFlow = "column";
      break;
    case "right":
      divMainImage.style.order = 0;
      divThumbContainer.style.order = 1;
      divFrame.style.flexFlow = "row";
      divThumbContainer.style.flexFlow = "column";
      divThumbsImagesContainer.style.flexFlow = "column";
      break;
    case "top":
      divMainImage.style.order = 1;
      divThumbContainer.style.order = 0;
      divFrame.style.flexFlow = "column";
      divThumbContainer.style.flexFlow = "row";
      divThumbsImagesContainer.style.flexFlow = "row";
      break;
    case "bottom":
      divMainImage.style.order = 0;
      divThumbContainer.style.order = 1;
      divFrame.style.flexFlow = "column";
      divThumbContainer.style.flexFlow = "row";
      divThumbsImagesContainer.style.flexFlow = "row";
      break;
  }

  // on loaded
  $(document).ready(function () {

    // initially image adjusting
    fitImageToContainer(imgMainImage, divMainImage);

    // if thumbs are overflowed add navigation arrows
    var needNavigation = isOverflowed(divThumbsImagesContainer);
    if (!needNavigation) {
      divThumbsImagesContainer.style.justifyContent = "center";
    }
    else {
      // div container for back (left or up) navigation button
      divThumbsScrollBack = document.createElement('div');
      divThumbsScrollBack.className = IBOX_THUMBNAVIGATIONDIV;
      divThumbContainer.insertBefore(divThumbsScrollBack, divThumbContainer.firstChild);

      // image for back (left or up) navigation button
      var imgBackImage = document.createElement('img');
      imgBackImage.style.cssText = DEFAULT_IMAGE_STYLE;
      divThumbsScrollBack.appendChild(imgBackImage);

      // div container for forth (right or down) navigation button
      divThumbsScrollForth = document.createElement('div');
      divThumbsScrollForth.className = IBOX_THUMBNAVIGATIONDIV;
      divThumbContainer.appendChild(divThumbsScrollForth);

      // image for forth (right or down) navigation button
      var imgForthImage = document.createElement('img');
      imgForthImage.style.cssText = DEFAULT_IMAGE_STYLE;
      divThumbsScrollForth.appendChild(imgForthImage);

      if (thumbsOrientation === "top" || thumbsOrientation === "bottom") {
        divThumbsScrollBack.style.cssText = DEFAULT_THUMB_SCROLL_LR_STYLE + " " + DEFAULT_THUMB_SCROLL_L_STYLE;
        divThumbsScrollForth.style.cssText = DEFAULT_THUMB_SCROLL_LR_STYLE + " " + DEFAULT_THUMB_SCROLL_R_STYLE;
        imgBackImage.src = DEFAULT_LEFT_NAVIGATION_IMG;
        imgForthImage.src = DEFAULT_RIGHT_NAVIGATION_IMG;
        divThumbsScrollBack.style.height = divThumbContainer.clientHeight + "px";
        divThumbsScrollForth.style.height = divThumbContainer.clientHeight + "px";

        $(divThumbsScrollBack).click(function () {
          scrollThumbsBack(divThumbsImagesContainer.scrollLeft,
            function (e) { return e.offsetWidth + parseInt(e.style.marginLeft, 10) + parseInt(e.style.marginRight, 10); },
            function (d) { return { scrollLeft: "-=" + d } });
        });

        $(divThumbsScrollForth).click(function () {
          scrollThumbsForth(divThumbsImagesContainer.scrollLeft + divThumbsImagesContainer.offsetWidth,
                              function (e) { return e.offsetWidth + parseInt(e.style.marginLeft, 10) + parseInt(e.style.marginRight, 10); },
                              function (d) { return { scrollLeft: "+=" + d } });
        });

      } else if (thumbsOrientation === "left" || thumbsOrientation === "right") {
        divThumbsScrollBack.style.cssText = DEFAULT_THUMB_SCROLL_UD_STYLE + " " + DEFAULT_THUMB_SCROLL_U_STYLE;
        divThumbsScrollForth.style.cssText = DEFAULT_THUMB_SCROLL_UD_STYLE + " " + DEFAULT_THUMB_SCROLL_D_STYLE;
        imgBackImage.src = DEFAULT_UP_NAVIGATION_IMG;
        imgForthImage.src = DEFAULT_DOWN_NAVIGATION_IMG;
        divThumbsScrollBack.style.width = divThumbContainer.clientWidth + "px";
        divThumbsScrollForth.style.width = divThumbContainer.clientWidth + "px";

        $(divThumbsScrollBack).click(function () {
          scrollThumbsBack(divThumbsImagesContainer.scrollTop,
                           function (e) { return e.offsetHeight + parseInt(e.style.marginTop, 10) + parseInt(e.style.marginBottom, 10); },
                           function (d) { return { scrollTop: "-=" + d } });
        });

        $(divThumbsScrollForth).click(function () {
          scrollThumbsForth(divThumbsImagesContainer.scrollTop + divThumbsImagesContainer.offsetHeight,
                              function (e) { return e.offsetHeight + parseInt(e.style.marginTop, 10) + parseInt(e.style.marginBottom, 10); },
                              function (d) { return { scrollTop: "+=" + d } });
        });
      }

      fitImageToContainer(imgBackImage, divThumbsScrollBack);
      fitImageToContainer(imgForthImage, divThumbsScrollForth);
    }
  });


};
