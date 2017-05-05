    published.getCookie = function(name) {
      var matches = document.cookie.match(new RegExp(
        "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
      ));
      return matches ? decodeURIComponent(matches[1]) : false;
    };

    published.setCookie = function(name, value) {
      var cookie = name+"="+escape(value)+"; path=/";
      document.cookie = cookie;
    };

    published.deleteCookie = function(name) {
      published.setCookie(name, null, { expires: -1 });
    };

    var platform =
    {
      iPhone: navigator.userAgent.match(/iPhone/i),
      iPod: navigator.userAgent.match(/iPod/i),
      iPad: navigator.userAgent.match(/iPad/i),
      Android: navigator.userAgent.match(/Android/i),
      IE: navigator.appName.indexOf("Microsoft") !== -1,
      IEMobile: navigator.userAgent.match(/IEMobile/i),
      WinPhone: /windows phone/i.test(navigator.userAgent),
      Chrome: !!window.chrome, // navigator.userAgent.match(/Chrome/i),
      Safari: navigator.userAgent.match(/Safari/i) && !window.chrome,
      FireFox: navigator.userAgent.indexOf("Firefox") > -1,
      BlackBerry: navigator.userAgent.match(/BlackBerry/i),
      WebOS: navigator.userAgent.match(/webOS/i),
      Opera: window.opera, // navigator.userAgent.indexOf("Presto") > -1
      OperaMini: navigator.userAgent.match(/Opera Mini/i),
      OperaMobi: navigator.userAgent.match(/Opera Mobi/i),
      Silk: /silk/i.test(navigator.userAgent)
    };

    platform.iOS    = platform.iPhone || platform.iPod || platform.iPad;

    platform.Mobile = platform.iOS || platform.Android ||
                      platform.OperaMini || platform.OperaMobi ||
                      platform.BlackBerry ||
                      platform.WebOS ||
                      platform.IEMobile || platform.WinPhone ||
                      platform.Silk;

    platform.WebKit = platform.Chrome || platform.Safari;

    published.Platform = platform;

//Ajax
    function ajaxCall(verb, url, data, success, error, fail, a, ct) {
      a = published.strDefault(a);
      ct = published.strDefault(ct);

      var xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject('Microsoft.XMLHTTP');
      xhr.open(verb, url);

      if (WAVE.isFunction(fail))
        xhr.onerror = fail;

      xhr.onreadystatechange = function() {
        if (xhr.readyState === 4) {
          if (xhr.status === 200) {
            if (WAVE.isFunction(success))
              success(xhr.responseText);
          } else {
            if (WAVE.isFunction(error))
              error(xhr.responseText);
          }
        }
      };
      if (!published.strEmpty(a)) xhr.setRequestHeader('Accept', a);
      if (!published.strEmpty(ct)) xhr.setRequestHeader('Content-Type', ct);

      if (data !== null) {
        if (published.isObject(data) && !published.strEmpty(ct) && ct.indexOf("application/json") !== -1)
          data = JSON.stringify(data);
        xhr.send(data);
      }
      else
        xhr.send();

      return xhr;
    }

    published.ajaxCall = function(verb, url, data, success, error, fail, a, ct) {
      ajaxCall(verb, url, data, success, error, fail, a, ct);
    };

    published.ajaxGet = function(url, success, error, fail, a) {
      ajaxCall("GET", url, null, success, error, fail, a);
    };

    published.ajaxGetJSON = function(url, data, success, error, fail) {
      ajaxCall("GET", url, data, success, error, fail, published.CONTENT_TYPE_JSON, published.CONTENT_TYPE_JSON_UTF8);
    };

    published.ajaxPost = function(url, data, success, error, fail, a, ct) {
      ajaxCall("POST", url, data, success, error, fail, a, ct);
    };

    published.ajaxPostJSON = function(url, data, success, error, fail) {
      ajaxCall("POST", url, data, success, error, fail, published.CONTENT_TYPE_JSON, published.CONTENT_TYPE_JSON_UTF8);
    };

    published.ajaxPatch = function (url, success, error, fail, a) {
      ajaxCall("PATCH", url, null, success, error, fail, a);
    };

    published.ajaxPatchJSON = function (url, data, success, error, fail) {
      ajaxCall("PATCH", url, data, success, error, fail, published.CONTENT_TYPE_JSON, published.CONTENT_TYPE_JSON_UTF8);
    };


    //call func when dom is loaded
    published.onReady = function(func) {
      if (!published.isFunction(func)) return;

      if (document.readyState !== 'loading'){
        func();
      } else if (published.isFunction(document.addEventListener)) {
        document.addEventListener('DOMContentLoaded', func);
      } else if (published.isFunction(document.attachEvent)) {
        document.attachEvent('onreadystatechange', function() {
          if (document.readyState !== 'loading')
            func();
        });
      }
    };

    published.copyToClipboard = function(value) {
      if (typeof(value) === tUNDEFINED || value === null) return;

      var txtEl,
          removeTxt = false;
      if (published.isString(value)) {
        if (published.strEmpty(value)) return;

        removeTxt = true;
        txtEl = document.createElement("p");
        txtEl.style.border = '0';
        txtEl.style.padding = '0';
        txtEl.style.margin = '0';
        txtEl.style.position = 'absolute';
        txtEl.style.left = '-9999px';
        txtEl.style.top = '-9999px';
        txtEl.setAttribute('readonly', '');
        txtEl.innerHTML = value;
        document.body.appendChild(txtEl);
      } else {
        txtEl = value;
      }

      var succeeded;
      try {
        published.selectElement(txtEl);
        succeeded = document.execCommand("copy");
      }
      catch (err) {
        succeeded = false;
      }

      if (removeTxt)
        document.body.removeChild(txtEl);
      txtEl = null;
      return succeeded;
    };

    published.selectElement = function(element) {
      var range,
          selection;

      if (document.body.createTextRange) {
        range = document.body.createTextRange();
        range.moveToElementText(element);
        range.select();
      } else if (window.getSelection) {
        selection = window.getSelection();
        range = document.createRange();
        range.selectNodeContents(element);
        selection.removeAllRanges();
        selection.addRange(range);
      }
    };

    published.getScrollBarWidth = function() {
      var scrollDiv = document.createElement("div");
      scrollDiv.style.height = "100px";
      scrollDiv.style.position = "absolute";
      scrollDiv.style.top = "-10000px";
      scrollDiv.style.width = "100px";
      scrollDiv.style.overflow = "scroll";

      document.body.appendChild(scrollDiv);
      var scrollbarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth;
      document.body.removeChild(scrollDiv);
      return scrollbarWidth;
    };