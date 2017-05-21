function equalsImage(left, right) {
  if (left === right) return true;
  if (typeof(left) === tUNDEFINED || typeof(right) === tUNDEFINED) return false;
  if (!WAVE.isObject(left)) left = { url: left };
  if (!WAVE.isObject(right)) right = { url: right };
  if (left.url === right.url) {
    if (WAVE.has(left, 'clip')) {
      if (WAVE.has(right, 'clip')) {
        return left.left|0 === right.left|0
          && left.right|0  === right.right|0
          && left.bottom|0 === right.bottom|0
          && left.top|0    === right.top|0;
      }
    } else {
      if (!WAVE.has(right, 'clip')) return true;
    }
  }
  return false;
}

function loadImage(image, onload, onerror, urlMap) {
  if (!WAVE.isObject(image)) image = { url: image };
  image.url = WAVE.strDefault(ensureUrl(WAVE.strDefault(image.url)));
  var img = new Image();
  img.onerror = function () {
    image.error = true;
    if (typeof(onerror) !== tUNDEFINED)
      onerror.apply(img, arguments);
  };
  if (WAVE.has(image, 'clip')) {
    var clip = image.clip;
    var left =   clip.left|0;
    var right  = clip.right|0;
    var bottom = clip.bottom|0;
    var top    = clip.top|0;
    var
      clipWidth = right - left,
      clipHeight = bottom - top;
    var canvas = document.createElement('canvas');
    canvas.width = clipWidth;
    canvas.height = clipHeight;
    img.onload = function () {
      image.load = true;
      image.width = clipWidth;
      image.height = clipHeight;
      var ctx = canvas.getContext('2d');
      ctx.drawImage(this, left, top, clipWidth, clipHeight, 0, 0, clipWidth, clipHeight);
      if (typeof(onload) !== tUNDEFINED)
        onload.apply(canvas, arguments);
      delete image.img;
    };
    image.element = canvas;
  } else {
    img.onload = function () {
      image.load = true;
      image.width = this.width;
      image.height = this.height;
      var that = this;
      if (typeof(onload) !== tUNDEFINED)
        onload.apply(img, arguments);
    };
    image.element = img;
  }
  img.src = image.url;
  image.free = free;

  return image;

  function free() {
    if (image.element.parentNode)
      image.element.parentNode.removeChild(image.element);
    delete image.element;
    delete image.error;
    delete image.load;
    delete image.img;
    delete image.width;
    delete image.height;
    delete image.free;
  }
  function ensureUrl(url) { return url.indexOf('$') === 0 && WAVE.isObject(urlMap) ? urlMap[url.substr(1)] : url; }
}

function makeDraggable(element, ondrag, onmove, ondrop, preventDefault) {
  element.addEventListener('mousedown', mousedown, false);
  element.addEventListener('touchstart', touchstart, false);

  function mousedown(event) {
    if (event.button === 0) {
      if (preventDefault) event.preventDefault();
      dragstart(event.pageX, event.pageY);
    }
  }
  function touchstart(event) {
    var touches = event.touches;
    if (touches.length === 1) {
      if (preventDefault) event.preventDefault();
      dragstart(touches[0].pageX, touches[0].pageY);
    }
  }
  function dragstart(x, y) {
    if (typeof(ondrag) !== tUNDEFINED)
      ondrag.call(element, x, y);
    document.addEventListener('mousemove', mousemove, false);
    document.addEventListener('touchmove', touchmove, false);
    document.addEventListener('mouseup', mouseup, false);
    document.addEventListener('touchend', touchend, false);
    function mousemove(event) {
      event.preventDefault();
      var
        pageX = event.pageX,
        pageY = event.pageY;
      dragmove(x, y, x - pageX, y - pageY);
      x = pageX; y = pageY;
    }
    function touchmove(event) {
      event.preventDefault();
      var touches = event.touches;
      var
        pageX = touches[0].pageX,
        pageY = touches[0].pageY;
      dragmove(x, y, x - pageX, y - pageY);
      x = pageX; y = pageY;
    }
    function mouseup(event) {
      dragend(event.pageX, event.pageY);
      cleanup();
    }
    function touchend(event) {
      dragend(x, y);
      cleanup();
    }
    function cleanup() {
      document.removeEventListener('mousemove', mousemove, false);
      document.removeEventListener('touchmove', touchmove, false);
      document.removeEventListener('mouseup',   mouseup,   false);
      document.removeEventListener('touchend',  touchend,  false);
    }
  }
  function dragmove(x, y, offsetX, offsetY) {
    if (typeof(onmove) !== tUNDEFINED)
      onmove.call(element, x, y);
    element.scrollLeft += offsetX;
    element.scrollTop += offsetY;
  }
  function dragend(x, y) {
    if (typeof(ondrop) !== tUNDEFINED)
      ondrop.call(element, x, y);
  }
}

var fImageViewIdSeed = 0;
published.ImageView = function (init, urlMap) {
  if (typeof(init) === tUNDEFINED || init === null) init = {};
  if (WAVE.isString(init)) init = { DIV: init };
  if (!WAVE.has(init, 'DIV')) throw "ImageView.ctor(init.thumbnails)";

  var fId = published.CLS_IMAGE_VIEW + fImageViewIdSeed++;

  var imageView = this;
  WAVE.extend(imageView, WAVE.EventManager);

  var fViewbox = build(WAVE.isString(init.DIV) ? WAVE.id(init.DIV) : init.DIV);

  var fImage;
  imageView.image = function (image) {
    if (typeof(image) !== tUNDEFINED)  {
      fImage = updateImage(image);
    }
    return fImage;
  };
  imageView.clear = function (flag) {
    if (flag)
      fViewbox.innerHTML = '';
    return fViewbox.innerHTML === '';
  };

  imageView.image(init.image);

  window.addEventListener('resize', function() { update(imageView.image() || {}); }, false);

  return imageView;

  function build(parent) {
    var viewbox = document.createElement('div');
    viewbox.className = published.CLS_IMAGE_VIEW;
    parent.appendChild(viewbox);
    makeDraggable(viewbox, function (x, y) {
      var
        image = imageView.image(),
        offset = position(viewbox),
        box = size(viewbox),
        client = { x: x - offset.x - box.x / 2, y: y - offset.y - box.y / 2 },
        sz = size(fImage.element),
        newClient = {
          x: client.x * fImage.width / sz.x,
          y: client.y * fImage.height / sz.y
        };
      if (fImage.width > sz.x || fImage.height > sz.y) {
        viewbox.className += ' ' + published.CLS_IMAGE_VIEW_ZOOM;
        var scr = scroll(viewbox);
        viewbox.scrollLeft = newClient.x + scr.x / 2 - (x - offset.x);
        viewbox.scrollTop = newClient.y + scr.y / 2 - (y - offset.y);
      }
    }, undefined, update, true);
    return viewbox;
  }

  function updateImage(image) {
    if (typeof(image) === tUNDEFINED) return;
    imageView.clear(true);
    if (image === null) return;

    if (WAVE.isObject(image) && WAVE.has(image, 'element')) {
      if (!image.error) fViewbox.appendChild(image.element);
      update(image);
    } else {
      image = loadImage(image, function () {
        this.className = published.CLS_IMAGE_VIEW_IMAGE;
        if (imageView.clear())
          fViewbox.appendChild(this);
        update(image);
      }, function () {
        update(image);
      }, urlMap);
    }
    return image;
  }

  function update(image) {
    if (typeof(fViewbox) === tUNDEFINED) return;
    fViewbox.className = published.CLS_IMAGE_VIEW;
    if (image.error) fViewbox.className += ' ' + published.CLS_IMAGE_BOX_ERROR;
    else if (!image.load) fViewbox.className += ' ' + published.CLS_IMAGE_BOX_LOADING;
    else {
      var
        sz = size(fViewbox), aspect = sz.x / sz.y,
        imageAspect = (image.width|0) / (image.height|0);
      if (!isNaN(imageAspect))
        fViewbox.className += ' ' + ((imageAspect >= aspect) ? published.CLS_IMAGE_BOX_WIDER : published.CLS_IMAGE_BOX_HIGHER);
    }
  }

  function position(elm) {
    var ox=0, oy=0;
    while (elm !== null)
      ox += elm.offsetLeft, oy += elm.offsetTop, elm = elm.offsetParent;
    return { x: ox, y: oy };
  }
  function size(elm) { return { x: elm.offsetWidth, y: elm.offsetHeight }; }
  function scroll(elm) { return { x: elm.scrollWidth, y: elm.scrollHeight }; }
};//ImageView

/*
Image = {
  url: $URL_NAME | URL,
  clip: { top: NUMBER, bottom: NUMBER, left: NUMBER, right: NUMBER }
};
Gallery = {
  thumbnails: DIV,
  preview: DIV,
  urls: {
    name: URL
  },
  images: [
    {
      id: TEXT,
      image: Image | $URL_NAME | URL,
      thumb: Image | URL,
      title: TEXT,
      content: TEXT | HTML,
      isText: BOOL
    } | Image | URL
  ]
} | DIV_ID;
*/
var fGalleryIdSeed = 0;
published.Gallery = function (init) {
  if (typeof(init) === tUNDEFINED || init === null) init = {};
  if (WAVE.isString(init)) init = { thumbnails: init };
  if (!WAVE.has(init, 'thumbnails')) throw "Gallery.ctor(init.thumbnails)";
  var hasUrls = WAVE.has(init, 'urls');
  if (hasUrls && !WAVE.isObject(init.urls)) throw "Gallery.ctor(init.urls)";

  var fId = fGalleryIdSeed++;

  var gallery = this;
  WAVE.extend(gallery, WAVE.EventManager);

  gallery.urls = (hasUrls ? init.urls : {});

  var fUpdate;
  var fThumbnails = build(WAVE.isString(init.thumbnails) ? WAVE.id(init.thumbnails) : init.thumbnails);
  var fPreview = WAVE.isString(init.preview) ? new published.ImageView(init.preview, gallery.urls) : init.preview;

  var fCurrent;
  var fItems = [];
  var fItemsLookup = {};

  var fItemIdSeed = 0;
  gallery.Item = function (init, gallery, view) {
    var seqId = fItemIdSeed++;

    var fId = WAVE.isObject(init) && WAVE.has(init, 'id') ? init.id : seqId;
    init.id = fId;
    var fInit = {};

    var item = this;
    WAVE.extend(item, WAVE.EventManager);

    var fGallery = gallery, fView = view;
    var fImage, fThumbImage;

    var
      fElem = document.createElement('li'),
      fThumb = document.createElement('div'),
      fContent = document.createElement('div');
    fElem.className = published.CLS_GALLERY_ITEM;
    fContent.className = published.CLS_GALLERY_CONTENT;
    fThumb.className = published.CLS_GALLERY_THUMBNAIL;
    fElem.appendChild(fThumb);
    fElem.appendChild(fContent);
    fElem.onclick = function() {
      gallery.current(fId);
    };

    item.id        = function () { return fId; };
    item.title     = function (title) {
      if (typeof(title) !== tUNDEFINED
        && fInit.title !== title)
        updateTitle(fInit.title = title);
      return WAVE.strDefault(fInit.title, fId);
    };
    item.content   = function (content, isText) {
      if (typeof(content) !== tUNDEFINED
        && (fInit.content !== content
          || !!fInit.isText !== !!isText))
        updateContent(fInit.content = content, fInit.isText = !!isText);
      return WAVE.strDefault(fInit.content);
    };
    item.isText    = function () { return !!fInit.isText; };
    item.image     = function (image) {
      if (typeof(image) !== tUNDEFINED
        && !equalsImage(fInit.image, image))
        updateImage(fInit.image = image);
      return fImage;
    };
    item.thumb     = function (image) {
      if (typeof(image) !== tUNDEFINED
        && !equalsImage(fInit.thumb, image))
        updateThumbnail(fInit.thumb = image);
      return fThumbImage;
    };
    item.data      = function (data) {
      if (typeof(data) !== tUNDEFINED) {
        fInit.data = data;
      }
      return fInit.data;
    };
    item.show      = function () { fImage = fView.image(fImage); };
    item.update    = update;
    item.free      = function () {
      if (fElem.parentNode)
        fElem.parentNode.removeChild(fElem);
      if (fView.image() === fImage && fImage.element.parentNode)
        fImage.element.parentNode.removeChild(fImage.element);
    };
    item.element   = function () { return fElem; };

    item.update(init);

    return item;

    function update(init) {
      if (!WAVE.isObject(init) || WAVE.has(init, "url"))  init = { image: init };
      if (!WAVE.has(init, 'image')) throw "Gallery.Item.update(init.image)";
      if (init.id !== fId) throw "Gallery.Item.update(init.id)";
      item.image(init.image);
      item.thumb(init.thumb || init.image);
      item.title(init.title);
      item.content(init.content, init.isText);
      item.data(init.data);
    }

    function updateImage(image) {
      var selected = false;
      if (typeof(fImage) !== tUNDEFINED) {
        selected = fImage === view.image();
        if (WAVE.has(fImage, 'free')) fImage.free();
      }
      fImage = image;
      if (selected) item.show();
    }

    function updateThumbnail(image) {
      fThumb.innerHTML = '';
      fThumb.className = published.CLS_GALLERY_THUMBNAIL + ' ' + published.CLS_IMAGE_BOX_LOADING;
      fThumbImage = loadImage(image, function() {
        this.className = published.CLS_GALLERY_IMAGE;
        this.id = published.CLS_GALLERY_IMAGE + item.id();
        fThumb.appendChild(this);
        fThumb.className = published.CLS_GALLERY_THUMBNAIL + ' '
          + ((this.width >= this.height) ? published.CLS_IMAGE_BOX_WIDER : published.CLS_IMAGE_BOX_HIGHER);
      }, function () {
        fThumb.className = published.CLS_GALLERY_THUMBNAIL + ' ' + published.CLS_IMAGE_BOX_ERROR;
      }, gallery.urls);
      fUpdate();
    }

    function updateContent(content, isText) {
      if (isText) fContent.innerText = content;
      else fContent.innerHTML = content;
    }

    function updateTitle(title) { fThumb.title = title; }
  };

  var fUpdateCurrentWithEvent = withEvent(gallery, published.EVT_GALLERY_CHANGE, updateCurrent);

  gallery.current = function (id) {
    if (typeof(id) !== tUNDEFINED) {
      var current = fUpdateCurrentWithEvent(id);
      if (typeof(current) !== tUNDEFINED) fCurrent = current;
    }
    if (typeof(fCurrent) !== tUNDEFINED) return fCurrent.id();
  };
  gallery.has = has;
  gallery.get = get;
  var fRemoveWithEvent = withEvent(gallery, published.EVT_GALLERY_REMOVE, remove);
  gallery.remove = function (id) {
    if (!has(id)) return true;
    return !!fRemoveWithEvent(id);
  };
  gallery.images = function () { return fItems; };
  gallery.update = withEvent(gallery, published.EVT_GALLERY_ADD, updateItem);

  WAVE.each(init.images, updateItem);

  fUpdate();

  return gallery;

  function build(parent) {
    var
      thumbnails = document.createElement('ul'),
      prev = document.createElement('div'),
      next = document.createElement('div');
    thumbnails.className = published.CLS_GALLERY_LIST;
    prev.className = published.CLS_GALLERY_PREV;
    next.className = published.CLS_GALLERY_NEXT;
    prev.onclick = function (e) {
      e.preventDefault();
      var sz = size(thumbnails.firstChild);
      scroll(thumbnails, { x: -sz.x, y: -sz.y}, 400, undefined, update);
    };
    next.onclick = function (e) {
      e.preventDefault();
      var sz = size(thumbnails.firstChild);
      scroll(thumbnails, sz, 400, undefined, update);
    };
    makeDraggable(thumbnails, undefined, update, update);
    parent.appendChild(prev);
    parent.appendChild(thumbnails);
    parent.appendChild(next);
    window.addEventListener('resize', update, false);
    fUpdate = update;
    return thumbnails;
    function update() {
      prev.className = published.CLS_GALLERY_PREV
        + ((fThumbnails.scrollLeft === 0 && fThumbnails.scrollTop === 0) ? ' ' + published.CLS_GALLERY_DISABLED : '');
      next.className = published.CLS_GALLERY_NEXT
        + ((fThumbnails.scrollLeft + fThumbnails.clientWidth === fThumbnails.scrollWidth
          && fThumbnails.scrollTop + fThumbnails.clientHeight === fThumbnails.scrollHeight) ? ' ' + published.CLS_GALLERY_DISABLED : '');
    }
  }

  function updateItem(item) {
    var it;
    if (has(item.id)) {
      it = get(item.id);
      it.update(item);
    } else {
      it = new gallery.Item(item, gallery, fPreview);
      it.order = item.order|0;
      var idx = insertAt(fItems, it, orderComp);
      fThumbnails.insertBefore(it.element(), WAVE.has(fItems, idx) ? fItems[idx].element() : null);
      fItems.splice(idx, 0, it);
      fItemsLookup[it.id()] = it;
    }
    return it;
  }

  function has(id) {
    if (typeof(id) === tUNDEFINED) return false;
    return WAVE.has(fItemsLookup, id);
  }

  function get(id) { return fItemsLookup[id]; }

  function updateCurrent(id) {
    if (!has(id)) return;
    var item = get(id);

    if (typeof(fCurrent) !== tUNDEFINED)
      fCurrent.element().className = published.CLS_GALLERY_ITEM;
    item.element().className = published.CLS_GALLERY_ITEM + ' ' + published.CLS_GALLERY_SELECTED;
    item.show();
    return item;
  }

  function remove(id) {
    if (!has(id)) return true;
    get(id).free();
    fItems = fItems.filter(function (it) { return it.id() !== id; });
    delete fItemsLookup[id];
    return true;
  }

  function insertAt(sorted, val, comp) {
    var low = 0, hig = sorted.length;
    var mid = -1, c = 0;
    while(low < hig) {
      mid = ((low + hig)/2) | 0;
      c = comp(sorted[mid], val);
      if (c < 0) low = mid + 1;
      else if ( c > 0) hig = mid;
      else  {
        while (mid < hig && comp(sorted[mid], val) === 0) mid++;
        return mid;
      }
    }
    return low;
  }

  function orderComp(a, b) { return a.order - b.order; }

  function size(elm) { return { x: elm.offsetWidth, y: elm.offsetHeight }; }
  function animate(opts) {
    var start = Date.now();
    var id = setInterval(function () {
      var progress = (Date.now() - start) / (+opts.duration || 1000);
      if (progress > 1) progress = 1;
      var cont = opts.step((opts.delta || def)(progress)) || true;
      if (progress === 1 || !cont) clearInterval(id);
    }, +opts.delay || 10);
    function def(progress) { return progress; }
  }
  function scroll(elm, to, duration, delta, onstep) {
    var scr = { x: elm.scrollLeft, y: elm.scrollTop };
    animate({
      duration: duration,
      delta: delta,
      step: function (delta) {
        elm.scrollLeft = scr.x + to.x * delta;
        elm.scrollTop = scr.y + to.y * delta;
        if (onstep)
          onstep.call(elm, delta);
    } });
  }

  function withEvent(that, evt, func) {
    return function () {
      var evtArgs = { evt: evt, phase: published.EVT_PHASE_BEFORE, args: arguments, abort: false };
      that.eventInvoke(evt, evtArgs);
      if (evtArgs.abort === true) return;

      var result = func.apply(that, arguments);

      evtArgs.result = result;
      evtArgs.phase = published.EVT_PHASE_AFTER;
      delete evtArgs.abort;
      that.eventInvoke(evt, evtArgs);
      return result;
    };
  }
};//Gallery

published.GallerySimple = function (init) {
  if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "GallerySimple.ctor(init.DIV)";
  var fDIV = WAVE.isString(init.DIV) ? WAVE.id(init.DIV) : init.DIV;
  var preview = document.createElement('div');
  preview.className = published.CLS_GALLERY_SIMPLE_PREVIEW;
  init.preview = new published.ImageView({ DIV: preview }, init.urls);
  init.thumbnails = document.createElement('div');
  init.thumbnails.className = published.CLS_GALLERY_SIMPLE_THUMBNAILS;
  fDIV.appendChild(init.thumbnails);
  fDIV.appendChild(preview);
  return published.Gallery.call(this, init);
};//GallerySimple