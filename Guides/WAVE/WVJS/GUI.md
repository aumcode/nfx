# WAVE.GUI


## currentDialog()
Returns currently active dialog or null.

## curtainOn/curtainOff()
Show/hide full page modal shadow. Class name = "wvCurtain".
```HTML
<button onclick="WAVE.GUI.curtainOn(); setTimeout(function(){WAVE.GUI.curtainOff();}, 2700);">Curtain</button>
```

## Dialog(init)
Create instance of dialog window. "wvDialogBase", "wvDialogTitle", "wvDialogContent" classes must be defined for pretty view. This is "low-level" way to show dialog box. `showConfirmationDialog()` is more suitable for the most cases.  
init: `{title: 'Title', content: 'html markup', cls: 'className', widthpct: 75, heightpct: 50, onShow: function(dlg){}, onClose: function(dlg, result){return true;}}`
```HTML
<style>
.wvDialogBase {
            display: block;
            position: fixed;
            background: #3866de;
            border: 1px solid #808080;
            border-radius: 4px;
            padding: 6px;
            color: white;
            box-shadow: 6px 6px 10px #888888;
        }
.wvDialogTitle {
    background: #2020c0;
    color: white;
    font-size: 1.37em;
    font-weight: bold;
    padding: 4px;
    border-radius: 4px;
}
.wvDialogContent {
            display: block;
            background: #fefef0;
            color: black;
            border: 1px solid #7070ff;
            padding: 4px;
            margin-top: 6px;
            border-radius: 4px;
            overflow: auto;
        }
.dlgGreen {
    background: linear-gradient(to bottom, #bfd255 0%,#8eb92a 50%,#588701 51%,#9ecb2d 100%);
}
    .dlgGreen > .wvDialogTitle {
        background: linear-gradient(to bottom, #45484d 0%,#000000 100%);
    }
</style>
<body>
<script>
function showDialog(){
   var dlg = new WAVE.GUI.Dialog({
            title: 'This is a test dialog',
            content: 'First line<br>The second one<br>'+
                     'One more<br>'+
                     '<button onclick="WAVE.GUI.currentDialog().cancel()">Dismiss</button>',
            widthpct: 75,
            heightpct: 90,
            });
};

function showManyDialogs(){
            var dlg = new WAVE.GUI.Dialog({
                    title: 'Root dialog',
                    content: 'This is an example of how to show one dialog from another.<br>'+
                             'When you click on "Show Another" it will display yet another dialog box on top of this one.<br>'+
                             'This behavior may be necessary in the nested applications, for example<br>'+
                             'when a confirmation box needs to be shown<hr>'+
                             '<button onclick="showDialog()">Show Another</button>&nbsp;'+
                             '<button onclick="WAVE.GUI.currentDialog().cancel()">Close This</button>',
                    cls: "dlgGreen"
                    });
};                    
</script>                    
<button onclick="showDialog()">Show Dialog</button>
<button onclick="showManyDialogs()">Show Many</button>
</body>
```

## IBox(container, object init)
Class that implements "thumbnail + view-image"  control. `container` is element or string with element id.
```HTML
<style>
.wvIBoxDivFrame {
            height: 500px;
            width: 400px;
        }
.wvIBoxDivMainImage {
    outline: 2px solid #e0e0e0;
    background-color: #f4f4f4;
}
</style>
<body>
<div id="divIBox"></div>
<script>
var init =
    {
        images: {
            "d": "https://cdn.tutsplus.com/vector/uploads/2013/11/chris-flower-600.png",
            "g": "https://www.google.ru/images/nav_logo225.png",
            "1": "https://www.google.ru/images/srpr/logo11w.png",
            "2": "http://simpleicon.com/wp-content/uploads/phone-symbol-1.png",
            "3": "http://www.cliparthut.com/clip-arts/910/green-play-icon-910267.gif"
        },
        thumbs: [
            { bigSrc: "1", thumbSrc: "g", h: 37, w: 95, l: 0, t: -41, title: "Some Text" },
            { bigSrc: "2", thumbSrc: "g", h: 18, w: 15, l: -13, t: -88, title: "<strong>Telephone</strong>" },
            { bigSrc: "3", thumbSrc: "g", h: 28, w: 28, l: -138, t: -41 },
            { bigSrc: "3" },
            { thumbSrc: "g", h: 18, w: 15, l: -13, t: -88 }
        ],
        defaultImageSrc: "d",
        defaultThumbSrc: "d",
        thumbsPosition: "bottom",
        thumbsScrollDelta: 0.7
    };

var imageBox = new WAVE.GUI.IBox('divIBox', init);
</script>
</body>
```

## Tree(object init)
Class that implements tree view control.
```HTML
<style>
.wvTreeNodeButtonExpanded {
  display: table-cell;
  color: red;
  font-size: 2em;
  cursor: pointer;
}

.wvTreeNodeButtonCollapsed {
  display: table-cell;
  color: black;
  font-size: 1.5em;
  cursor: pointer;
}

.wvTreeNodeContent {
  display: table-cell;
}

.wvTreeNodeOwn, .wvTreeNodeChildren {
  display: block;
}

.wvTreeNodeOwnSelected {
  display: table;
  background: linear-gradient(0deg, rgba(160, 170, 255, 0.7), rgba(240, 240, 255, 0.9), rgba(160, 170, 255, 0.3));
  font-weight: bold;
}
</style>
<body>
 <div id="tv1" style="width: 15%; height: 200px; border: 1px solid #000; overflow: auto;"></div>
 <button id="btnRemove11" style="margin-top: 20px;">Remove 1.1</button>
<script>
var tree = new WAVE.GUI.Tree({DIV: WAVE.id("tv1"), treeSelectionType: WAVE.GUI.TREE_SELECTION_SINGLE, supressEvents: true});
rootNode1 = tree.root().addChild({html: "1 Root Node", id: 1});
rootNode2 = tree.root().addChild({html: "2 Root Node", id: 2});

childNode11 = rootNode1.addChild({html: "1.1 <strong>Child</strong> Node", id: 11, selectable: false});
childNode111 = childNode11.addChild({html: "<img src='http://itadapter.com/img/menucontacthotslice.png' style='width:35px;height:24px;'><input type='checkbox'>1.1.1 Child Node <br> another line", id: 111});
childNode112 = childNode11.addChild({html: "<img src='http://itadapter.com/img/menuserviceshotslice.png' style='width:35px;height:24px;'><input type='checkbox'>1.1.2 Child Node", id: 112});

childNode21 = rootNode2.addChild({html: "2.1 <strong>Child</strong> Node", id: 21, selectable: false});
childNode22 = rootNode2.addChild({html: "2.2 <strong>Child</strong> Node", id: 22});

$("#btnRemove11").click(function() {
      childNode11.remove();
    });
</script>
</body>
```

## toast(string msg, string type, int duration)
Shows "toast" with message `msg` that disappears after `duration` in ms (defaule 2500). Parameter `type` allows change style of message box.
```HTML
<style>
.wvToast {
    display: block;
    position: fixed;
    background: black;
    border: 1px solid #808080;
    width: auto;
    padding: 8px;
    top: 45%;
    left: 50%;
    color: white;
    box-shadow: 2px 2px 10px #888888;
}
.wvToast_warning {
    display: block;
    position: fixed;
    background: yellow;
    border: 1px solid #bcbc00;
    width: auto;
    padding: 8px;
    top: 45%;
    left: 50%;
    color: black;
    box-shadow: 2px 2px 10px #888888;
}
.wvToast_error {
    display: block;
    position: fixed;
    background: #ff2020;
    border: 1px solid #ff8080;
    width: auto;
    padding: 8px;
    top: 45%;
    left: 50%;
    color: white;
    box-shadow: 2px 2px 10px #888888;
}
</style>
...
<button onclick="WAVE.GUI.toast('Message 1')">Toast 1</button>
<button onclick="WAVE.GUI.toast('Message 2 which contains much more text in comparison to the prior message')">Toast 2</button>
<button onclick="WAVE.GUI.toast('This is a warning text','warning')">Warning</button>
<button onclick="WAVE.GUI.toast('Error message','error', 1500)">Error</button>
```

## showConfirmationDialog(title, content, buttons, callback, options)
Displays a simple confirmation prompt dialog.
```HTML
<style>
/* style is the same with one in Dialog() example*/
</style>
...
<script>
function showConfirmationDialog(){
            WAVE.GUI.showConfirmationDialog(
                'Action confirmation', 
                'Are you <strong>very</strong> sure?',
                [WAVE.GUI.DLG_YES, WAVE.GUI.DLG_NO, WAVE.GUI.DLG_CANCEL],
                function(sender, result) {
                   alert('You chose: ' + result);
                   return true; 
                }, {cls: 'dlgGreen'});
};      
</script>
...
<button onclick="showConfirmationDialog();">Show Confirmation Dialog</button>
```
