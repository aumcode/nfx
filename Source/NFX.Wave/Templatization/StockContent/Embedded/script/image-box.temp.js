// constants

var IBOX_FRAMEDIV = "wvIBoxFrameDiv";
var IBOX_MAINIMAGEDIV = "wvIBoxMainImageDiv";
var IBOX_THUMBCONTAINERDIV = "wvIBoxThumbContainerDiv";
var IBOX_THUMBNAVIGATIONDIV = "wvIBoxThumbNavigationDiv";
var IBOX_THUMBIMAGESCONTAINERDIV = "wvIBoxThumbImagesContainerDiv";
var IBOX_THUMBIMAGEDIV = "wvIBoxThumbImageDiv";

var EVT_IMAGE_CHANGED = 'image-changed';

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
  "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";

var DEFAULT_FIT_IMAGE_STYLE = "width: 100%; height: 100%; object-fit: contain;";
var DEFAULT_THUMB_CONTAINER_STYLE = "display: flex; overflow: auto; overflow-x: hidden; text-align:center;";
var DEFAULT_THUMB_SCROLL_LEFT_STYLE = "height: 20px; width: 20px; float: left; background: green; flex-shrink: 0;";
var DEFAULT_THUMB_SCROLL_RIGHT_STYLE = "height: 20px; width: 20px; float: left; background: green; flex-shrink: 0;";
var DEFAULT_THUMB_IMAGE_CONTAINER_STYLE = "display: flex; overflow: auto; float: left; overflow-x: hidden; text-align:center;";
var DEFAULT_THUMB_IMAGEDIV_STYLE = "overflow: hidden; position: relative; flex-shrink: 0;";
var DEFAULT_THUMB_IMAGE_STYLE = "position: absolute;";
var DEFAULT_SCROLL_SPEED = 300;






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
  var thumbsLength;

  var divFrame;
  var divMainImage;
  var imgMainImage;
  var divThumbContainer;
  var divThumbsScrollLeft;
  var divThumbsImagesContainer;
  var divThumbsScrollRight;

  // public

  this.getSelectedThumb = function () {
    return selectedThumb;
  }

  this.getAllThumbs = function () {
    return allThumbs;
  }

  // private

  var getNextThumb = function () {
    if (typeof (selectedThumb) == WAVE.UNDEFINED ||
        typeof (allThumbs) == WAVE.UNDEFINED)
      return WAVE.UNDEFINED;
    var i = allThumbs.indexOf(selectedThumb);
    if (i < 0 || i >= allThumbs.length - 1)
      return WAVE.UNDEFINED;
    return allThumbs[i + 1];
  }

  var getPrevThumb = function () {
    if (typeof (selectedThumb) == WAVE.UNDEFINED ||
        typeof (allThumbs) == WAVE.UNDEFINED)
      return WAVE.UNDEFINED;
    var i = allThumbs.indexOf(selectedThumb);
    if (i <= 0)
      return WAVE.UNDEFINED;
    return allThumbs[i - 1];
  }

  var loadImage = function (element, imageSrc, defaultImgSource, loadedEventName) {
    var prevSrc = element.src;
    if (imageSrc !== prevSrc) {
      var downloadingImage = new Image();
      downloadingImage.onload = function () {
        element.src = this.src;
        if (typeof (loadedEventName) != WAVE.UNDEFINED) {
          ibox.eventInvoke(loadedEventName, imageSrc);
        }
      };
      downloadingImage.onerror = function () {
        element.src = defaultImgSource;
      };
      downloadingImage.src = imageSrc;
    }
  }

  var thumbClickFactory = function (divThumbImage, mainImageSrc) {
    return function () {
      selectedThumb = divThumbImage;
      loadImage(imgMainImage, mainImageSrc, DEFAULT_MAIN_IMG, EVT_IMAGE_CHANGED);
    }
  }

  var thumbLoadImageFactory = function (imgThumbImage, thumbImageSrc) {
    return function () {
      loadImage(imgThumbImage, thumbImageSrc, DEFAULT_THUMB_IMG);
    }
  }

  function isOverflowed(element) {
    return element.scrollHeight > element.clientHeight || element.scrollWidth > element.clientWidth;
  }

  // parse init json

  var defaultImageSrc = WAVE.strDefault(init['defaultImageSrc'], DEFAULT_MAIN_IMG);
  var defaultThumbSrc = WAVE.strDefault(init['defaultThumbSrc'], DEFAULT_THUMB_IMG);
  var thumbSrc = init['thumbSrc'];
  var thumbs = init['thumbs'];

  // initialize control UI

  divFrame = document.createElement('div');
  divFrame.className = IBOX_FRAMEDIV;

  divMainImage = document.createElement('div');
  divMainImage.className = IBOX_MAINIMAGEDIV;
  divFrame.appendChild(divMainImage);

  imgMainImage = document.createElement('img');
  imgMainImage.style.cssText = DEFAULT_FIT_IMAGE_STYLE;
  imgMainImage.src = DEFAULT_MAIN_IMG;
  divMainImage.appendChild(imgMainImage);

  divThumbContainer = document.createElement('div');
  divThumbContainer.className = IBOX_THUMBCONTAINERDIV;
  divThumbContainer.style.cssText = DEFAULT_THUMB_CONTAINER_STYLE;
  divFrame.appendChild(divThumbContainer);

  divThumbsImagesContainer = document.createElement('div');
  divThumbsImagesContainer.style.cssText = DEFAULT_THUMB_IMAGE_CONTAINER_STYLE;
  divThumbContainer.appendChild(divThumbsImagesContainer);

  if (typeof (thumbs) != WAVE.UNDEFINED) {
    for (var i = 0; i < thumbs.length; ++i) {
      var thumb = thumbs[i];

      var divThumbImage = document.createElement('div');
      divThumbImage.className = IBOX_THUMBIMAGEDIV;
      divThumbImage.style.cssText = DEFAULT_THUMB_IMAGEDIV_STYLE;

      var imgThumbImage = document.createElement('img');
      imgThumbImage.style.cssText = DEFAULT_THUMB_IMAGE_STYLE;
      imgThumbImage.src = DEFAULT_THUMB_IMG;
      divThumbImage.appendChild(imgThumbImage);

      var src;
      if (typeof (thumbSrc) !== WAVE.UNDEFINED) {
        divThumbImage.style.height = WAVE.strDefault(thumb['h'], '0');
        divThumbImage.style.width = WAVE.strDefault(thumb['w'], '0');
        imgThumbImage.style.top = WAVE.strDefault(thumb['t'], '0');
        imgThumbImage.style.left = WAVE.strDefault(thumb['l'], '0');
        imgThumbImage.style.objectFit = 'contain';
        src = thumbSrc;
      } else {
        divThumbImage.style.height = '50px';
        divThumbImage.style.width = '50px';
        imgThumbImage.style.cssText = DEFAULT_FIT_IMAGE_STYLE;
        src = defaultThumbSrc;
      }

      thumbLoadImageFactory(imgThumbImage, src)();

      var mainImageSrc = WAVE.strDefault(thumb['imageSrc'], defaultImageSrc);
      $(divThumbImage).click(thumbClickFactory(divThumbImage, mainImageSrc));

      $(divThumbImage).mouseenter(function () {
        this.style.border = '1px solid black';
      });

      $(divThumbImage).mouseleave(function () {
        this.style.border = '0';
      });

      divThumbsImagesContainer.appendChild(divThumbImage);
      allThumbs.push(divThumbImage);
    }

    container.appendChild(divFrame);

    // load the 1st image
    var imageSrc = WAVE.strDefault(thumbs[0]['imageSrc'], defaultImageSrc);
    thumbClickFactory(allThumbs[0], imageSrc)();
  };

  // on loaded
  $(document).ready(function () {

    // calculate displayed characteristics
    thumbsLength = WAVE.arrayWalkable(allThumbs).wSelect(function (t) { return t.offsetWidth; }).wSum();

    // if thumbs are overflowed add navigation arrows
    var needNavigation = isOverflowed(divThumbsImagesContainer);
    if (needNavigation) {
      divThumbsScrollLeft = document.createElement('div');
      divThumbsScrollLeft.className = IBOX_THUMBNAVIGATIONDIV;
      divThumbsScrollLeft.style.cssText = DEFAULT_THUMB_SCROLL_LEFT_STYLE;
      $(divThumbsScrollLeft).click(function () {
        var left = divThumbsImagesContainer.scrollLeft;
        var summ = 0;
        for (var i = 0; i < allThumbs.length; i++) {
          if (summ + allThumbs[i].offsetWidth >= left) break;
          summ += allThumbs[i].offsetWidth;
        }
        var delta = left - summ;
        $(divThumbsImagesContainer).animate({
          scrollLeft: "-=" + delta
        }, DEFAULT_SCROLL_SPEED);
      });
      divThumbContainer.insertBefore(divThumbsScrollLeft, divThumbContainer.firstChild);

      divThumbsScrollRight = document.createElement('div');
      divThumbsScrollRight.className = IBOX_THUMBNAVIGATIONDIV;
      divThumbsScrollRight.style.cssText = DEFAULT_THUMB_SCROLL_RIGHT_STYLE;
      $(divThumbsScrollRight).click(function () {
        var right = divThumbsImagesContainer.scrollLeft + divThumbsImagesContainer.offsetWidth;
        var summ = 0;
        for (var i = 0; i < allThumbs.length; i++) {
          summ += allThumbs[i].offsetWidth;
          if (summ > right) break;
        }
        var delta = summ - right;
        $(divThumbsImagesContainer).animate({
          scrollLeft: "+=" + delta
        }, DEFAULT_SCROLL_SPEED);
      });
      divThumbContainer.appendChild(divThumbsScrollRight);
    }
  });
};
