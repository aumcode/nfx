//returns true if an element is a direct or indirect child of the specified parent
published.isParentOf = function(parent, elem){
  if (published.isFunction(parent.contains)) {
    try {return parent.contains(elem);}
    catch(e) {return false;}
  }

  var node = elem;
  while(true){
    node = node.parentNode;
    if(node === null) return false;
    if(node === parent) return true;
  }
};

//returns computed value of specified css style for given elemen
published.styleOf = function(elem, cssStyle){
  if (typeof(elem) === tUNDEFINED || elem === null || published.strEmpty(cssStyle) || !published.isFunction(window.getComputedStyle))
    return "";

  return window.getComputedStyle(elem, null).getPropertyValue(cssStyle);
};

//removes html element with given id
published.removeElem = function(id) {
  var el = published.id(id);
  if (el !== null){
    el.parentNode.removeChild(el);
    return true;
  }
  return false;
};

published.removeChildren = function(el) {
  while (true) {
    var lc = el.lastChild;
    if (lc) el.removeChild(lc);
    else break;
  }
};

published.addClass = function(elem, className) {
  if (typeof(elem) === tUNDEFINED || elem === null || published.strEmpty(className)) return;

  published.removeClass(elem, className);
  elem.className += (" " + className);
};

published.removeClass = function(elem, className) {
  if (typeof(elem) === tUNDEFINED || elem === null || published.strEmpty(className) || typeof(elem.className) === tUNDEFINED) return;

  elem.className = elem.className.replace(new RegExp('(?:^|\\s)' + className + '(?!\\S)', "g") , '' );
};

published.fullHieght = function(elem) {
  if (typeof(elem) === tUNDEFINED || elem === null) return;

  return elem.clientHeight +
          parseFloat(published.styleOf(elem, "margin-top")) +
          parseFloat(published.styleOf(elem, "margin-bottom")) +
          parseFloat(published.styleOf(elem, "padding-top")) +
          parseFloat(published.styleOf(elem, "padding-bottom")) +
          parseFloat(published.styleOf(elem, "border-top-width")) +
          parseFloat(published.styleOf(elem, "border-bottom-width"));
};

published.fullWidth = function(elem) {
  if (typeof(elem) === tUNDEFINED || elem === null) return;

  return elem.clientWidth +
          parseFloat(published.styleOf(elem, "margin-left")) +
          parseFloat(published.styleOf(elem, "margin-right")) +
          parseFloat(published.styleOf(elem, "padding-left")) +
          parseFloat(published.styleOf(elem, "padding-right")) +
          parseFloat(published.styleOf(elem, "border-left-width")) +
          parseFloat(published.styleOf(elem, "border-right-width"));
};

published.addEventHandler = function(object, event, handler, useCapture) {
  if (typeof(object) === tUNDEFINED || object === null) return;

  if (published.isFunction(object.addEventListener))
    object.addEventListener(event, handler, useCapture === true);
  else if (published.isFunction(object.attachEvent))
    object.attachEvent("on" + event, handler);
  else
    object["on"+event] = handler;
};

published.removeEventHandler = function(object, event, handler, useCapture) {
  if (typeof(object) === tUNDEFINED || object === null) return;

  if (published.isFunction(object.removeEventListener))
    object.removeEventListener(event, handler, useCapture === true);
  else if (published.isFunction(object.detachEvent))
    object.detachEvent("on" + event, handler);
  else
    object["on"+event] = null;
};

published.id = function(id){
  return document.getElementById(id);
};

published.ce = function(tag){
  return document.createElement(tag);
};

published.getRadioGroupValue = function(groupName){
  var group = document.getElementsByName(groupName);
  for (var i = 0; i < group.length; i++) {
    if (group[i].checked) {
      return group[i].value;
    }
  }
};