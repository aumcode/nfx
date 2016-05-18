"use strict";
/*jshint devel: true,browser: true, sub: true */ 
/*global WAVE: true,$: true */

/*!
 * Wave Java Script Library Default UI v2.0.0 
 *
 * Based on IT Adapter JS Library 2002-2011 
 * License: Unrestricted use/modification with mandatory reference to IT Adapter Corp. as the original author  
 * (c) 2002-2011, 2013-2014 IT Adapter Corp.
 * http://www.itadapter.com/
 * Authors: Dmitriy Khmaladze
 * Revision Epoch:  May 9, 2014
 */

WAVE.GUI = (function(){

    var tUNDEFINED = "undefined";

    var published = { 
        TUNDEFINED: tUNDEFINED,
        CLS_TOAST: "wvToast",
        CLS_CURTAIN: "wvCurtain",
        CLS_DIALOG_BASE: "wvDialogBase",
        CLS_DIALOG_TITLE: "wvDialogTitle",
        CLS_DIALOG_CONTENT: "wvDialogContent",
        CLS_DIALOG_CONFIRM_FOOTER: "wvConfirmDialogFooter",
        CLS_DIALOG_CONFIRM_BUTTON: "wvConfirmDialogButton",

        DLG_UNDEFINED: '',
        DLG_CANCEL: 'cancel',
        DLG_OK: 'ok',
        DLG_YES: 'yes',
        DLG_NO: 'no',

        EVT_DIALOG_CLOSE: 'dlg-close',
        EVT_PUZZLE_KEYPAD_CHANGE: 'puzzle-change',

        // Tree {
        CLS_TREE_NODE: "wvTreeNode",
        CLS_TREE_NODE_OWN_SELECTED: "wvTreeNodeOwnSelected",
        CLS_TREE_NODE_BUTTON_EXPANDED: "wvTreeNodeButtonExpanded",
        CLS_TREE_NODE_BUTTON_COLLAPSED: "wvTreeNodeButtonCollapsed",
        CLS_TREE_NODE_CONTENT: "wvTreeNodeContent",
        CLS_TREE_NODE_OWN: "wvTreeNodeOwn",
        CLS_TREE_NODE_CHILDREN: "wvTreeNodeChildren",

        EVT_PHASE_BEFORE: "before",
        EVT_PHASE_AFTER:  "after",

        
        EVT_TREE_NODE_ADD: 'tree-node-add',
        EVT_TREE_NODE_REMOVE: 'tree-node-remove',

        EVT_TREE_NODE_CONTENT: 'tree-node-content',
        EVT_TREE_NODE_DATA: 'tree-node-data',

        EVT_TREE_NODE_EXPANSION: 'tree-node-expansion',
        EVT_TREE_NODE_SELECTION: 'tree-node-selection',

        EVT_RULER_MOUSE_MOVE: 'ruler-mouse-move',
        EVT_RULER_MOUSE_ENTER: 'ruler-mouse-enter',
        EVT_RULER_MOUSE_LEAVE: 'ruler-mouse-leave',

        TREE_SELECTION_NONE: 0,
        TREE_SELECTION_SINGLE: 1,
        TREE_SELECTION_MULTI: 2,
        // } Tree

        // ImageBox {   
        // classes               
        CLS_IBOX_DIV_FRAME: "wvIBoxDivFrame",
        CLS_IBOX_DIV_MAINIMAGE: "wvIBoxDivMainImage",
        CLS_IBOX_IMG_MAINIMAGE: "wvIBoxImgMainImage",
        CLS_IBOX_DIV_THUMBSCONTAINER: "wvIBoxDivThumbsContainer",
        CLS_IBOX_DIV_THUMBSNAVIGATION: "wvIBoxDivThumbsNavigation",
        CLS_IBOX_DIV_THUMBSIMAGESCONTAINER: "wvIBoxDivThumbsImagesContainer",
        CLS_IBOX_DIV_THUMB: "wvIBoxDivThumb",
        CLS_IBOX_DIV_THUMB_CROP: "wvIBoxDivThumbCrop",
        CLS_IBOX_DIV_THUMB_IMAGE: "wvIBoxDivThumbImage",

        // default styles
        STL_DIV_FRAME: "",
        STL_IMG_MAINIMAGE: "position: relative; display: block;",
        STL_DIV_MAINIMAGE_V: "width: 82%;" +
                             "height: 100%;" +
                             "margin: 0 1%;" +
                             "overflow: hidden;",
        STL_DIV_MAINIMAGE_H: "width: 100%;" +
                             "height: 82%;" +
                             "margin: 1% 0;" +
                             "overflow: hidden;",
        STL_DIV_THUMBSCONTAINER_V: "width: 15%;" +
                                   "height: 100%;",
        STL_DIV_THUMBSCONTAINER_H: "width: 100%;" +
                                   "height: 15%;",
        STL_DIV_THUMBSNAV_V: "width: 100%;" +
                             "height: 5%;" +
                             "display: none;",
        STL_DIV_THUMBSNAV_H: "width: 5%;" +
                             "height: 100%;" +
                             "display: none;",
        STL_IMG_THUMBSNAV: "display: block;" +
                           "position: relative;",
        STL_DIV_THUMBSIMAGESCONTAINER_V: "width: 100%;" +
                                         "height: 100%;" +
                                         "overflow: hidden;",
        STL_DIV_THUMBSIMAGESCONTAINER_H: "width: 100%;" +
                                         "height: 100%;" +
                                         "overflow: hidden;" +
                                         "display: inline-block;" +
                                         "white-space: nowrap; " +
                                         "vertical-align: top;",
        STL_DIV_THUMB_V: "width: 100%;" +
                         "margin: 3px 0;",
        STL_DIV_THUMB_H: "height: 100%;" +
                         "margin: 0 3px;" +
                         "display: inline-block;" +
                         "vertical-align: top;",
        STL_DIV_THUMB_CROP: "overflow: hidden;" +
                            "position:relative;",
        STL_DIV_THUMB_IMAGE: "position: relative;",
        STL_DISABLE_MOBILE_HANDLING: "-webkit-touch-callout: none;" +
                                     "-webkit-user-select: none;" +
                                     "-khtml-user-select: none;" +
                                     "-moz-user-select: none;" +
                                     "-ms-user-select: none;" +
                                     "user-select: none;", 

        // events
        EVT_IBOX_IMAGE_CHANGED: "ibox-image-changed",
        EVT_IBOX_ERROR: "ibox-error",
        
        // constants
        POS_LEFT:   "left",
        POS_RIGHT:  "right",
        POS_TOP:    "top",
        POS_BOTTOM: "bottom",

        // default values
        DEFAULT_THUMBS_POSITION: "left",
        DEFAULT_IMAGE_FADEIN_TIME: 500,
        DEFAULT_IMAGE_FADEOUT_TIME: 0, 
        DEFAULT_THUMBS_SCROLL_DELTA: 0.5,
        DEFAULT_ARROWS_OPACITY: 0.3,
        DEFAULT_IMAGE:
          "data:image/png;base64," +
          "iVBORw0KGgoAAAANSUhEUgAAAgAAAAIACAMAAADDpiTIAAAABGdBTUEAALGPC/xhBQAAAwBQTFRFAAAAPD" +
          "w8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
          "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAL+bwigAAAQB0Uk5T////////////////////////////////" +
          "//////////////////////////////////////////////////////////////////////////////////" +
          "//////////////////////////////////////////////////////////////////////////////////" +
          "//////////////////////////////////////////////////////////////////////////////////" +
          "//////////////////////////////////////////////////////////////AFP3ByUAAAAJcEhZcwAA" +
          "FiQAABYkAZsVxhQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNvyMY98AABAiSURBVHhe7dfreu" +
          "M6rkXR7vd/6NqOM1NlJ76QEgAyWGv86sgSUKLm6a/P//6YNAcgzgGIcwDiHIA4ByDOAYhzAOIcgDgHIM4B" +
          "iHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhzAOIcgDgHIM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhzAOIcgD" +
          "gHIM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhzAOIcgDgHIM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhz" +
          "AOIcgDgHIM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhzAOIcgDgHIM4BiHMA4hyAOAcgzgGIcwDiHIA4By" +
          "BOO4D/f+A/i5IO4Pr9xQtQDoDvr12AcAB8/Q9cUSQbAJ/+C1f1qAbAd/+H63JEA+Cr3+IXNZoB8M3v8ZsY" +
          "yQD44t/xqxbFAPjeP/G7FMEA+NqPcIcSvQD41o9xjxC5APjSz3CXDrEA+MyvcKcKrQD4xq9xrwipAPjC73" +
          "C3BqUA+L7vcb8EoQD4uiN4QoFOAHzbMTwjQCYAvuwonupPJQC+6ziea08kAL7qDJ7sTiMAvukcnm1OIgC+" +
          "6Cye7k0hAL7nPJ5vTSAAvuYRTOisfwB8y2OY0ZgDeIkZjbUPgC95FFP66h4A3/E45rTVPAC+4hlM6qp3AH" +
          "zDc5jVlAN4i1lNtQ6AL3gW03rqHADf7zzmteQABjCvpcYB8PUiMLGjvgHw7WIwsyEHMISZDbUNgC8Xhan9" +
          "dA2A7xaHue04gEHMbadpAHy1SEzuxgGMYnI3DmAUk7txAKOY3E3PAPhmsZjdjAMYxuxmHMAwZjfTMgC+WD" +
          "Sm9+IAxjG9Fwcwjum9dAyA7xWP+a04gAnMb8UBTGB+Kw5gAvNbaRgAXysDGzpxADPY0IkDmMGGThzADDZ0" +
          "4gBmsKETBzCDDZ04gBls6KRfAHyrHOxoxAFMYUcjDmAKOxpxAFPY0YgDmMKORhzAFHY04gCmsKMRBzCFHY" +
          "04gCnsaMQBTGFHIw5gCjsacQBT2NHIVgFwyjf4YQZP5mDHDJ68wQ972CQAjuYJbhrDMznYMYZnnuCm1dYH" +
          "wHm8xe1vcXsOdrzF7W9x+0KLA+AcBvHQa9ybgx2vce8gHlplZQCcwBQefYU7M7DhFe6cwqNLLAuAdz+AAU" +
          "9xWwY2PMVtBzCg3poAeOujmPIEN2VgwxPcdBRTii0IgPc9h1mPcEcGNjzCHecwq1J5ALzqaYx7gBsysOEB" +
          "bjiNcXVqA+AtYzDzB37OwIYf+DkGM4sUBsD7BWLwN/yYgQ3f8GMgBleoCoA3i8b0e/wWj/n3+C0a09PVBM" +
          "BLJWDBHX6Kx/w7/JSABclKAuCNcrDjBj/EY/4NfsjBjly/P4Cf58T1eMz/h+tJWJKrIgDeJw97/uJyNKb/" +
          "xeU87EnVIoDvJ8XVaEz/wtVELErVI4BvR8XFaEwHFzOxKVWTAO7PimvRmP6Ja6lYlarB/wj8xKpPXIvF7E" +
          "9cS8WqXG0CuDsuLsVi9hWXcrErV58Asj8Qk6+4lIxluRoFcHtgXInE5A9cyca2XJ0CuDkxLkRi8gUX0rEu" +
          "V0kAHQpg7gUX0rEuWa8A/h0af8dh7op3SdUsgLTvxNQlr5KqWwB/j40/ozB1xZvkahtA7E5mOoCjeKcKbM" +
          "wJgD8rsDFbvwAyPhYTl7xGMgcwgokO4DBeqgY745Yyb81bJCsKYMnZ8dd50fNGsDNd5wCiljLNAZzBaxWJ" +
          "XRo7bRBL07UOIGYrsxzAKbxWFbZGrGXSqjfIVhXAov/74c8zmLToBdI1DSDuszFn1b8/XfcAzu5ligM4jR" +
          "crw9qTi5mx8J+fzQG8xAwHEIA3K8PaU4uZ0Pj7KwRwfDPPX3ChDGsLKARwdDVPf+BKGdYWqAtgswL45QtX" +
          "b/HLB66UYW0FjQDudnPpEe644tIVl8qwtoJIAF/L+euVR3dyrQxrK6gE8LGd//Teo4dLsbZCYQC/9xiZV4" +
          "a1JRzAAOaVYW0JBzCAeWVYW8IBDGBeGdaWqAyg+iDZeh7zqrC1RucAok6SaWVYW8MBvMe0MqytURrAL/3v" +
          "UqZVYWuRzgGw9DzmFWFpkdoAKo+SjTGYWYGNVdoGwMIoTC3AwirFAZQdJOviMDcd68o0DYBtkZicjW1lqg" +
          "OoOUd2xWJ2LnbVaRkAq6IxPRWr6pQHUHCMLIrH/EQsKtQwAPZkYEMe9hSqDyD9FFmTgQ1pWFOpXwBsycGO" +
          "LGyptCCA3FNkRxa25GBHqRUBZJ4iG/KwJwMbai0JIPEUWZCHPQlYUKxZAMzPxKZ4zC+2JoCsU2R6LnZFY3" +
          "q1RQEknSLDc7ErGMPLrQog5RgZnY1toRhdb1kAGcfI5Gxsi8TkBdYFEH+OzM3HvjjMXWFhAOHnyNh87AvD" +
          "2CVWBhB8kAytwMYgDF1jaQCxB8nMCmyMwcxF1gYQeZJMrMHOCExcZXEAgSfJwBrsDMDAZVYHEHeUzKvBzv" +
          "OYt87yAKLOkmlV2HoW0xZaH0DQYTKrClvPYdZSOwQQcZpMqsPeM5i01hYBBBwnc+qw9zjmrLZJAKfPkzF1" +
          "2HsYY5bbJYCzJ8qQOuw9iCEb2CeAU2fKhEpsPoIJW9gpgBNnyoBKbD6AAXvYKoDjp8rjldg8jcd3sVkAF5" +
          "zTHJ6txOY5PLuR/QI4dLQ8WYnNM3hyKzsGcMGJDeOxSmwexmO72TSAC85tDM9UYvMYntnQvgFccHrvcX8t" +
          "dr/H/XvaOoArTvEJblqHf8cT3LSx/QP4xIHe4Ic98G+6wQ/b+y0BWBIHIM4BiHMA4hyAOAcg7rcEwP9zdY" +
          "Mf9sC/6QY/bG//ADjQJ7hpHf4dT3DTxrYOgFN8j/trsfs97t/TvgFwemN4phKbx/DMhjYNgHMbxmOV2DyM" +
          "x3azYwCc2AyerMTmGTy5lf0C4LDm8GwlNs/h2Y1sFgDnNI3HK7F5Go/vYqsAOKIDGFCJzQcwYA87BcD5HM" +
          "GESmw+gglb2CcADucghtRh70EM2cAuAXAwhzGmDnsPY8xymwTAqRzHnDrsPY45q20RAEdyBpPqsPcMJq21" +
          "QwCcxznMqsLWc5i11PoAOIyzmFaFrWcxbaHlAXAS5zGvBjvPY946qwPgHAIwsAY7AzBwmcUBcAoRmFiDnR" +
          "GYuMraADiDGMyswMYYzFxkaQCcQBCGVmBjEIausTIA3j8MY/OxLwxjl1gYAG8fh7n52BeHuSusC4B3j8Tk" +
          "bGyLxOQFlgXAm4didDa2hWJ0vVUB8N7BGJ6LXcEYXm5RALx1NKbnYlc0pldbEwDvHI/5mdgUj/nFmgWQf4" +
          "rsScCCYksC4I0zsCEPezKwodaKAHjfHOzIwpYc7Ci1IADeNgtbcrAjC1sq9Qsg8xTZkIY1leoD4F3zsCcD" +
          "G/Kwp1DDAPJOkfmJWFSoPADeNBWrojE9FavqtAwg5xiZnYtddaoD4D2zsS0Sk7OxrUzTAOLPkbnpWFemOA" +
          "DesgALozC1AAurtA0g9iCZWYGNVWoD4B2LsPQ85hVhaZHOAUQdJdOqsLVIaQC8YRnWnsW0Mqyt4QDeY1oZ" +
          "1tboHABbz2NeFbbWqAyA9yvD2vOYV4a1JRzAAOaVYW0JBzCAeWVYW8IBDGBeGdaWKAyAtyvDWvy48MKjh0" +
          "uxtoJKAI+uPfboTq6VYW0FkQC4dMWlR7jjiktXXCrD2goaAXDlFr984eotfvnAlTKsrVAXAO9WhrUfuDKL" +
          "pz9wpQxrCygEwIV5PH/BhTKsLSAQAH8fwQQHEIA3K8Pak4uZ0bgAB/ASMxzAebxYFbae3suUdf/+bN0D4M" +
          "/jmOMAzuLFirA0YiuTFr1AuqoAeK8qbP29AZQV0DMAlsZsZdaiV8jWOgD+Oit22iCWpmsZADuDA1jzDtk6" +
          "B8Bf50XPG8HOdEUB8FY12BkeQMsCGgbAysidTFzyGskcwAgmOoDDeKkKbIxdycwV75GsJgDeqQIbkwLoV0" +
          "DbAPgzClMdwDG8UgEWpgWw4lVSNQuAfQkbmbviXVL1CoB1F1yIw9wLLqRjXbKSAHijdKy74EIkJl9wIR3r" +
          "cnUKgG0fuBKJyR+4ko1tuRoFwLIrLkVi8hWXkrEsV58A2HXFpVjMvuJSLnblahMAqz5xLRazP3EtFatyVQ" +
          "TA+6Ri1SeuRWP6J66lYlWqJgGwCVyMxnRwMRObUvUIgEVfuBqN6V+4mohFqVoEwJ6/uByN6X9xOQ97UjX4" +
          "H4Es+Yfr8Zj/D9eTsCTX7w+AHTf4IR7zb/BDDnbkKgkg8ZxYcIef4jH/Dj8lYEGymgDSzonp9/gtHvPv8V" +
          "s0pqerCuCCNwvE4G/4MQMbvuHHQAyuUBjABe8Xg5k/8HMGNvzAzzGYWaQ2gAve8jTGPcANGdjwADecxrg6" +
          "5QEEnRWzHuGODGx4hDvOYValBQFc8L5HMeUJbsrAhie46SimFFsTwAVvfQADnuK2DGx4itsOYEC9ZQFc8O" +
          "5TePQV7szAhle4cwqPLrEygAtOYBAPvca9OdjxGvcO4qFVFgdwwTm8xe1vcXsOdrzF7W9x+0LrA7jiPJ7g" +
          "pjE8k4MdY3jmCW5abZMAPnE0N/hhBk/mYMcMnrzBD3vYKoAQnHIOdjTiAKawoxEHMIUdjTiAKexoxAFMYU" +
          "cjDmAKOxpxAFPY0YgDmMKORhzAFHY04gCmsKMRBzCFHY04gCnsaKRfAJkFsKETBzCDDZ04gBls6MQBzGBD" +
          "Jw5gBhs6cQAz2NCJA5jBhk4aBpBXAPNbcQATmN+KA5jA/FYcwATmt9IxgKwCmN6LAxjH9F4cwDim99IygJ" +
          "wCmN2MAxjG7GYcwDBmN9MzgIwCmNyNAxjF5G4cwCgmd+MARjG5m6YBxBfA3HYcwCDmttM1gOgCmNpP2wBi" +
          "C2BmQw5gCDMb6htAZAFM7KhxAHEFMK8lBzCAeS11DiCqAKb11DqAmAKY1ZQDeItZTfUOIKIAJnXVPIDzBT" +
          "Cnre4BnC2AKX21D+BcAcxozAG8xIzG+gdwpgAmdCYQwPECeL41hQCOFsDTvUkEcKwAnm1OI4AjBfBkdyIB" +
          "zBfAc+2pBDBbAE/1JxPAXAE8I0AngJkCeEKBUADjBXC/BKUARgvgbg1SAYwVwL0itAIYKYA7VYgF8DYB7t" +
          "IhF8DrArhHiF4ArwrgDiWCATwvgN+lKAbwrAB+1SIZwOMC+E2MZgCPCuAXNaIB/CyA63JUA/heAFf1yAZw" +
          "lwBXFAkH8K8A/pakHMBXAfylSTqAzwL4z6K0AzAHoM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhzAOIcgD" +
          "gHIM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhzAOIcgDgHIM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhz" +
          "AOIcgDgHIM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhzAOIcgDgHIM4BiHMA4hyAOAcgzgGIcwDiHIA4By" +
          "DOAYhzAOIcgDgHIM4BiHMA4hyAOAcgzgGIcwDiHIA4ByDOAYhzAOIcgDgHIM4BiHMA4hyAOAcg7c+f/wCx" +
          "0c2+VmqfPgAAAABJRU5ErkJggg==",
        DEFAULT_UP_NAVIGATION_IMAGE:
          "data:image/png;base64," +
          "iVBORw0KGgoAAAANSUhEUgAAACAAAAAQCAYAAAB3AH1ZAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJA" +
          "AAFiQBmxXGFAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAIlJREFUSEu9zlsOgCAQ" +
          "Q1EXxv63NQaTSYZ6QV7ycT6s0HKZ2bSUkmWaj8Cwh4+vPgLDLzru9FwPDFtoONLzXzCsoUGi91owJDTUov" +
          "drMFQ00EN7CIYRFY/QPoWho8IZ2hthmFHRCu13GFLBDrqTvQK6uJPuFR904Q9x8/i4Kx5AB054HkA/zkl2" +
          "A3fzm51wX0ZlAAAAAElFTkSuQmCC",
        DEFAULT_DOWN_NAVIGATION_IMAGE:
          "data:image/png;base64," +
          "iVBORw0KGgoAAAANSUhEUgAAACAAAAAQCAYAAAB3AH1ZAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJA" +
          "AAFiQBmxXGFAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAIxJREFUSEvFzkEOgCAM" +
          "RFEOxv2vVVOTGhi/gIJh8RZMaKcp52w7JTPbdoR3nwfsOCJ6rwMcffxD2Vkd4GhgJe2rHoEGV9AedwsCLZ" +
          "ih+wOGgRZ9oXtLGJZo4Ru6T2GoaPEI3UMwJFTQovNPMHxCRUTnWjBsocKS/u/BsIeKnf4bgeGIFeUOw1Gz" +
          "5WaWDtsHm52ye7DmAAAAAElFTkSuQmCC",
        DEFAULT_LEFT_NAVIGATION_IMAGE:
          "data:image/png;base64," +
          "iVBORw0KGgoAAAANSUhEUgAAABAAAAAgCAYAAAAbifjMAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJA" +
          "AAFiQBmxXGFAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAIZJREFUSEut0EEKxEAI" +
          "RNE5mPe/lqEWgjE/kLZm8VbtL0h+mbkWEfzwheL1QMWrgR4fD8z4aIBiweOJwoJBR1GHUaFgwlDomFixWL" +
          "FYsVixWLFYsfgD9if85Sc6I7eBzchj4HQEB4SOCcaFggnDjqIOo4nCggGhWPD4jT0g9oDYA2IPSETkBTnK" +
          "m53Tz5HlAAAAAElFTkSuQmCC",
        DEFAULT_RIGHT_NAVIGATION_IMAGE:
         "data:image/png;base64," +
         "iVBORw0KGgoAAAANSUhEUgAAABAAAAAgCAYAAAAbifjMAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAWJAA" +
         "AFiQBmxXGFAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAJVJREFUSEvFz0sOgCAQBF" +
         "EOxv2vpbYJBEo+w8xCk9ro9EtMOefreZK3F4ggFfAiHeBBPsApMgQUD2dNAcXjUUtAccC2gOKozQQoDktmQ" +
         "HGsjgAVBlQYUGFAhQH1LxD6BY3dQBm7gHZ8DHCszACHJRPAUdsW4IAtAR6PmgI8nDUEeLTqA/BgVwfwo6UK" +
         "8IO1F+BLe1e6AdOmm5054q0eAAAAAElFTkSuQmCC",
        
        // json data parameter names
        PARAM_THUMBS_POSITION: "thumbsPosition",
        PARAM_DEF_IMG_SRC: "defaultImageSrc",
        PARAM_DEF_THUMB_SRC: "defaultThumbSrc",
        PARAM_THUMBS: "thumbs",
        PARAM_BIG_IMG_SRC: "bigSrc",
        PARAM_THUMB_IMG_SRC: "thumbSrc",
        PARAM_HEIGHT: "h",
        PARAM_WIDTH: "w",
        PARAM_LEFT: "l",
        PARAM_TOP: "t",  
        PARAM_THUMB_IMAGE_HEIGHT: "thumbImageHeight",
        PARAM_THUMB_IMAGE_WIDTH: "thumbImageWidth",
        PARAM_IMAGES_LIST: "images",
        PARAM_THUMBS_SCROLL_DELTA: "thumbsScrollDelta",
        //} ImageBox            

        //ObjectInspector {
        CLS_OBJECTINSPECTOR_EDITOR: 'wvObjectInspectorEditor',
        // { ObjectInspector

        //PropSetEditor
        EVT_PS_EDITOR_UPDATED: 'editor-updated',

        //classes
        CLS_PS_DIV_LANGS_CONTAINER: 'wvPSLangsContainer',
        CLS_PS_BTN_ADD: 'wvPSBtnAdd',
        CLS_PS_BTN_DEL: 'wvPSBtnDel',
        CLS_PS_BTN_RAW: 'wvPSBtnRaw',
        CLS_PS_BTN_CLEAR: 'wvPSBtnClear',
        CLS_PS_DIV_LANG_NAME: 'wvPSLangLabel',
        CLS_PS_LANG_SELECTOR: 'wvPSLangSelector',
        CLS_PS_TEXTAREA_RAW_EDITOR: 'wvPSTextareaRawEditor',
        CLS_PS_LABEL_CONTAINER: 'wvPSLabelContainer',
        CLS_PS_INPUT_CONTAINER: 'wvPSInputContainer',


        //Tabs control
        CLS_TABS_ACTIVE_TAB: "wvTabsActive",
        CLS_TABS_UL: "wvTabsUl",
        CLS_TABS_CONTENT_DIV: "wvTabsContentDiv",
        CLS_TABS_CONTENT_CONTAINER: "wvTabsContentContainer",
        CLS_TABS_UL_CONTAINER: "wvTabsUlContainer",
        CLS_TABS_LI_DISABLED: "wvTabDisabled",

        EVT_TABS_TAB_CHANGED: "wv-active-tab-changed",
        EVT_TABS_TAB_ADD: "wv-tab-add",
        EVT_TABS_TAB_REMOVE: "wv-tab-remove",

        //Multiline
        EVT_MULTI_LINE_TEXT_UPDATED: "wv-multiline-updated",

        //ObjectEditor
        EVT_OBJECT_EDITOR_UPDATED: "wv-objectEditor-updated",
        EVT_OBJECT_EDITOR_VALIDATION_ERROR: "wv-objectEditor-validation-error",
        EVT_OBJECT_EDITOR_VALIDATION_SUCCESS: "wv-objectEditor-validation-success",
        EVT_OBEJCT_EDITOR_ARRAY_ADD_ITEM: "wv-objEditor-array-add-item",
        EVT_OBEJCT_EDITOR_ARRAY_REMOVE_ITEM: "wv-objEditor-array-remove-item",
    };

    var CURTAIN_ZORDER  = 500000;
    var TOAST_ZORDER   = 1000000;

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


    var fCurtains = [];
    var fCurtainDIV = null;

    document.addEventListener("focus", function(e){
      if (fCurtains.length > 0){
        var topCurtain = fCurtains[fCurtains.length -1];
        var t = e.target;
        if (typeof(t) !== tUNDEFINED && t !== null){
           if(!WAVE.isParentOf(topCurtain, t)) {
            topCurtain.tabIndex = 0;
            topCurtain.focus();
          }
        }
      }
    }, true);

    published.curtainOn = function(){
        try{ document.activeElement.blur(); } catch(e){}

        var div = document.createElement("div");
        div.style.backgroundColor = "rgba(127,127,127,0.45)";
        div.className = published.CLS_CURTAIN;
        div.style.position = "fixed";
        div.style.left = "0";
        div.style.top = "0";
        div.style.width = "100%";
        div.style.height = "100%";
        div.style.zIndex = CURTAIN_ZORDER + fCurtains.length;
        document.body.appendChild(div);
        fCurtainDIV = div;

        fCurtains.push(div);
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
    };

    published.isCurtain = function(){ return fCurtains.length>0;};

    //Returns currently active dialog or null
    published.currentDialog = function(){
        for(var i=fCurtains.length-1; i>=0; i--)
         if (typeof(fCurtains[i].__DIALOG)!==tUNDEFINED) return fCurtains[i].__DIALOG;
        return null;
    };

    //Dialog window:   
    //   {title: 'Title', content: 'html markup', cls: 'className', widthpct: 75, heightpct: 50, onShow: function(dlg){}, onClose: function(dlg, result){return true;}}
    published.Dialog = function(init)
    {
        if (!WAVE.isObject(init)) init = {};

        var fOnShow = WAVE.isFunction(init.onShow) ? init.onShow : function(){};
        var fOnClose = WAVE.isFunction(init.onClose) ? init.onClose : function(){ return true;};
        
        var dialog = this;
        WAVE.extend(dialog, WAVE.EventManager);

        published.curtainOn();

        var fdivBase = document.createElement("div");
        fdivBase.__DIALOG = this;
        fdivBase.className = published.CLS_DIALOG_BASE + ' ' + WAVE.strDefault(init['cls']);
        fdivBase.style.position = "fixed";
        fdivBase.style.zIndex = CURTAIN_ZORDER + fCurtains.length;

        
        var fWidthPct = WAVE.intValidPositive(init.widthpct) ? init.widthpct : 0;
        var fHeightPct = WAVE.intValidPositive(init.heightpct) ? init.heightpct : 0;

        if (fWidthPct>100) fWidthPct = 100;
        if (fHeightPct>100) fHeightPct = 100;


        document.body.appendChild(fdivBase);
        
        fCurtains.push(fdivBase);

        var fdivTitle = document.createElement("div");
        fdivTitle.className = published.CLS_DIALOG_TITLE;
        fdivTitle.innerHTML = WAVE.strDefault(init['title'], 'Dialog');
        fdivBase.appendChild(fdivTitle);

        var fdivContent = document.createElement("div");
        fdivContent.className = published.CLS_DIALOG_CONTENT;
        fdivContent.innerHTML = WAVE.strDefault(init['content'], '&nbsp;');
        fdivBase.appendChild(fdivContent);

        function adjustBounds(){
          var sw = $(window).width();
          var sh = $(window).height();
          var cx = sw / 2;
          var cy = sh / 2;

          var w = fWidthPct===0 ? fdivBase.offsetWidth : Math.round( sw * (fWidthPct/100)); 
          var h = fHeightPct===0 ? fdivBase.offsetHeight : Math.round( sh * (fHeightPct/100)); 
          
          fdivBase.style.left = Math.round(cx - (w / 2)) + "px";
          fdivBase.style.top = Math.round(cy - (h / 2)) + "px";
          fdivBase.style.width  = fWidthPct===0  ? "auto" : w + "px";
          fdivBase.style.height = fHeightPct===0 ? "auto" : h + "px";

        //  fdivContent.style.width  = fWidthPct===0  ? "auto" : w - (fdivContent.offsetLeft*2) + "px";
          fdivContent.style.height  = fWidthPct===0  ? "auto" :
                                                      h - (fdivContent.offsetTop + fdivContent.offsetLeft) + "px";//todo This may need to be put as published.OFFSETY that depends on style
        }
        

        var fResult = published.DLG_UNDEFINED;

        //returns dialog result or DLG_UNDEFINED
        this.result = function(){ return fResult; };

        //closes dialog with the specified result and returns the result
        this.close = function(result){
            if (typeof(result)===tUNDEFINED) result = published.DLG_CANCEL;
            if (fResult!==published.DLG_UNDEFINED) return fResult;

            if (!fOnClose(this, result)) return published.DLG_UNDEFINED;//aka CloseQuery

            fResult = result;
            
            document.body.removeChild(fdivBase);
            WAVE.arrayDelete(fCurtains, fdivBase);
            published.curtainOff();
            this.eventInvoke(published.EVT_DIALOG_CLOSE, result);
            return result;
        };

        //closes dialog with OK result
        this.ok = function(){ this.close(published.DLG_OK); };
        
        //closes dialog with CANCEL result
        this.cancel = function(){ this.close(published.DLG_CANCEL); };

        //get/set title
        this.title = function(val){
          if (typeof(val)===tUNDEFINED || val===null) return fdivTitle.innerHTML;
          fdivTitle.innerHTML = val;
          adjustBounds();
        };

        //get/set content
        this.content = function(val){
          if (typeof(val)===tUNDEFINED || val===null) return fdivContent.innerHTML;
          fdivContent.innerHTML = val;
          adjustBounds();
        };

        //gets/sets width in screen size pct 0..100, where 0 = auto
        this.widthpct = function(val){
          if (typeof(val)===tUNDEFINED || val===fWidthPct) return fWidthPct;
          fWidthPct = val;
          adjustBounds();
        };

        //gets/sets height in screen size pct 0..100, where 0 = auto
        this.widthpct = function(val){
          if (typeof(val)===tUNDEFINED || val===fHeightPct) return fHeightPct;
          fHeightPct = val;
          adjustBounds();
        };

        adjustBounds();
        
        var tmr = null;
        $(window).resize(function(){
           if (tmr) clearTimeout(tmr);//prevent unnecessary adjustments when too many resizes happen
           tmr = setTimeout(function(){  adjustBounds(); tmr = null; }, 500);
        });
        
        fOnShow(this);
    };//dialog
    
    //Displays a simple confirmation propmt dialog
    published.showConfirmationDialog = function (title, content, buttons, callback, options) {
      if (!WAVE.isObject(options)) options = {};
      if (!WAVE.isArray(buttons)) buttons = [];

      content = WAVE.strDefault(content, 'Please confirm');
      var btnCls = WAVE.strDefault(options['btnCls']);
      var footerCls = WAVE.strDefault(options['footerCls']);

      function createButtonHtml(lbl, rslt) {
          var btnTemplate = '<button class="@btnClass@" onclick="WAVE.GUI.currentDialog().close(\'@result@\');">@label@</button>';
          return WAVE.strHTMLTemplate(
                                     btnTemplate, 
                                     { 
                                       btnClass: published.CLS_DIALOG_CONFIRM_BUTTON + ' ' + btnCls, 
                                       result: rslt,
                                       label: lbl
                                     });
      }

      var btnYes = '';
      var btnNo = '';
      var btnOk = '';
      var btnCancel = '';
                       
      if (WAVE.inArray(buttons, WAVE.GUI.DLG_YES))
         btnYes = createButtonHtml('Yes', WAVE.GUI.DLG_YES);
      if (WAVE.inArray(buttons, WAVE.GUI.DLG_NO))
         btnNo = createButtonHtml('No', WAVE.GUI.DLG_NO);
      if (WAVE.inArray(buttons, WAVE.GUI.DLG_OK))
         btnOk = createButtonHtml('OK', WAVE.GUI.DLG_OK);
      if (WAVE.inArray(buttons, WAVE.GUI.DLG_CANCEL))
         btnCancel = createButtonHtml('Cancel', WAVE.GUI.DLG_CANCEL);   
      
      var fullContent = 
        '<div>'+
          content+
          '<div class="'+published.CLS_DIALOG_CONFIRM_FOOTER+' '+footerCls+'" style="text-align:center; padding: 5px 0 0 0">'+
            btnYes + btnNo + btnOk + btnCancel +
          '</div>'+
        '<div>';

      var dialog = new WAVE.GUI.Dialog(
        {
          title: WAVE.strDefault(title, 'Confirmation'), 
          content: fullContent,
          cls: WAVE.strDefault(options['cls']), 
          onShow: function(dlg){}, 
          onClose: callback
        });
    };
    
    var fDirty = false;

    //gets/sets dirty flag
    published.dirty = function(val){
      if (typeof(val)===tUNDEFINED) return fDirty;
      fDirty = val;
    };

    //Returns true if dirty flag is set or dialog shown
    published.isDirty = function(){
      return fDirty || published.currentDialog()!==null;
    };
    
    //Set to true to bypass check on page unload
    published.SUPPRESS_UNLOAD_CHECK = false;

    window.onbeforeunload = function(e){
      if (!published.SUPPRESS_UNLOAD_CHECK && (published.isDirty() || WAVE.RecordModel.isDirty()))
        (e || window.event).returnValue = "The page is in the middle of the data entry. If you navigate away/close the page now some changes will be lost. Are you sure?";
    };

    var PUZZLE_DFLT_HELP = "Please enter the following security information by touching the symbols below";

    //Puzzle keypad class: new PuzzleKeypad({DIV: divPuzzle, Image: '/security/captcha?for=login', Question: 'Your first name spelled backwards'});
    published.PuzzleKeypad = function(init)
    {
        if (typeof(init)===tUNDEFINED || init===null || typeof(init.DIV)===tUNDEFINED || WAVE.strEmpty(init.Image)) throw "PuzzleKeypad.ctor(init.DIV, init.Image)";
        
        var keypad = this;
        WAVE.extend(keypad, WAVE.EventManager);

        var rndKey = WAVE.genRndKey(4);
        var fDIV = init.DIV;
        var fHelp = WAVE.strEmpty(init.Help) ? PUZZLE_DFLT_HELP : init.Help;
        var fQuestion = WAVE.strEmpty(init.Question) ? "" : init.Question;
        var fValue = [];
        var fImage = init.Image;

        var fdivHelp = null;
        var fdivQuestion = null;
        var ftbValue = null;
        var fbtnClear = null;
        var fimgKeys = null;

        function rebuild(){
            var seed = "pzl_"+rndKey+"_"+WAVE.genAutoincKey("_puzzle#-?Keypad@Elements");

            var args = {
              hid: "divHelp_"+seed,
              qid: "divQuestion_"+seed,
              tid: "tbValue_"+seed,
              bid: "btnClear_"+seed,
              iid: "img_"+seed,
              help: fHelp,
              question: fQuestion, 
              img: fImage + "&req="+WAVE.genRndKey(),
              clear: "Clear"
            };
  
            var html = WAVE.strTemplate(
                        "<div class='wvPuzzleHelp'     id='@hid@'>@help@</div>"+
                        "<div class='wvPuzzleQuestion' id='@qid@'>@question@</div>"+
                        "<div class='wvPuzzleInputs'> <input id='@tid@' placeholder='···············' type='text' disabled /><button id='@bid@'>@clear@</button> </div>"+
                        "<div class='wvPuzzleImg'> <img id='@iid@' src='@img@'/></div>", args);

            fDIV.innerHTML = html;

            $("#"+args.bid).click(function(evt){//CLEAR
               evt.preventDefault();
               ftbValue.value = "";
               fValue = [];
               keypad.eventInvoke(published.EVT_PUZZLE_KEYPAD_CHANGE, fValue);
             });
             
            $("#"+args.iid).click(function(e){//IMAGE click
               var offset = $(this).offset();
               var point = {x: Math.round(e.pageX-offset.left), y: Math.round(e.pageY-offset.top)};
               fValue.push(point);
               ftbValue.value += "*";
               keypad.eventInvoke(published.EVT_PUZZLE_KEYPAD_CHANGE, fValue);
             });


            fdivHelp = WAVE.id(args.hid);
            fdivQuestion = WAVE.id(args.qid);
            ftbValue  = WAVE.id(args.tid);
            fbtnClear = WAVE.id(args.bid);
            fimgKeys  = WAVE.id(args.iid);
        }

        //Returns value as an array of points where user clicked
        this.value = function(){ return fValue;};

        //Returns value as a JSON array of points where user clicked
        this.jsonValue = function(){ return JSON.stringify(fValue);};

        this.help = function(val){
           if (typeof(val)===tUNDEFINED) return fHelp;
           if (WAVE.strEmpty(val)) fHelp = PUZZLE_DFLT_HELP;
           else fHelp = val;
           fdivHelp.innerHTML = fHelp;
        };

        this.question = function(val){
           if (typeof(val)===tUNDEFINED) return fQuestion;
           if (WAVE.strEmpty(val)) fQuestion = "";
           else fQuestion = val;
           fdivQuestion.innerHTML = fQuestion;
        };

        rebuild();
    };//keypad

    // Manages all ruler logic
    var RulerManager = function (cfg) {
      var self = this;

      WAVE.extend(this, WAVE.EventManager);

      var DEFAULT_SCOPE_NAME = "";

      var fScopeInfos = [], fScopeInfosWlk = WAVE.arrayWalkable(fScopeInfos); // [{name: , cfg: [{}], elementInfos: []}] 
      var fElementInfos = [], // [{element: , ruler: , cfg: {getInfo: , scopeMouseEnter: , scopeMouseLeave: , scopeMouseMove: }}, scopeInfos: []]
          fElementInfosWlk = WAVE.arrayWalkable(fElementInfos);

      var fRemovedFromConnected = false;

      function getElementInfo(element) {
       return fElementInfosWlk.wFirst(function(e) { return e.element === element; });
      }

      function getScopeInfo(scopeName) {
        return fScopeInfosWlk.wFirst(function(s) { return s.name === scopeName; });
      }

      function ensureScopeInfo(scopeName, cfg) {
        var scopeInfo = getScopeInfo(scopeName);
        if (scopeInfo === null) {
          scopeInfo = {name: scopeName, cfg: cfg || {}, elementInfos: []};
          fScopeInfos.push(scopeInfo);
        } else {
          if (cfg) scopeInfo.cfg = cfg;
        }
        return scopeInfo;
      }

      function ensureElementInfo(element, cfg) {
        var elementInfo = getElementInfo(element);
        if (elementInfo === null) {
          elementInfo = {element: element, cfg: cfg || {}, scopeInfos: []};

          var jElement = $(element);
          jElement.bind("mouseenter", onMouseEnter);
          jElement.bind("mouseleave", onMouseLeave);
          jElement.bind("mousemove", onMouseMove);

          var ruler = new ElementRuler(element, cfg);
          elementInfo.ruler = ruler;

          fElementInfos.push(elementInfo);
        } else {
          if (cfg) {
            elementInfo.cfg = cfg;
            elementInfo.ruler.cfg(cfg);
          }
        }
        return elementInfo;
      }

      function ensureElementInScope(element, elementCfg, scopeName, scopeCfg, cfg) {
        var scopeInfo = ensureScopeInfo(scopeName, scopeCfg);
        var elementInfo = ensureElementInfo(element, elementCfg);
        if (scopeInfo.elementInfos.indexOf(elementInfo) === -1) scopeInfo.elementInfos.push(elementInfo);
        if (elementInfo.scopeInfos.indexOf(scopeInfo) === -1) elementInfo.scopeInfos.push(scopeInfo);
      }

      this.set = function(e) { // {element: , elementCfg: , scopeName: , scopeCfg: , cfg: {}}
        var element = e.element;
        var elementCfg = e.elementCfg; // {getTxt: , getMasterInfo: , getSlaveInfo: }
        var scopeName = e.scopeName || DEFAULT_SCOPE_NAME;
        var scopeCfg = e.scopeCfg;
        var cfg = e.cfg;

        ensureElementInScope(element, elementCfg, scopeName, scopeCfg, cfg);
      };

      this.unset = function (e) {
        var element = e.element;
        var scopeName = e.scopeName;

        var elementInfo = getElementInfo(e.element);
        if (elementInfo === null) return;
        if(scopeName) {
          var scopeInfo = getScopeInfo(scopeName);
          if (scopeInfo !== null) WAVE.arrayDelete(scopeInfo.elements, elementInfo);
          WAVE.arrayDelete(elementInfo.scopes, scopeInfo);
        } else {
          WAVE.arrayWalkable(elementInfo.scopeInfos).wEach(function(s) { WAVE.arrayDelete(s.elementInfos, elementInfo); });
          elementInfo.scopeInfos.splice(0, elementInfo.scopeInfos.length);
        }
        
        if (elementInfo.scopeInfos.length === 0) {
          var jElement = $(element);
          jElement.unbind("mouseenter", onMouseEnter);
          jElement.unbind("mouseleave", onMouseLeave);
          jElement.unbind("mousemove", onMouseMove);
          WAVE.arrayDelete(fElementInfos, elementInfo);
        }
      };

      this.setScope = function (scopeName, cfg) {
        ensureScopeInfo(scopeName, cfg);
      };

      this.mouseMove = function(masterRes, scopeNames) {
        var scopeInfos = fScopeInfosWlk.wWhere(function(si) { return scopeNames.indexOf(si.name) !== -1;}).wToArray();
        moveSlaves(null, scopeInfos, masterRes);
      };

      this.mouseEnter = function (scopeNames) {
        var scopeInfos = fScopeInfosWlk.wWhere(function (si) { return scopeNames.indexOf(si.name) !== -1; }).wToArray();
        enterSlaves(null, scopeInfos);
      };

      this.mouseLeave = function (scopeNames) {
        var scopeInfos = fScopeInfosWlk.wWhere(function (si) { return scopeNames.indexOf(si.name) !== -1; }).wToArray();
        leaveSlaves(null, scopeInfos);
      };

      function onMouseMove(e) {
        var el = e.currentTarget;
        var elementInfo = getElementInfo(el);
        if (elementInfo === null) return;

        var masterRes = elementInfo.ruler.onMouseMove(e);

        // {scope: scopeInfo, masterRes: masterRes, event: e}

        moveSlaves(el, elementInfo.scopeInfos, masterRes);

        var scopeNames = WAVE.arrayWalkable(elementInfo.scopeInfos).wSelect(function(si) { return si.name; }).wToArray();
        self.eventInvoke(published.EVT_RULER_MOUSE_MOVE, masterRes, scopeNames);
      }

      function onMouseEnter(e) {
        var el = e.currentTarget;
        var elementInfo = getElementInfo(el);
        if (elementInfo === null) return;

        elementInfo.ruler.onMouseEnter();

        enterSlaves(el, elementInfo.scopeInfos);

        var scopeNames = WAVE.arrayWalkable(elementInfo.scopeInfos).wSelect(function (si) { return si.name; }).wToArray();
        self.eventInvoke(published.EVT_RULER_MOUSE_ENTER, scopeNames);
      }

      function onMouseLeave(e) {
        var el = e.currentTarget;
        var elementInfo = getElementInfo(el);
        if (elementInfo === null) return;

        elementInfo.ruler.onMouseLeave(e);

        leaveSlaves(el, elementInfo.scopeInfos);

        var scopeNames = WAVE.arrayWalkable(elementInfo.scopeInfos).wSelect(function (si) { return si.name; }).wToArray();
        self.eventInvoke(published.EVT_RULER_MOUSE_LEAVE, scopeNames);
      }

      function moveSlaves(element, scopeInfos, masterRes) {
        var slaveEvt = { scope: null, masterRes: masterRes};

        for (var iSi in scopeInfos) {
          var si = scopeInfos[iSi];
          slaveEvt.scope = si;
          for (var iEi in si.elementInfos) {
            var ei = si.elementInfos[iEi];
            if (element === ei.element) continue;
            ei.ruler.onScopeMouseMove(slaveEvt);
          }
        }
      }

      function enterSlaves(element, scopeInfos) {
        for (var iSi in scopeInfos) {
          var si = scopeInfos[iSi];
          for (var iEi in si.elementInfos) {
            var ei = si.elementInfos[iEi];
            if (element === ei.element) continue;
            ei.ruler.onScopeMouseEnter(si);
          }
        }
      }

      function leaveSlaves(element, scopeInfos) {
        for (var iSi in scopeInfos) {
          var si = scopeInfos[iSi];
          for (var iEi in si.elementInfos) {
            var ei = si.elementInfos[iEi];
            if (element === ei.element) continue;
            ei.ruler.onScopeMouseLeave(si);
          }
        }
      }

      //function fireMouseMove(masterRes, scopeNames) { self.eventInvoke(published.EVT_RULER_MOUSE_MOVE, masterRes, scopeNames); }

    };//RulerManager

    var ElementRuler = function(element, cfg) {
      var self = this;

      var fElement = element;
      var fCfg = cfg || {};
      this.cfg = function (val) {
        if (typeof(val) !== tUNDEFINED && val !== cfg) {
          fCfg = val;
        }
        return fCfg;
      };

      var fRulerHintCls = fCfg.hintCls || "wvRulerHint";
      var fRulerSightCls = fCfg.sightCls || "wvRulerSight";

      var fSigntSize = fCfg.sightSize || 8;

      var fMasterElementsCreated = false, fSlaveElementsCreated = false;

      var divHint = null, divSightLeft = null, divSightTop = null, divSightRight = null, divSightBottom = null, divSightCenter = null,
          divSightBoxLeft = null, divSightBoxTop = null, divSightBoxRight = null, divSightBoxBottom = null, divSightBoxCenter = null,
          divSlave = null;

      this.onMouseEnter = function(e) {
        ensureMasterElements();
      };

      this.onMouseLeave = function(e) {
        ensureNoMasterElements();
      };

      this.onMouseMove = function (e) {
        ensureNoSlaveElements();
        ensureMasterElements();

        var parentRc;
        if (fCfg.parentRc) {
          parentRc = fCfg.parentRc;
        } else {
          var parentRect = fElement.getBoundingClientRect();
          parentRc = WAVE.Geometry.toRectWH(parentRect.left, parentRect.top, parentRect.width, parentRect.height);
        }

        var elX = e.clientX - parentRc.left();
        var elY = e.clientY - parentRc.top();

        divHint.style.left = e.clientX + "px";
        divHint.style.top = e.clientY + "px";

        var txt = null;

        if (fCfg.getTxt) {
          txt = fCfg.getTxt({
            clientPoint: new WAVE.Geometry.Point(e.clientX, e.clientY),
            divHint: divHint
          });
        } else {
          txt = elX + ", " + elY;
        }

        divHint.innerHTML = txt;

        var divHintRect = divHint.getBoundingClientRect();

        var hintRc = WAVE.Geometry.toRectWH(divHintRect.left, divHintRect.top, divHintRect.width, divHintRect.height);
        
        var hintPos = getHintPos(e.clientX, e.clientY, hintRc, parentRc);

        divHint.style.left = hintPos.x() + "px";
        divHint.style.top = hintPos.y() + "px";

        locateSight(e.clientX, e.clientY, parentRc);

        var r = fCfg.getMasterInfo ?
                  fCfg.getMasterInfo({clientPoint: new WAVE.Geometry.Point(e.clientX, e.clientY)}) :
                  { elementRcPoint: new WAVE.Geometry.Point(elX, elY), isInParentRc: true }; 

        ensureDivsVisibility(e.clientX, e.clientY, r.isInParentRc);

        return r;
      };

          function locateSight(clientX, clientY, parentRc) {
            var left = clientX - fSigntSize;
            if (left < parentRc.left()) left = parentRc.left();

            var right = clientX + fSigntSize;
            if (right > parentRc.right()) right = parentRc.right();

            var top = clientY - fSigntSize;
            if (top < parentRc.top()) top = parentRc.top();

            var bottom = clientY + fSigntSize;
            if (bottom > parentRc.bottom()) bottom = parentRc.bottom();

            divSightLeft.style.left = parentRc.left() + "px";
            divSightLeft.style.width = (left - parentRc.left()) + "px";
            divSightLeft.style.top = clientY + "px";

            divSightRight.style.left = right + "px";
            divSightRight.style.width = (parentRc.right() - right) + "px";
            divSightRight.style.top = clientY + "px";

            divSightTop.style.top = parentRc.top() + "px";
            divSightTop.style.height = (top - parentRc.top()) + "px";
            divSightTop.style.left = clientX + "px";

            divSightBottom.style.top = bottom + "px";
            divSightBottom.style.height = (parentRc.bottom() - bottom) + "px";
            divSightBottom.style.left = clientX + "px";

            divSightBoxLeft.style.top = top + "px";
            divSightBoxLeft.style.height = (bottom - top) + "px";
            divSightBoxLeft.style.left = left + "px";

            divSightBoxRight.style.top = top + "px";
            divSightBoxRight.style.height = (bottom - top) + "px";
            divSightBoxRight.style.left = right + "px";

            divSightBoxTop.style.left = left + "px";
            divSightBoxTop.style.width = (right - left) + "px";
            divSightBoxTop.style.top = top + "px";

            divSightBoxBottom.style.left = left + "px";
            divSightBoxBottom.style.width = (right - left) + "px";
            divSightBoxBottom.style.top = bottom + "px";
          }

          function ensureDivsVisibility(clientX, clientY, isInParentRc) {
            var allDivsWlk = getAllDivsWlk();
            if (!isInParentRc)
            {
              allDivsWlk.wEach(function (d) {
                if (d !== null && d.style.visibility !== "hidden")
                  d.style.visibility = "hidden";
              });
            } else {
              allDivsWlk.wEach(function (d) {
                if (d !== null && d.style.visibility === "hidden")
                  d.style.visibility = "visible";
              });
            }
          }

          function getAllDivsWlk() {
            return new WAVE.arrayWalkable([
                divHint, divSightLeft, divSightTop, divSightRight, divSightBottom, divSightCenter,
                divSightBoxLeft, divSightBoxTop, divSightBoxRight, divSightBoxBottom, divSightBoxCenter,
                divSlave]);
          }

      this.onScopeMouseEnter = function (scope) {
        ensureSlaveElements();
      };

      this.onScopeMouseLeave = function (scope) {
        ensureNoSlaveElements();
      };

      this.onScopeMouseMove = function (e) { // {scope: scopeInfo, masterRes: masterRes, event: e}
        ensureNoMasterElements();
        ensureSlaveElements();

        var slaveParentRc = self.getParentRc();

        var clientX;
        var clientY;

        var elementRcPoint;
        var slaveIsInParentRc;
        if (fCfg.getSlaveInfo) {
          var slaveInfo = fCfg.getSlaveInfo({ masterRes: e.masterRes });
          //elementRcPoint = slaveInfo.elementRcPoint;
          slaveIsInParentRc = slaveInfo.isInParentRc;
          clientX = slaveInfo.clientPoint.x();
          clientY = slaveInfo.clientPoint.y();
        } else {
          elementRcPoint = e.masterRes.elementRcPoint;
          slaveIsInParentRc = true;
          clientX = slaveParentRc.left() + elementRcPoint.x();
          clientY = slaveParentRc.top() + elementRcPoint.y();
        }

        

        ensureDivsVisibility(clientX, clientY, e.masterRes.isInParentRc && slaveIsInParentRc);

        divSlave.style.top = slaveParentRc.top() + "px";
        divSlave.style.height = slaveParentRc.height() + "px";
        divSlave.style.left = clientX + "px";
      };

      this.getParentRc = function () {
        var parentRc;

        if (fCfg.parentRc) {
          parentRc = fCfg.parentRc;
        } else {
          var parentRect = fElement.getBoundingClientRect();
          parentRc = WAVE.Geometry.toRectWH(parentRect.left, parentRect.top, parentRect.width, parentRect.height);
        }

        return parentRc;
      };

      function ensureMasterElements() {
        if (fMasterElementsCreated) return;

        divHint = document.createElement("div");
        divHint.id = "WAVE_GUI_Ruler";
        divHint.innerHTML = "Hint DIV";
        divHint.className = fRulerHintCls;
        divHint.style.position = "absolute";
        document.body.appendChild(divHint);

        divSightRight = createLineDiv(true);
        divSightLeft = createLineDiv(true);
        divSightBoxRight = createLineDiv(false);
        divSightBoxLeft = createLineDiv(false);

        divSightTop = createLineDiv(false);
        divSightBottom = createLineDiv(false);
        divSightBoxTop = createLineDiv(true);
        divSightBoxBottom = createLineDiv(true);

        fMasterElementsCreated = true;
      }

      function ensureNoMasterElements() {
        if (!fMasterElementsCreated) return;

        removeDiv(divHint);           divHint = null;
        removeDiv(divSightRight);     divSightRight = null;
        removeDiv(divSightLeft);      divSightLeft = null;
        removeDiv(divSightBoxRight);  divSightBoxRight = null;
        removeDiv(divSightBoxLeft);   divSightBoxLeft = null;
        removeDiv(divSightTop);       divSightTop = null;
        removeDiv(divSightBottom);    divSightBottom = null;
        removeDiv(divSightBoxTop);    divSightBoxTop = null;
        removeDiv(divSightBoxBottom); divSightBoxBottom = null;

        fMasterElementsCreated = false;
      }

      function ensureSlaveElements() {
        if (fSlaveElementsCreated) return;
        divSlave = createLineDiv(false);
        fSlaveElementsCreated = true;
      }

      function ensureNoSlaveElements() {
        if (!fSlaveElementsCreated) return;
        removeDiv(divSlave); divSlave = null;
        fSlaveElementsCreated = false;
      }

        function createLineDiv(horizontal) {
          var div = document.createElement("div");
          if (horizontal) div.style.height = "1px"; else div.style.width = "1px";
          div.style.position = "absolute";
          div.className = fRulerSightCls;
          document.body.appendChild(div);
          return div;
        }

        function removeDiv(div) {
          if (div === null) return;
          document.body.removeChild(div); 
        }

        function getHintPos(cx, cy, hintRc, parentElementRc) {
          var resultRc = null;
          var hintSquare = hintRc.square();

          var minPenalty = Number.MAX_VALUE;
          for (var rad = 0; rad < 2 * Math.PI; rad += Math.PI / 4) {
            var rc = WAVE.Geometry.rotateRectAroundCircle(cx, cy, 20, hintRc.width(), hintRc.height(), rad);

            var visibleSquare = WAVE.Geometry.overlapAreaRect(parentElementRc, rc);

            var penalty = hintSquare - visibleSquare;

            if (penalty === 0) {
              resultRc = rc;
              break;
            }

            if (penalty < minPenalty) {
              minPenalty = penalty;
              resultRc = rc;
            }
          }

          return resultRc.topLeft();
        }
    };//ElementRuler

    var fRulerManager = null;

    published.rulerSet = function (e) { // {element: , elementCfg: , scopeName: , scopeCfg: , cfg: {}}
      if (fRulerManager === null) fRulerManager = new RulerManager();
      fRulerManager.set(e);
    };

    published.rulerUnset = function(e) { // {element: , scope: }}
      if (fRulerManager === null) return;
      fRulerManager.unset(e);
    };

    published.rulerSetScope = function (scopeName, cfg) { 
      if (fRulerManager === null) fRulerManager = new RulerManager();
      fRulerManager.setScope(scopeName, cfg);
    };

    published.rulerMouseMove = function(masterRes, scopeNames) {
      if (fRulerManager === null) return;
      fRulerManager.mouseMove(masterRes, scopeNames);
    };

    published.rulerMouseEnter = function (scopeNames) {
      if (fRulerManager === null) return;
      fRulerManager.mouseEnter(scopeNames);
    };

    published.rulerMouseLeave = function (scopeNames) {
      if (fRulerManager === null) return;
      fRulerManager.mouseLeave(scopeNames);
    };

    published.rulerEventBind = function(e, handler) {
      if (fRulerManager === null) fRulerManager = new RulerManager();
      fRulerManager.eventBind(e, handler);
    };

    // Implements inter-window browser commenication
    published.WindowConnector = function (cfg) {
      cfg = cfg || {};

      var self = this;

      var fWnd = cfg.wnd || window;

      var fDomain = fWnd.location.protocol + "//" + fWnd.location.host;

      var fConnectedWindows = [], fConnectedWindowsWlk = WAVE.arrayWalkable(fConnectedWindows);
      this.connectedWindows = function () { return WAVE.arrayShallowCopy(fConnectedWindows); };

      this.openWindow = function (href) {
        var win = window.open(href || window.location.href);
        fConnectedWindowsWlk.wEach(function (w) {
          w.Connector.addWindow(win);
        });
        self.addWindow(win);
      };

      this.closeCurrentWindow = function () {
        fConnectedWindowsWlk.wEach(function (w) { w.Connector.removeWindow(fWnd); });
      };

      this.addWindow = function (e) {
        fConnectedWindows.push(e.window);
      };

      this.removeWindow = function (w) {
        WAVE.arrayDelete(fConnectedWindows, w);
      };

      this.callWindowFunc = function (func) { // func is callback of type function(w) { }
        var closedWindows;
        fConnectedWindowsWlk.wEach(function (cw) {
          if (cw.closed) {
            if (!closedWindows) closedWindows = [];
            closedWindows.push(cw);
          } else {
            func(cw);
          }
        });

        if (closedWindows) {
          var closedWindowsWlk = WAVE.arrayWalkable(closedWindows);
          closedWindowsWlk.wEach(function (rw) {
            self.removeWindow(rw);
          });
          closedWindowsWlk.wEach(function (rw) {
            fConnectedWindowsWlk.wEach(function (w) {
              w.Connector.removeWindow(rw);
            });
          });
        }
      };

      this.logMsg = function (msg) {
        console.log(msg);
      };

      if (fWnd.opener && fWnd.opener.Connector) {
        self.addWindow(fWnd.opener);
        var openerConnectedWindows = fWnd.opener.Connector.connectedWindows();
        for (var i in openerConnectedWindows) {
          var cw = openerConnectedWindows[i];
          if (cw === fWnd) continue;
          if (cw.closed) continue;
          self.addWindow({ window: cw });
        }
      }
    };//WindowConnector

    // Ensures that wnd (window by default) has Connector property of type WindowConnector.
    // Call this prior to first call to window.Connector
    published.connectorEnsure = function (wnd) {
      wnd = wnd || window;
      if (!wnd.Connector) wnd.Connector = new published.WindowConnector({ wnd: wnd });
    };

    var fTreeNodeIDSeed = 0;

    var TREE_NODE_TEMPLATE =  "<div id='exp_@id@' class='@wvTreeNodeButton@'></div>" +
                              "<div id='content_@id@' class='@wvTreeNodeContent@'>" +
                              "  <div id='own_@id@' class='@wvTreeNodeOwn@'></div>" +
                              "  <div id='children_@id@' class='@wvTreeNodeChildren@'>" +
                              "  </div>" +
                              "</div>";

    published.Tree = function(init) {

                var Node = function(nodeInit) {
                  nodeInit = nodeInit || {};

                  var node = this;

                  fTreeNodeIDSeed++;
                  var fElmID = "tvn_" + fTreeNodeIDSeed;

                  var fParent = nodeInit.parent || null;
                  if (fParent !== null && fParent.tree() !== tree) throw "Tree.Node.ctor(wrong parent tree)";
                  this.parent = function() { return fParent; };

                  var fDIV;
                  var fExpander;
                  var fDIVContent;
                  var fDIVOwn;
                  var fDIVChildren;
                  this.__divChildren = function() { return fParent !== null ? fDIVChildren : fTreeDIV; };

                  var fID = typeof(nodeInit.id) !== tUNDEFINED ? nodeInit.id.toString() : fTreeNodeIDSeed.toString();
                  this.id = function() { return fID; };

                          function updateExpansionContent() {
                            if (!fExpander) return;
                            fExpander.innerHTML = fExpanded ? fExpandedContent : fCollapsedContent;
                          }

                  var fExpandedContent = typeof(nodeInit.expandedContent) === tUNDEFINED ? tree.DEFAULT_NODE_EXPANDED_CONTENT : nodeInit.expandedContent;
                  this.expandedContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpandedContent;
                    fExpandedContent = val;
                    updateExpansionContent();
                  };

                  var fCollapsedContent = typeof(nodeInit.collapsedContent) === tUNDEFINED ? tree.DEFAULT_NODE_COLLAPSED_CONTENT : nodeInit.collapsedContent;
                  this.collapsedContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fCollapsedContent;
                    fCollapsedContent = val;
                    updateExpansionContent();
                  };

                  this.path = function() {
                    if (fParent === null) return "";
                    if (fParent === fRootNode) return fID;
                    return fParent.path() + "/" + fID;
                  };

                  // returns integer nesting level from root
                  this.level = function() {
                    if (fParent === null) return -1;
                    if (fParent === fRootNode) return 0;
                    return fParent.level() + 1;
                  };

                  this.html = function(val) { 
                    if (fParent === null) return null;
                    if (typeof(val) === tUNDEFINED) return fDIVOwn.innerHTML;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, oldContent: fDIVOwn.innerHTML, newContent: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_CONTENT, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fDIVOwn.innerHTML = val;

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, oldContent: fDIVOwn.innerHTML, newContent: val};
                    treeEventInvoke(published.EVT_TREE_NODE_CONTENT, evtArgsAfter);
                  };

                  var fSelectable = nodeInit.selectable !== false;
                  this.selectable = function(val) {
                    if (typeof(val) === tUNDEFINED) return fSelectable;
                    fSelectable = val;
                    if (fSelected && !val) node.selected(false);
                  };

                  var fSelected = nodeInit.selected === true;
                  this.selected = function(val, supressEvents) {
                    if (fTreeSelectionType === published.TREE_SELECTION_NONE) return false;

                    if (typeof(val) === tUNDEFINED) return fSelected;
                    if (val === fSelected) return;

                    if (fSelectable || (!fSelectable && !val)) {

                      if (supressEvents !== true) {
                        var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, value: val, abort: false};
                        treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsBefore);
                        if (evtArgsBefore.abort === true) return;
                      }
                    
                      fSelected = val;
                      fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();

                      if (supressEvents !== true) onNodeSelectionChanged(node, val);
                    }
                  };

                  this.wvTreeNode = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIV.className;
                    fDIV.className = val;
                  };

                  this.wvTreeNodeButton = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpander.className;
                    fExpander.className = val;
                  };

                  this.wvTreeNodeContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIVContent.className;
                    fDIVContent.className = val;
                  };


                  var fWVTreeNodeButtonExpanded = nodeInit.wvTreeNodeButtonExpanded || fNodeTemplateClsArgs.wvTreeNodeButtonExpanded;
                  this.wvTreeNodeButtonExpanded = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeButtonExpanded || fNodeTemplateClsArgs.wvTreeNodeButtonExpanded);
                    if (val === fWVTreeNodeButtonExpanded) return;
                    fWVTreeNodeButtonExpanded = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                  };

                  var fWVTreeNodeButtonCollapsed = nodeInit.wvTreeNodeButtonCollapsed || fNodeTemplateClsArgs.wvTreeNodeButtonCollapsed;
                  this.wvTreeNodeButtonCollapsed = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeButtonCollapsed || fNodeTemplateClsArgs.wvTreeNodeButtonCollapsed);
                    if (val === fWVTreeNodeButtonCollapsed) return;
                    fWVTreeNodeButtonCollapsed = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                  };


                  var fWVTreeNodeOwn = nodeInit.wvTreeNodeOwn || fNodeTemplateClsArgs.wvTreeNodeOwn;
                  this.wvTreeNodeOwn = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeOwn || fNodeTemplateClsArgs.wvTreeNodeOwn);
                    if (val === fWVTreeNodeOwn) return;
                    fWVTreeNodeOwn = val;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();
                  };

                  var fWVTreeNodeOwnSelected = nodeInit.wvTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected;
                  this.wvTreeNodeOwnSelected = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected);
                    if (val === fWVTreeNodeOwnSelected) return;
                    fWVTreeNodeOwnSelected = val;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();
                  };

                  this.wvTreeNodeChildren = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIVChildren.className;
                    fDIVChildren.className = val;
                  };

                  var fChildrenDisplayVisible = nodeInit.childrenDisplayVisible || fNodeChildrenDisplayVisible;
                  this.childrenDisplayVisible = function(val) {
                    if (typeof(val) === tUNDEFINED) return fChildrenDisplayVisible;
                    if (val === fChildrenDisplayVisible) return;
                    fChildrenDisplayVisible = val;
                    if (fExpanded) fDIVChildren.style.display = fChildrenDisplayVisible;
                  };

                  var fData = nodeInit.data;
                  this.data = function(val) { 
                    if (typeof(val) === tUNDEFINED) return fData;
                    if (fData === val) return;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, oldData: fData, newData: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_DATA, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fData = val;

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, oldData: fData, newData: val};
                    treeEventInvoke(published.EVT_TREE_NODE_DATA, evtArgsAfter);
                  };

                  var fExpanded = nodeInit.expanded === true;
                  this.expanded = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpanded;
                    if (fParent === null) return; // fake root node coudn't be expanded/collapsed
                    if (fExpanded === val) return;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, value: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_EXPANSION, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fExpanded = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                    fExpander.innerHTML = val ? fExpandedContent : fCollapsedContent;
                    fDIVChildren.style.display = val ? fChildrenDisplayVisible : "none";
                      
                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
                    treeEventInvoke(published.EVT_TREE_NODE_EXPANSION, evtArgsAfter);
                  };

                  var fChildren = [];
                  this.children = function() { return WAVE.arrayShallowCopy(fChildren); };
                  this.__children = function() { return fChildren; };

                  this.tree = function() { return tree; };

                  // public {

                  this.getDescendants = function() {
                    return node.__getDescendants(0);
                  };

                  this.__getDescendants = function(level) {
                    var walker = function() {
                      var childrenWalker = WAVE.arrayWalkable(fChildren).getWalker();
                      var childWalker = null;
                      var currentType = 0; // 0 - not initialized, 1 - child, 2 - grandchild
                      var nodeNum = -1;

                      this.reset = function() {
                        childrenWalker = WAVE.arrayWalkable(fChildren).getWalker();
                        currentType = 0;
                        nodeNum = -1;
                      };

                      this.moveNext = function() {
                        if (currentType === 0) {
                          return moveChildrenWalker();
                        } else if (currentType === 1) {
                          childWalker = childrenWalker.current().__getDescendants(level+1).getWalker();
                          if (!childWalker.moveNext()) {
                            return moveChildrenWalker();
                          } else {
                            currentType = 2;
                            return true;
                          }
                        } else if (currentType === 2) {
                          if (!childWalker.moveNext()) {
                            return moveChildrenWalker();
                          } else {
                            return true;
                          }
                        }

                        return true;

                        function moveChildrenWalker() {
                          if (!childrenWalker.moveNext()) { 
                            currentType = 0;
                            return false;
                          } else {
                            nodeNum++;
                            currentType = 1;
                            return true;
                          }
                        }

                      };

                      this.current = function() {
                        if (currentType === 1) return {level: level, nodeNum: nodeNum, node: childrenWalker.current()};
                        if (currentType === 2) return childWalker.current();
                      };
                    };

                    var walkable = {getWalker: function() { return new walker(); }, tree: tree};
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
                  };

                  this.getChildIdx = function (id, recursive) {
                    if (!recursive)
                      return WAVE.arrayWalkable(fChildren).wFirstIdx(function (c) { return WAVE.strSame(c.id(), id); });
                    else
                      return node.getDescendants(0).wFirstIdx(function (d) { return WAVE.strSame(d.node.id(), id); });
                  };

                  this.getChild = function (id, recursive) {
                    if (!recursive)
                      return WAVE.arrayWalkable(fChildren).wFirst(function (c) { return WAVE.strSame(c.id(), id); });
                    else {
                      var descendant = node.getDescendants(0).wFirst(function (d) { return WAVE.strSame(d.node.id(), id); });
                      return descendant ? descendant.node : null;
                    }
                  };

                  this.navigate = function(path) {
                    if (typeof(path)===tUNDEFINED || path===null) return null;

                    var segments;
                    if (WAVE.isArray(path)) {
                      segments = path;
                    } else if (!WAVE.strEmpty(path)) {
                      segments = path.split(/[\\/]+/);
                    } else {
                      return null;
                    }

                    function getNodeIdSegmentEqualsFunc(segment) {
                        var f = function (n) { return n.id() === segment; };
                        return f;
                    }

                    var node = null;
                    var childrenWalkable = WAVE.arrayWalkable(fChildren);
                    for(var i in segments) {
                      var segment = segments[i];
                      node = childrenWalkable.wFirst(getNodeIdSegmentEqualsFunc(segment));
                      if (node===null) return null;
                      childrenWalkable = WAVE.arrayWalkable(node.__children());
                    }
                    return node;
                  };

                  // Expands all parent nodes
                  this.unveil = function() {
                    var parent = fParent;
                    if (parent === null) return;

                    while(parent !== null) {
                      var parentParent = parent.parent();
                      if (parentParent !== null) parent.expanded(true);
                      parent = parentParent;
                    }
                  };

                  this.addChild = function(childNodeInit) {
                    if (typeof(childNodeInit) === tUNDEFINED) childNodeInit = {};

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, parentNode: node, childNodeInit: childNodeInit, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_ADD, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    childNodeInit.parent = node;
                    var childNode = new Node(childNodeInit);
                    fChildren.push(childNode);
                    if (fParent !== null)
                      fExpander.style.visibility = "visible";

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, parentNode: node, childNode: childNode};
                    treeEventInvoke(published.EVT_TREE_NODE_ADD, evtArgsAfter);

                    return childNode;
                  };

                  this.__removeChild = function(childNode) {
                    WAVE.arrayDelete(fChildren, childNode);
                    if (fChildren.length === 0 && fParent !== null)
                      fExpander.style.visibility = "hidden";
                  };

                  this.remove = function() {
                    if (fParent === null) {
                      console.error("Root node couldn't be removed");
                      return;
                    }

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, parentNode: fParent, childNode: node, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_REMOVE, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fDIV.parentNode.removeChild(fDIV);
                    fParent.__removeChild(node);

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, parentNode: fParent, childNode: node};
                    treeEventInvoke(published.EVT_TREE_NODE_REMOVE, evtArgsAfter);
                  };

                  this.removeAllChildren = function() {
                    var children = WAVE.arrayShallowCopy(fChildren);
                    for(var i in children) {
                      var child = children[i];
                      child.remove();
                    }
                  };
                  
                  // public }

                  if (fParent !== null) {
                    var nodeTemplateClsArgs = {
                      id: fElmID,

                      wvTreeNode            : nodeInit.wvTreeNode            || fNodeTemplateClsArgs.wvTreeNode,
                      wvTreeNodeContent     : nodeInit.wvTreeNodeContent     || fNodeTemplateClsArgs.wvTreeNodeContent,
                      wvTreeNodeOwn         : nodeInit.wvTreeNodeOwn         || fNodeTemplateClsArgs.wvTreeNodeOwn,
                      wvTreeNodeOwnSelected : nodeInit.wvTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected,
                      wvTreeNodeChildren    : nodeInit.wvTreeNodeChildren    || fNodeTemplateClsArgs.wvTreeNodeChildren
                    };

                    nodeTemplateClsArgs.wvTreeNodeButton = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();

                    var html = WAVE.strTemplate(TREE_NODE_TEMPLATE, nodeTemplateClsArgs);
                  
                    var parentDIV = fParent.__divChildren();
                    
                    fDIV = document.createElement("div");
                    fDIV.id = "root_" + fElmID;
                    var cls = "";
                    fDIV.className = nodeTemplateClsArgs.wvTreeNode;

                    fDIV.innerHTML = html;

                    parentDIV.appendChild(fDIV);
                    
                    fDIVContent = WAVE.id("content_" + fElmID);
                    fDIVOwn = WAVE.id("own_" + fElmID);
                    fDIVChildren = WAVE.id("children_" + fElmID);

                    fDIVOwn.innerHTML = nodeInit.html;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();

                    $(fDIVOwn).click(function() {
                      node.selected(!fSelected);
                    });

                    var exp = nodeInit.expanded === true;
                    fExpander = WAVE.id("exp_" + fElmID);
                    fExpander.innerHTML = fExpanded ? fExpandedContent : fCollapsedContent;
                    fExpander.style.visibility = "hidden";

                    $(fExpander).click(function() {
                      node.expanded(!fExpanded);
                    });

                    fDIVChildren.style.display = exp ? fChildrenDisplayVisible : "none";
                  }

                }; //Node class 


      if (typeof(init)===tUNDEFINED || init===null || typeof(init.DIV)===tUNDEFINED || init.DIV===null) throw "Tree.ctor(init.DIV)";

      var tree = this;
      WAVE.extend(tree, WAVE.EventManager);

      this.DEFAULT_NODE_EXPANDED_CONTENT = '-';
      this.DEFAULT_NODE_COLLAPSED_CONTENT = '+';

      var fTreeDIV = init.DIV;

      var fNodeTemplateClsArgs = {
        wvTreeNode                : init.wvTreeNode                || published.CLS_TREE_NODE,
        wvTreeNodeButtonExpanded  : init.wvTreeNodeButtonExpanded  || published.CLS_TREE_NODE_BUTTON_EXPANDED,
        wvTreeNodeButtonCollapsed : init.wvTreeNodeButtonCollapsed || published.CLS_TREE_NODE_BUTTON_COLLAPSED,
        wvTreeNodeContent         : init.wvTreeNodeContent         || published.CLS_TREE_NODE_CONTENT,
        wvTreeNodeOwn             : init.wvTreeNodeOwn             || published.CLS_TREE_NODE_OWN,
        wvTreeNodeOwnSelected     : init.wvTreeNodeOwnSelected     || published.CLS_TREE_NODE_OWN_SELECTED,
        wvTreeNodeChildren        : init.wvTreeNodeChildren        || published.CLS_TREE_NODE_CHILDREN
      };

      var fNodeChildrenDisplayVisible = init.childrenDisplayVisible || "block";

      var fTreeSelectionType = init.treeSelectionType || published.TREE_SELECTION_NONE;
      this.treeSelectionType = function(val) {
        if (typeof(fTreeSelectionType) === tUNDEFINED) return fTreeSelectionType;
        fTreeSelectionType = val;
      };

      function onNodeSelectionChanged(node, val) {
        var evtArgsAfter;

        if (fTreeSelectionType === published.TREE_SELECTION_SINGLE) {
          
          if (val === true) {
            fRootNode.getDescendants().wEach(function(n) {
              if (n.node !== node && n.node.selected() === true) {

                var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: n.node, value: n.node.selected()};
                treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsBefore);

                n.node.selected(false, true);

                var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: n.node, value: n.node.selected()};
                treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);
              }
            });
          }

          evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
          treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);

        } else if (fTreeSelectionType === published.TREE_SELECTION_MULTI) {
          evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
          treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);
        }
      }

      this.selectedNodes = function(val) {
        if (typeof(val) === tUNDEFINED)
          return fRootNode.getDescendants().wWhere(function(n) { return n.node.selected() === true;});

        if (val === null) throw "val couldn't be null";

        var selectedWalkable;
        if (WAVE.isFunction(val.wAny)) {
          selectedWalkable = val;
        } else if (WAVE.isArray(val)) {
          selectedWalkable = WAVE.arrayWalkable(val);
        } else {
          throw "val type is invalid";
        }

        fRootNode.getDescendants().wEach(function(n) {
          n.node.selected(selectedWalkable.wAny(function(nodeId){ return nodeId === n.node.id();}), true);
        });
      };

      var fSupressEvents = init.supressEvents === true;
      this.supressEvents = function(val) { 
        if (typeof(val) === tUNDEFINED) return fSupressEvents;
        fSupressEvents = val;
      };

      function treeEventInvoke(evt, args) {
        if (!fSupressEvents) tree.eventInvoke(evt, args);
      }

      var fRootNode = new Node();
      this.root = function() { return fRootNode; };
    };//Tree

    var fObjectInspectorEditorIDSeed = 0, fObjectInspectorEditorIDPrefix = "objinsp_edit_";
    // Visulalizes object properties hierarchy in editable form
    published.ObjectInspector = function (obj, cfg) {
      var self = this;

      WAVE.extend(this, WAVE.EventManager);

      var fTree = new published.Tree({ DIV: cfg.div });

      cfg = cfg || {};

      var fWVObjectInspectorEditor = cfg.wvObjectInspectorEditor || published.CLS_OBJECTINSPECTOR_EDITOR;
      this.wvObjectInspectorEditor = function (val) {
        if (typeof (val) === tUNDEFINED) return fWVObjectInspectorEditor;
        fWVObjectInspectorEditor = val;
      };

      var HTML_EDITOR = '<div class="@cls@">' +
                        '  <label for="@id@">@key@</label>' +
                        '  <input type="textbox" id="@id@" value="@val@" objpath="@objpath@">' +
                        '</div>';

      function onInput(e) {
          var keys = e.target.getAttribute("objpath").split(/\//);
          var objToEdit = obj;
          for (var i = 0; i < keys.length-1; i++)
              objToEdit = objToEdit[keys[i]];
              
          objToEdit[keys[keys.length - 1]] = e.target.value;
      }

      function build(troot, oroot, orootpath) {
        var keys = Object.keys(oroot);
        var cn;
        for (var iKey in keys) {
          var key = keys[iKey];
          var val = oroot[key];
          if (val !== null && typeof (val) !== "object") { //leaf
            var editorID = fObjectInspectorEditorIDPrefix + (fObjectInspectorEditorIDSeed++);
            var html = WAVE.strTemplate(HTML_EDITOR, {
              id: editorID, key: key, val: val,
              objpath: orootpath ? orootpath + "/" + key : key,
              cls: fWVObjectInspectorEditor
            });

            cn = troot.addChild({ html: html });
            
            var input = WAVE.id(editorID);
            input.objpath = cn.path();
            input.addEventListener("input", onInput);
          } else { //branch
            cn = troot.addChild({ html: key });
            build(cn, val, orootpath ? orootpath + "/" + key : key);
          }
        }
      }

      build(fTree.root(), obj, "");
    };//ObjectInspector


    // ImageBox {
    published.IBox = function (container, init) {
    
// ************************* Fields *************************
    
      if (WAVE.isStringType(typeof (container)))
        container = WAVE.id(container);
    
      if (!WAVE.isObject(init))
        init = {};
    
      var ibox = this;
      WAVE.extend(ibox, WAVE.EventManager);
    
      var seed = "ibox_"+WAVE.genRndKey(4);
      var ids = {
        divFrameId: "divFrame_"+seed,
        divThumbsContainerId: "divThumbsContainer_"+seed,
        divNavBackId: "divNavBack_" + seed,
        imgNavBackId: "imgNavBack_" + seed,
        divThumbsImagesContainerId: "divThumbsImagesContainer_" + seed,
        divNavForthId: "divNavForth_" + seed,
        imgNavForthId: "imgNavForth_" + seed,
        divMainImageId: "divMainImage_" + seed,
        imgMainImageId: "imgMainImage_" + seed
      };
      var isMainImageMouseDown = false;
      var isNavAnimationInProgress = false;
      var navTouchStartPosition = null;
      var thumbsOrientation;
      var thumbsScrollDelta;
      var thumbsData;
      var imagesNaturalSizeCache = {};

// ************************* Utilities *************************
   
      // Returns pointer's position
      function getPos(e) {
        return { 
                 x: (isNaN(e.pageX) ? e.originalEvent.touches[0].pageX : e.pageX),
                 y: (isNaN(e.pageY) ? e.originalEvent.touches[0].pageY : e.pageY)
               };
      }

      // Cancels event bubbling and prevents default action
      function cancelBubbleAndDefault(e) {
        if (e.stopPropagation) e.stopPropagation();
        if (e.cancelBubble) e.cancelBubble = true;
        if (e.preventDefault) e.preventDefault();
        e.returnValue = false;
        return false;
      }

      // invokes ERROR event
      function handle(src, message, rethrow) {
        if (rethrow !== false) rethrow = true;
        ibox.eventInvoke(published.EVT_IBOX_ERROR, { src: src, message: message });
        if (rethrow) throw error;
      }

// ****************** Thumbs and Navigation ********************

      // 'True' if IBox is vertically oriented (thumb container is located on the left or right), 'False' otherwise
      function isVerticalOrientation() {
        return thumbsOrientation === published.POS_LEFT  || 
               thumbsOrientation === published.POS_RIGHT;
      }

      function areThumbsOverflowed() {
        var thumbsImagesContainer = WAVE.id(ids.divThumbsImagesContainerId);
        return isVerticalOrientation() ?
                  thumbsImagesContainer.scrollHeight > thumbsImagesContainer.clientHeight :
                  thumbsImagesContainer.scrollWidth > thumbsImagesContainer.clientWidth;
      } 

      function alignThumbContainer() {
        try {
          if (thumbsData.length < 1)
            return;

          var first = WAVE.id(thumbsData[0].id);
          var thumbsContainer = WAVE.id(ids.divThumbsImagesContainerId);
          var parent = thumbsContainer.parentElement;
          var thumbsFullWidth = 0;
          var thumbsFullHeight = 0;
          for (var i = 0; i < thumbsData.length; i++) {
            var thumb = WAVE.id(thumbsData[i].id);
            thumbsFullWidth += thumb.clientWidth;
            thumbsFullHeight += thumb.clientHeight;
          }

          if (isVerticalOrientation())
            first.style.marginTop = (parent.clientHeight - thumbsFullHeight)/2 + "px";
          else
            first.style.marginLeft = (parent.clientWidth - thumbsFullWidth)/2 + "px";
        }
        catch (error) {
          handle("alignThumbContainer", error );
        }
      }
    
      function showNavigation() {
        try {
          if (thumbsData.length < 1)
            return;
        
          var first = WAVE.id(thumbsData[0].id);
          first.style.marginTop = "";
          first.style.marginLeft = "";

          var divThumbsImagesContainer = WAVE.id(ids.divThumbsImagesContainerId);
          var divNavBack = WAVE.id(ids.divNavBackId);
          var divNavForth = WAVE.id(ids.divNavForthId);

          if (isVerticalOrientation()) {
            divThumbsImagesContainer.style.height = "90%";
            $(divNavBack).attr("style", published.STL_DIV_THUMBSNAV_V); // ios throws exception when setting style directly as .style=...
            divNavBack.style.display = "";
            $(divNavForth).attr("style", published.STL_DIV_THUMBSNAV_V); // ios throws exception when setting style directly as .style=...
            divNavForth.style.display = "";
          } else {
            divThumbsImagesContainer.style.width = "90%";
            $(divNavBack).attr("style", published.STL_DIV_THUMBSNAV_H); // ios throws exception when setting style directly as .style=...
            divNavBack.style.display = "inline-block";
            divNavForth.style.height = published.STL_DIV_THUMBSNAV_H;
            divNavForth.style.display = "inline-block";
          }
        }
        catch (error) {
          handle("showNavigation", error);
        }
      }

      function hideNavigation() {
        WAVE.id(ids.divNavBackId).style.display = "none";
        WAVE.id(ids.divNavForthId).style.display = "none";
      }
      
      function refreshNavigation() {
        try {
          var needNavigation = areThumbsOverflowed();
          if (needNavigation) {
            showNavigation();
            fitImageToContainer(ids.imgNavBackId);
            fitImageToContainer(ids.imgNavForthId);
          } else {
            hideNavigation();
            alignThumbContainer();
          }
        }
        catch (error) {
          handle("refreshNavigation", error);
        }
      }

      function areAllThumbsLoaded() {
        for (var i = 0; i < thumbsData.length; i++) {
          if (!thumbsData[i].imageLoaded)
            return false;
        }
        return true;
      }

// ******************* Navigation Scrolling ******************** 

      function canScrollBack() {
        var scrollContainer = WAVE.id(ids.divThumbsImagesContainerId);
        return isVerticalOrientation() ?
                  scrollContainer.scrollTop > 0 :
                  scrollContainer.scrollLeft > 0;
      }

      function canScrollForth() {
        var scrollContainer = WAVE.id(ids.divThumbsImagesContainerId);
        return isVerticalOrientation() ?
                  scrollContainer.scrollTop  + scrollContainer.clientHeight < scrollContainer.scrollHeight - 1 :
                  scrollContainer.scrollLeft + scrollContainer.clientWidth  < scrollContainer.scrollWidth  - 1;
      }

      function scrollBack() {
        var scrollContainer = WAVE.id(ids.divThumbsImagesContainerId);
        return isVerticalOrientation() ?
                  { scrollTop:  scrollContainer.scrollTop  - thumbsScrollDelta * scrollContainer.clientHeight } :
                  { scrollLeft: scrollContainer.scrollLeft - thumbsScrollDelta * scrollContainer.clientWidth  };
      }

      function scrollForth() {
        var scrollContainer = WAVE.id(ids.divThumbsImagesContainerId);
        return isVerticalOrientation() ?
                  { scrollTop:  scrollContainer.scrollTop  + thumbsScrollDelta * scrollContainer.clientHeight } :
                  { scrollLeft: scrollContainer.scrollLeft + thumbsScrollDelta * scrollContainer.clientWidth  };
      }

      function refreshScrolling() {
        try {
          var divNavBack  = WAVE.id(ids.divNavBackId);
          var divNavForth = WAVE.id(ids.divNavForthId);
          var navBackOpacity  = canScrollBack()  ? 1 : published.DEFAULT_ARROWS_OPACITY;
          var navForthOpacity = canScrollForth() ? 1 : published.DEFAULT_ARROWS_OPACITY;
          divNavBack.style.opacity  = navBackOpacity;
          divNavForth.style.opacity = navForthOpacity;
        }
        catch (error) {
          handle("refreshScrolling", error);
        }
      }

      function initializeScrolling() { 
        var scrollContainer = WAVE.id(ids.divThumbsImagesContainerId);
        var divNavBack = WAVE.id(ids.divNavBackId);
        var divNavForth = WAVE.id(ids.divNavForthId);

        $(divNavBack).bind("touchend click", function () {
          if (isNavAnimationInProgress) return;
          isNavAnimationInProgress = true;
          $(scrollContainer).animate(scrollBack(), 
                                     function() {
                                       refreshScrolling();
                                       isNavAnimationInProgress = false;
                                     });
          });

        $(divNavForth).bind("touchend click", function () {
          if (isNavAnimationInProgress) return;
          isNavAnimationInProgress = true;
          $(scrollContainer).animate(scrollForth(), 
                                     function() {
                                       refreshScrolling();
                                       isNavAnimationInProgress = false;
                                     });
          });
      }

// ******************** Working with Images ********************

      // Returns original ('natural') image size via temporary Image element (imageElement.natural* is not enough for Safari at startup). 
      // Performs caching by image src.
      function getNaturalImageSize(imageElement) {
        try {
          var size = imagesNaturalSizeCache[imageElement.src];
          if (!size) {
            var height = imageElement.naturalHeight;
            var width = imageElement.naturalWidth;
            if (height === 0 || width === 0) {
              var tmp = new Image();
              tmp.src = imageElement.src;
              width = tmp.width;
              height = tmp.height;
            }

            size = { width: width, height: height };
            imagesNaturalSizeCache[imageElement.src] = size;
          }

          return size;
        }
        catch (error) {
          handle("getNaturalImageSize", error);
        }
      }

      function fitImageToContainer(imageElementId) {
        try {
          var imageElement = WAVE.id(imageElementId);
          var size = getNaturalImageSize(imageElement);
          var iw = size.width;
          var ih = size.height;
          var w = imageElement.parentElement.clientWidth;
          var h = imageElement.parentElement.clientHeight;

          if ((w * ih) / (iw * h) > 1) {
            var width = h * iw / ih;
            imageElement.style.width = width + "px";
            imageElement.style.height = h + "px";
            imageElement.style.left = (w - width) / 2 + "px";
            imageElement.style.top = "0";
          } else {
            var height = w * ih / iw;
            imageElement.style.width = w + "px";
            imageElement.style.height = height + "px";
            imageElement.style.left = "0";
            imageElement.style.top = (h - height) / 2 + "px";
          }
        }
        catch (error) {
          handle("fitImageToContainer", error);
        }
      }

      function fitThumbImageToContainer(thumb) {
        try {
          var divThumb = WAVE.id(thumb.id);
          var divCrop = WAVE.id(thumb.cropId);
          var imgImage = WAVE.id(thumb.imageId);
        
          if (isVerticalOrientation()) {
            divThumb.style.height = divThumb.clientWidth + "px";
          } else {
            divThumb.style.width = divThumb.clientHeight + "px";
          }

          var size = getNaturalImageSize(imgImage);
          var naturalWidth = size.width;
          var naturalHeight = size.height;

          var imageHeight = (typeof (thumb.imageHeight) === WAVE.UNDEFINED) || isNaN(thumb.imageHeight) ? naturalHeight : thumb.imageHeight;
          var imageWidth = (typeof (thumb.imageWidth) === WAVE.UNDEFINED) || isNaN(thumb.imageWidth) ? naturalWidth : thumb.imageWidth;
          var imageLeft = (typeof (thumb.imageLeft) === WAVE.UNDEFINED) || isNaN(thumb.imageLeft) ? 0 : thumb.imageLeft; 
          var imageTop = (typeof (thumb.imageTop) === WAVE.UNDEFINED) || isNaN(thumb.imageTop) ? 0 : thumb.imageTop; 

          var delta = Math.min(divThumb.clientHeight / imageHeight, divThumb.clientWidth / imageWidth);

          divCrop.style.height  = (imageHeight * delta) + "px";
          divCrop.style.width   = (imageWidth * delta) + "px";
          divCrop.style.top     = (divThumb.clientHeight - imageHeight * delta)/2 + "px";
          divCrop.style.left    = (divThumb.clientWidth - imageWidth * delta)/2 + "px";
          imgImage.style.height = (imgImage.naturalHeight * delta) + "px";
          imgImage.style.width  = (imgImage.naturalWidth * delta) + "px";
          imgImage.style.left   = (imageLeft * delta) + "px";
          imgImage.style.top    = (imageTop * delta) + "px";
        }
        catch (error) {
          handle("fitThumbImageToContainer", error);
        }
      }
      
      function loadMainImage(imageSrc) {
        var imageElement = WAVE.id(ids.imgMainImageId);
        if (imageSrc === imageElement.src)
          return;

        var downloadingImage = new Image();
        downloadingImage.onload = function () {
          var src = this.src;
          $(imageElement).fadeOut(published.DEFAULT_IMAGE_FADEOUT_TIME, function () {
            imageElement.src = src;
            fitImageToContainer(ids.imgMainImageId);
          }).fadeIn(published.DEFAULT_IMAGE_FADEIN_TIME, function () {
            ibox.eventInvoke(published.EVT_IBOX_IMAGE_CHANGED, imageSrc);
          });
        };
        downloadingImage.onerror = function () {
          handle("loadMainImage", "failed to load main image, src=" + src, false);
          imageElement.src = published.DEFAULT_IMAGE;
          fitImageToContainer(ids.imgMainImageId);
        };
        downloadingImage.src = imageSrc;
      } 

      // Force recalculate and redraw IBox's UI
      function doRefreshMarkup() {
        try {
          // refresh thumb images
          for (var i = 0; i < thumbsData.length; i++) {
            fitThumbImageToContainer(thumbsData[i]);
          }

          // refresh scrollers
          refreshNavigation();

          // refresh main image
          fitImageToContainer(ids.imgMainImageId);
        }
        catch (error) {
          handle("doRefreshMarkup", error);
        }
      }

      function onThumbImageLoaded(thumb, thumbImageSrc) {
        thumb.imageLoaded = true;
        var thumbElement = WAVE.id(thumb.imageId);
        thumbElement.src = thumbImageSrc;
        fitThumbImageToContainer(thumb);
        if (areAllThumbsLoaded()) {
          doRefreshMarkup();
        }
        else {
          refreshNavigation();
        }
      }
       
      function loadThumbImage(thumb) {                                                     
        var thumbElement = WAVE.id(thumb.imageId);
        if (thumb.thumbImageSrc === thumbElement.src)
          return;

        var downloadingImage = new Image();
        downloadingImage.onload = function () {
          var src = this.src;
          $(thumbElement).fadeOut(published.DEFAULT_IMAGE_FADEOUT_TIME, function () {
            onThumbImageLoaded(thumb, src);
          }).fadeIn(published.DEFAULT_IMAGE_FADEIN_TIME);
        };
        downloadingImage.onerror = function () {
          handle("loadThumbImage", "failed to load thumb image, src=" + src, false);
          onThumbImageLoaded(thumb, published.DEFAULT_IMAGE);
        };
        downloadingImage.src = thumb.thumbImageSrc;
      }

      function calculateImageZoomOffsets(ih, iw, h, w, x, y) {
        try {
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
        catch (error) {
          handle("calculateImageZoomOffsets", error);
        }
      }
    
      // Called when user clicks/taps on the main image area.
      // It resets image's size to its original ('natural') value and shifts the image according to poiter position
      function adjustMainImage(e) {
        try {
          var img = WAVE.id(ids.imgMainImageId);
          var div = WAVE.id(ids.divMainImageId);
          var size = getNaturalImageSize(img);
          var iw = size.width;
          var ih = size.height;
          var w = div.clientWidth;
          var h = div.clientHeight;
    
          if (iw <= w && ih <= h)
            return false;

          var pos = getPos(e);
          var ex = pos.x - div.offsetLeft;
          var ey = pos.y - div.offsetTop;
          var offset = calculateImageZoomOffsets(ih, iw, h, w, ex, ey);
          img.style.width = "";
          img.style.height = "";
          img.style.left = (-offset.left) + "px";
          img.style.top = (-offset.top) + "px";

          return true;
        }
        catch (error) {
          handle("adjustMainImage", error);
        }
      }

// ******************** Event Handlers ********************

      function divMainImageMouseDown(e) {
        try {
          if (adjustMainImage(e)) isMainImageMouseDown = true;
          return cancelBubbleAndDefault(e);
        }
        catch (error) {
          handle("divMainImageMouseDown", error);
        }
      }
    
      function divMainImageMouseMove(e) {
        try {
          if (isMainImageMouseDown) adjustMainImage(e);
          return cancelBubbleAndDefault(e);
        }
        catch (error) {
          handle("divMainImageMouseMove", error);
        }
      }
    
      function onMouseUp(e) {
        try {
          // if thumb navigation in progress
          if (navTouchStartPosition !== null) {
            refreshNavigation();
            navTouchStartPosition = null;
          }

          // if main image zooming finished 
          if (isMainImageMouseDown) {
            fitImageToContainer(ids.imgMainImageId);
            isMainImageMouseDown = false;
          } 

          return cancelBubbleAndDefault(e);
        }
        catch (error) {
          handle("onMouseUp", error);
        }
      }
    
      function divThumbsContainerMouseDown(e) {
        try {
          navTouchStartPosition = getPos(e);
          return cancelBubbleAndDefault(e);
        }
        catch (error) {
          handle("divThumbsContainerMouseDown", error);
        }
      }
    
      function divThumbsContainerMouseMove(e) {
        try {
          if (navTouchStartPosition !== null) {
            var scrollContainer = WAVE.id(ids.divThumbsImagesContainerId);
            var delta;
            var pos = getPos(e);
            if (isVerticalOrientation()) {
              delta = pos.y - navTouchStartPosition.y;
              scrollContainer.scrollTop += - delta;
            } else {
              delta = pos.x - navTouchStartPosition.x;
              scrollContainer.scrollLeft += - delta;
            }
            navTouchStartPosition = pos;
            refreshScrolling();
          }
          return cancelBubbleAndDefault(e);
        }
        catch (error) {
          handle("divThumbsContainerMouseMove", error);
        }
      }

      function thumbClickFactory(imageSrc) {
        return function () { loadMainImage(imageSrc); };
      }
      
// ********************** Public *************************

      // Toggle IBox's visibility
      this.visible = function (val) {
        var vis = WAVE.id(ids.divFrameId).style.visibility;
        if (vis === "") vis = "visible";
        var isv = vis === "visible";

        if (typeof (val) === WAVE.UNDEFINED || val === isv) return val;

        WAVE.id(ids.divFrameId).style.visibility = val ? "visible" : "hidden";
        ibox.eventInvoke(published.EVT_INTERACTION_CHANGE, "visible", val);
        return val;
      };

      // Force recalculate and redraw IBox's UI
      this.refreshMarkup = function () {
        doRefreshMarkup();
      };

// ******** Initialization and UI Construction ***********

      // parse init json
      try {
        var imagesList = init[published.PARAM_IMAGES_LIST];
        var defaultImageSrcKey = init[published.PARAM_DEF_IMG_SRC];
        var defaultImageSrc = (typeof (defaultImageSrcKey) === WAVE.UNDEFINED) ?
                              published.DEFAULT_IMAGE :
                              WAVE.strDefault(imagesList[defaultImageSrcKey], published.DEFAULT_IMAGE);
        var defaultThumbSrcKey = init[published.PARAM_DEF_THUMB_SRC];
        var defaultThumbSrc = (typeof (defaultThumbSrcKey) === WAVE.UNDEFINED) ?
                              published.DEFAULT_IMAGE :
                              WAVE.strDefault(imagesList[defaultThumbSrcKey], published.DEFAULT_IMAGE);
        thumbsData = init[published.PARAM_THUMBS];
        thumbsScrollDelta = WAVE.strDefault(init[published.PARAM_THUMBS_SCROLL_DELTA], published.DEFAULT_THUMBS_SCROLL_DELTA);
        thumbsOrientation = WAVE.strDefault(init[published.PARAM_THUMBS_POSITION], published.DEFAULT_THUMBS_POSITION);
      }
      catch (error) {
        handle(".ctor 'parse init json'", error);
      }

      // UI initialization
      try {
        var imgMainImage = document.createElement("img");
        imgMainImage.id = ids.imgMainImageId;
        imgMainImage.className = published.CLS_IBOX_IMG_MAINIMAGE;
        imgMainImage.src = published.DEFAULT_IMAGE;
        imgMainImage.style.cssText = published.STL_IMG_MAINIMAGE + published.STL_DISABLE_MOBILE_HANDLING;
        imgMainImage.ondragstart = function() { return false; };
        
        var divThumbsImagesContainer = document.createElement("div");
        divThumbsImagesContainer.id = ids.divThumbsImagesContainerId;
        divThumbsImagesContainer.className = published.IBOX_DIV_THUMBSIMAGESCONTAINER;
        
        if (typeof (thumbsData) !== WAVE.UNDEFINED) {
          for (var i = 0; i < thumbsData.length; ++i) {
            var thumb = thumbsData[i];
        
            var id      = "divThumbId_ibox_" + i + "_" + WAVE.genRndKey(4);
            var cropId  = "divThumbCropId_ibox_" + i + "_" + WAVE.genRndKey(4);
            var imageId = "imgThumbImageId_ibox_" + i + "_" + WAVE.genRndKey(4);
            var bigImageSrcKey = thumb[published.PARAM_BIG_IMG_SRC];
            var bigImageSrc = (typeof (bigImageSrcKey) === WAVE.UNDEFINED) ? 
                              published.DEFAULT_IMAGE :
                              WAVE.strDefault(imagesList[bigImageSrcKey], defaultImageSrc);
            var thumbImageSrcKey = thumb[published.PARAM_THUMB_IMG_SRC];
            var thumbImageSrc = (typeof (thumbImageSrcKey) === WAVE.UNDEFINED) ? 
                                published.DEFAULT_IMAGE :
                                WAVE.strDefault(imagesList[thumbImageSrcKey], defaultThumbSrc);
            
            if (thumbImageSrc !== published.DEFAULT_IMAGE && thumbImageSrc !== defaultThumbSrc) {
              thumb.imageHeight = parseFloat(thumb[published.PARAM_HEIGHT]);
              thumb.imageWidth  = parseFloat(thumb[published.PARAM_WIDTH]);
              thumb.imageLeft   = parseFloat(thumb[published.PARAM_LEFT]);
              thumb.imageTop    = parseFloat(thumb[published.PARAM_TOP]);
            }
        
            thumb.imageLoaded = false;
            thumb.bigImageSrc = bigImageSrc;
            thumb.thumbImageSrc = thumbImageSrc;
            thumb.id = id;
            thumb.cropId = cropId;
            thumb.imageId = imageId;
        
            // thumbnails UI templatization
        
            var divThumbStyle = isVerticalOrientation() ? published.STL_DIV_THUMB_V : published.STL_DIV_THUMB_H;
        
            var divThumbTemplate = 
                  "<div id='@divThumbId@' class='@divThumbClass@' style='@divThumbStyle@'>" +
                    "<div id='@divThumbCropId@' class='@divThumbCropClass@' style='@divThumbCropStyle@'>" +
                      "<img id='@divThumbImageId@' " +
                           "class='@divThumbImageClass@' " +
                           "style='@divThumbImageStyle@' " +
                           "src='@initialSrc@' " +
                           "ondragstart='return false;'>" +
                    "</div>" +
                  "</div>";
            divThumbsImagesContainer.innerHTML += WAVE.strHTMLTemplate(divThumbTemplate,
                  {           
                    divThumbId: id,
                    divThumbClass: published.CLS_IBOX_DIV_THUMB,
                    divThumbStyle: divThumbStyle,  
                    divThumbCropId: cropId,
                    divThumbCropClass: published.CLS_IBOX_DIV_THUMB_CROP,
                    divThumbCropStyle: published.STL_DIV_THUMB_CROP,
                    divThumbImageId: imageId,
                    initialSrc: published.DEFAULT_IMAGE,
                    divThumbImageClass: published.CLS_IBOX_DIV_THUMB_IMAGE,
                    divThumbImageStyle: published.STL_DIV_THUMB_IMAGE + published.STL_DISABLE_MOBILE_HANDLING
                  });
          }
        }
      }
      catch (error) {
        handle(".ctor 'UI initialization'", error);
      }

      // UI templatization 
      try {
        var imageFloat;
        var divThumbsContainerStyle;
        var divNavStyle;
        var divMainImageStyle;
        var imgNavBackSrc;
        var imgNavForthSrc;
        
        if (isVerticalOrientation()) {
          divThumbsImagesContainer.style.cssText = published.STL_DIV_THUMBSIMAGESCONTAINER_V;
          divThumbsContainerStyle = published.STL_DIV_THUMBSCONTAINER_V;
          divNavStyle = published.STL_DIV_THUMBSNAV_V;
          divMainImageStyle = published.STL_DIV_MAINIMAGE_V;
          imgNavBackSrc = published.DEFAULT_UP_NAVIGATION_IMAGE;
          imgNavForthSrc = published.DEFAULT_DOWN_NAVIGATION_IMAGE; 
          imageFloat = thumbsOrientation === published.POS_LEFT ? "right" : "left";
        } else {
          divThumbsImagesContainer.style.cssText = published.STL_DIV_THUMBSIMAGESCONTAINER_H;
          divThumbsContainerStyle = published.STL_DIV_THUMBSCONTAINER_H;
          divNavStyle = published.STL_DIV_THUMBSNAV_H;
          divMainImageStyle = published.STL_DIV_MAINIMAGE_H;
          imgNavBackSrc = published.DEFAULT_LEFT_NAVIGATION_IMAGE;
          imgNavForthSrc = published.DEFAULT_RIGHT_NAVIGATION_IMAGE;
          imageFloat = "";
        }
        
        var imageBlockTemplate = 
          "<div id='@divMainImageId@' class='@divMainImageClass@' style='@divMainImageStyle@ float: @imageFloat@'>" +
              imgMainImage.outerHTML +
          "</div>";
        var thumbsContainerBlockTemplate =
          "<div id='@divThumbsContainerId@' class='@divThumbsContainerClass@' style='@divThumbsContainerStyle@'>" +
            "<div id='@divNavBackId@' class='@divNavBackClass@' style='@divNavStyle@ opacity: @defaultOpacity@'>" +
              "<img id='@imgNavBackId@' style='@imgNavBackStyle@' src='@imgNavBackSrc@'>" +
            "</div>" +
            divThumbsImagesContainer.outerHTML +
            "<div id='@divNavForthId@' class='@divNavForthClass@' style='@divNavStyle@'>" +
              "<img id='@imgNavForthId@' style='@imgNavForthStyle@' src='@imgNavForthSrc@'>" +
            "</div>" +
          "</div>";
        
        var divFrameTemplate;
        if (isVerticalOrientation() || thumbsOrientation === published.POS_BOTTOM) {
          divFrameTemplate =
            "<div id='@divFrameId@' class='@divFrameClass@' style='@divFrameStyle@'>" +
              imageBlockTemplate +
              thumbsContainerBlockTemplate +
            "</div>";
        } else {
          divFrameTemplate =
            "<div id='@divFrameId@' class='@divFrameClass@' style='@divFrameStyle@'>" +
              thumbsContainerBlockTemplate +
              imageBlockTemplate +
            "</div>";
        }
        
        container.innerHTML = WAVE.strHTMLTemplate(divFrameTemplate,
              {
                divFrameId: ids.divFrameId,
                divFrameClass: published.CLS_IBOX_DIV_FRAME,
                divFrameStyle: published.STL_DIV_FRAME,
                          
                divMainImageId: ids.divMainImageId,
                divMainImageClass: published.CLS_IBOX_DIV_MAINIMAGE,
                divMainImageStyle: divMainImageStyle,
                imageFloat: imageFloat,
        
                divThumbsContainerId: ids.divThumbsContainerId,
                divThumbsContainerClass: published.CLS_IBOX_DIV_THUMBSCONTAINER,
                divThumbsContainerStyle: divThumbsContainerStyle,
                
                divNavBackId: ids.divNavBackId,
                defaultOpacity: published.DEFAULT_ARROWS_OPACITY,
                divNavBackClass: published.CLS_IBOX_DIV_THUMBSNAVIGATION,
                imgNavBackId: ids.imgNavBackId,
                imgNavBackStyle: published.STL_IMG_THUMBSNAV,
                imgNavBackSrc: imgNavBackSrc,
                divNavStyle: divNavStyle,
                
                divNavForthId: ids.divNavForthId,
                divNavForthClass: published.CLS_IBOX_DIV_THUMBSNAVIGATION,
                imgNavForthId: ids.imgNavForthId,
                imgNavForthStyle: published.STL_IMG_THUMBSNAV,
                imgNavForthSrc: imgNavForthSrc
              });
      }
      catch (error) {
        handle(".ctor 'UI templatization'", error);
      }

      // apply handlers
      var divMainImage = WAVE.id(ids.divMainImageId);
      var divThumbsContainer = WAVE.id(ids.divThumbsContainerId);
      $(divMainImage).bind("touchstart mousedown", divMainImageMouseDown);
      $(divMainImage).bind("touchmove mousemove", divMainImageMouseMove);
      $(divThumbsContainer).bind("touchstart mousedown", divThumbsContainerMouseDown);
      $(divThumbsContainer).bind("touchmove mousemove", divThumbsContainerMouseMove);

      initializeScrolling();
      
      // load the 1st image
      fitImageToContainer(ids.imgMainImageId);
      if (thumbsData.length > 0) {
        thumbClickFactory(thumbsData[0].bigImageSrc)();
      }
      
      // load thumb images
      for (var j = 0; j < thumbsData.length; j++) {
        var tmb = thumbsData[j];
        fitThumbImageToContainer(tmb);
        var thumbImage = WAVE.id(tmb.imageId);
        $(thumbImage).bind("touchend click", thumbClickFactory(tmb.bigImageSrc));
        loadThumbImage(tmb);
      }

      // global events
      
      $(window).resize(function() { doRefreshMarkup(); });
      
      $(document).bind("touchend mouseup", onMouseUp);
    };

  // {
  //   "DIV": html container,
  //   "id" : "uniqueId",
  //   "value" : "",
  //   "disabled" : false,
  //   "readonly" : false,
  //   "placeholder" : "some text",
  //   "attrs" : {attr : value}
  // }
    published.MultiLineTextBox = function (init) {
      if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "MultiLineTextBox.ctor(init.DIV)";
      if (WAVE.strEmpty(init.id)) throw "MultiLineTextBox.ctor(init.id)";

      var editor = this; 
      WAVE.extend(editor, WAVE.EventManager);
      var fDIV = init.DIV;
      var fId = init.id;
      var fAddLabel = WAVE.strAsBool(init.addLabel, true);
      var fShadowDivId = "shdw_div_" + fId;
      var fPlaceholder = WAVE.strDefault(init.placeholder);
      var fReadOnly = WAVE.strAsBool(init.readonly, false);
      var fDisabled = WAVE.strAsBool(init.disabled, false);
      var fValue = WAVE.strDefault(init.value);
      var fAttrs = WAVE.tryParseJSON(init.attrs).obj; 

      var fTemplate = 
        "<div id='@shdwDivId@' style='position: fixed;visibility: hidden;height: auto;width: auto;'></div>" +
        (fAddLabel ? "<label for='@id@'>@placeholder@</label>" : "") +
        "<textarea id='@id@' @disabled@ @readonly@ style='resize: none' placeholder='@placeholder@' title='@placeholder@'></textarea>";

      fDIV.innerHTML = WAVE.strHTMLTemplate(fTemplate, {
        shdwDivId: fShadowDivId,
        id : fId,             
        disabled: fDisabled ? "disabled" : "",
        readonly: fReadOnly ? "readonly" : "",
        placeholder : fPlaceholder
      });

      var dI = WAVE.id(fId);
      var shadowDiv = WAVE.id(fShadowDivId);

      dI.onresize = function () { shadowDiv.style.maxWidth = WAVE.styleOf(dI, 'width'); };
      dI.style.overflow = 'hidden';
      for(var n in fAttrs){
        dI.setAttribute(n, fAttrs[n]);
      }

      shadowDiv.innerText = dI.value = fValue;
      shadowDiv.style.maxWidth = WAVE.styleOf(dI, 'width');
      shadowDiv.style.font = WAVE.styleOf(dI, 'font');
      shadowDiv.style.minHeight = WAVE.styleOf(dI, 'height');
      shadowDiv.style.paddingTop = WAVE.styleOf(dI, 'padding-top');
      shadowDiv.style.paddingRight = WAVE.styleOf(dI, 'padding-right');
      shadowDiv.style.paddingBottom = WAVE.styleOf(dI, 'padding-bottom');
      shadowDiv.style.paddingLeft = WAVE.styleOf(dI, 'padding-left');
      shadowDiv.style.wordWrap = WAVE.styleOf(dI, 'word-wrap');
      shadowDiv.style.wordSpacing = WAVE.styleOf(dI, 'word-spacing');
      shadowDiv.style.whiteSpace = WAVE.styleOf(dI, 'white-space');
      shadowDiv.style.borderSpacing = WAVE.styleOf(dI, 'border-spacing');
      shadowDiv.style.letterSpacing = WAVE.styleOf(dI, 'letter-spacing');
      shadowDiv.style.lineHeight = WAVE.styleOf(dI, 'line-height');
      shadowDiv.style.boxSizing = WAVE.styleOf(dI, 'box-sizing');
      shadowDiv.style.border = WAVE.styleOf(dI, 'border');
      shadowDiv.style.borderWidth = WAVE.styleOf(dI, 'border-width');
      shadowDiv.style.borderRadius = WAVE.styleOf(dI, 'border-radius');
      shadowDiv.style.textIndent = WAVE.styleOf(dI, 'text-indent');

      var setAreaHeight = function (control) {
        shadowDiv.innerText = WAVE.strEmpty(control.value) ? 'W' : control.value.replace(/\r\n|\r|\n/g, "\n ");
        control.style.height = shadowDiv.clientHeight + "px";
      };

      dI.onchange = function (e) {
        setAreaHeight(this);
        shadowDiv.style.maxWidth = WAVE.styleOf(dI, 'width');
        editor.eventInvoke(published.EVT_MULTI_LINE_TEXT_UPDATED, { phase: published.EVT_PHASE_AFTER, target: e.target, value: this.value });
      };
      dI.onkeydown = dI.onkeyup = function (e) {
        setAreaHeight(this);
      };

      setAreaHeight(dI);
      return editor;
    };//MultiLineTextBox

    var fPSEditorIDSeed = 0;

  // {
  //   "DIV": html container, --required
  //   "outputFormFieldName" : "fieldId"
  //   "sets" :[], 
  //   "content": "{}", 
  //   "disabled": false,
  //   "readonly": false,
  //   "canAdd": true,
  //   "canDel": true,
  //   "canRaw": true,
  //   "options" : {"n": {type : "text", plh: "Name"}, "d": {type : "textarea", plh: "Description"}},
  //   "btnClasses" : {
  //     "addBtnCls" : "class name",
  //     "delBtnCls" : "class name",
  //     "rawBtnCls" : "class name",
  //     "clearBtnCls" : "class name",
  //     "modalBtnCls" : "class name",
  //   },
  //   "texts" : {
  //     "add-title" : "Add New Language",
  //     "add-body" : "",
  //     "raw-title" : "Raw Edit",
  //     "raw-body" : "",
  //     "clear-confirm-title" : "Confirm Clear",
  //     "clear-confirm-body" : "Clear all language data?",
  //     "del-confirm-title" : "Confirm Delete",
  //     "del-confirm-body" : "",
  //     "btn-del" : "Delete",
  //     "btn-add" : "Add",
  //     "btn-raw" : "Edit Raw",
  //     "btn-clear" : "Clear",
  //     "n-plh" : "Name",
  //     "d-plh" : "Description",
  //   }
  // }
    published.PropSetEditor = function (init) {
      if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "PropSetEditor.ctor(init.DIV)";

      var editor = this;
      WAVE.extend(editor, WAVE.EventManager);

      fPSEditorIDSeed++;
      var fDIV = init.DIV;
      var fContent = WAVE.cloneEnsure(WAVE.tryParseJSON(init.content).obj);
      var fOutputFormFieldName = WAVE.strDefault(init.outputFormFieldName, null);
      var fReadOnly = WAVE.strAsBool(init.readonly, false);
      var fDisable = WAVE.strAsBool(init.disabled, false);
      var fCanRaw = WAVE.strAsBool(init.canRaw, true);
      var fCanAdd = WAVE.strAsBool(init.canAdd, true);
      var fCanDel = WAVE.strAsBool(init.canDel, true);
      var fSets =  WAVE.mergeArrays(["eng"], init.sets, function(a, b) { return a.toLowerCase() === b.toLowerCase(); }, function(a) { return a.toLowerCase(); });
      var fCurrentLocal = WAVE.strDefault(init.local, "eng");
      var fLocalizerSchema = "PSEditor";
      var fLocalizerField = "ps";
      var fTitle = WAVE.strDefault(init.title);
      var fSchema = WAVE.get(init, "schema", {n: {plh: "Name"}, d: {type : "textarea", plh: "Description"}});

      var fClasses = WAVE.get(init, "btnClasses", {});
        fClasses.addBtnCls = WAVE.strDefault(fClasses.addBtnCls, published.CLS_PS_BTN_ADD);
        fClasses.delBtnCls = WAVE.strDefault(fClasses.delBtnCls, published.CLS_PS_BTN_DEL);
        fClasses.rawBtnCls = WAVE.strDefault(fClasses.rawBtnCls, published.CLS_PS_BTN_RAW);
        fClasses.clearBtnCls = WAVE.strDefault(fClasses.clearBtnCls, published.CLS_PS_BTN_CLEAR);
        fClasses.modalBtnCls = WAVE.strDefault(fClasses.modalBtnCls, "");

      var fSeed = "ps_" + fPSEditorIDSeed;
      var fControlIds = {
        btnAddId : "addLang_" + fSeed,
        btnRawId : "rawEdit_" + fSeed,
        btnClearId : "clearBtn_" + fSeed,
        divTextTemplateId : "textTemplate_" + fSeed,
        divEditorsContainerId : "divEditorsContainer_" + fSeed,
        divDeleteButtonId : "deleteButtonId_" + fSeed,
        inputId : "_inputId_" + fSeed,
        divLangNameId : "islLabel_" + fSeed,
        outputHiddenId : "hidden_" + fSeed
      };

      var fTexts = WAVE.get(init, "texts", {});
        fTexts.addTitle = getText("add-title", "Add New Language");
        fTexts.addBody = getText("add-body", "");
        fTexts.rawTitle= getText("raw-title", "Raw Edit");
        fTexts.rawBody = getText("raw-body");
        fTexts.clearConfirmTitle = getText("clear-confirm-title", "Confirm Clear");
        fTexts.clearConfirmBody = getText("clear-confirm-body", "Clear all language data?");
        fTexts.delConfirmTitle = getText("del-confirm-title", "Confirm Delete");
        fTexts.delConfirmBody = getText("del-confirm-body");
        fTexts.btnDel = getText("btn-del", "Delete");
        fTexts.btnAdd = getText("btn-add", "Add");
        fTexts.btnRaw = getText("btn-raw", "Edit Raw");
        fTexts.btnClear = getText("btn-clear", "Clear");

      function getText(n, dflt){
        return localize(fTexts[n], dflt);
      }

      function localize(text, dflt){
        return WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, WAVE.strDefault(text, dflt));
      }

      function getNotAddedLangs() {
        var langs = [];
        for (var l in fSets) {
          if (!WAVE.has(fContent, fSets[l]))
            langs.push(fSets[l]);
        }
        return langs;
      }

      function addMainControls() {
        var addControlTemaplate =
          "<input id='@outerId@' type='hidden' name='@outerName@'/>" +
          "<div id='@divEditorsContainerId@' class='@divEditorsContainerClass@'></div>" +
          ((!fReadOnly && !fDisable) ?
            (fCanAdd ? "<input type='button' id='@btnAddId@' class='@btnAddClass@' value='@add@' />" : "") +
            (fCanRaw ? "<input type='button' id='@btnRawId@' class='@btnRawClass@' value='@raw@' />" : "") +
            (fCanDel ? "<input type='button' id='@btnClearId@' class='@btnClearClass@' value='@clear@' />" : "") : "");

        fDIV.innerHTML = WAVE.strHTMLTemplate(addControlTemaplate,
          {
            outerId: fControlIds.outputHiddenId,
            outerName: WAVE.strDefault(fOutputFormFieldName, ''),
            divEditorsContainerId: fControlIds.divEditorsContainerId,
            divEditorsContainerClass: published.CLS_PS_DIV_LANGS_CONTAINER,
            btnAddId: fControlIds.btnAddId,
            btnAddClass: fClasses.addBtnCls,
            btnRawId: fControlIds.btnRawId,
            btnRawClass: fClasses.rawBtnCls,
            btnClearId: fControlIds.btnClearId,
            btnClearClass: fClasses.clearBtnCls,

            add: fTexts.btnAdd,
            raw: fTexts.btnRaw,
            clear: fTexts.btnClear
          });
        if (!fReadOnly && !fDisable && fCanAdd && getNotAddedLangs().length === 0)
          WAVE.id(fControlIds.btnAddId).disabled = true;

        if (fReadOnly || fDisable) return;

        if (fCanAdd && getNotAddedLangs().length > 0) {
          WAVE.id(fControlIds.btnAddId).onclick = function () {
            var btnAdd = this;
            var ls = getNotAddedLangs();
            var opts = "";
            for (var l in ls) {
              opts += "<option>" + ls[l] + "</option>";
            }

            var inputId = "newIso_" + fSeed;
            published.showConfirmationDialog(
              pfxTitle() + fTexts.addTitle,
              "<div>" + fTexts.addBody + "</div>" +
              "<div><select class='" + published.CLS_PS_LANG_SELECTOR + "' id='" + inputId + "'>" + opts + "</select></div>",
              [published.DLG_OK, published.DLG_CANCEL],
              function (sender, result) {
                if (result === published.DLG_OK) {
                  var iso = WAVE.id(inputId).value;
                  editor.addLang(iso);
                  if (ls.length === 1)
                    btnAdd.disabled = true;
                }
                return true;
              },
              { btnCls: fClasses.modalBtnCls });
            WAVE.id(inputId).focus();
          };
        }

        if (fCanRaw) {
          WAVE.id(fControlIds.btnRawId).onclick = function () {
            var inputId = "raw" + fSeed;
            published.showConfirmationDialog(
              pfxTitle() + fTexts.rawTitle,
              "<div>" + fTexts.rawBody + "</div>" +
              "<div><textarea style='width: 70vw; height: 70vh;' class='" + published.CLS_PS_TEXTAREA_RAW_EDITOR + "' type='text' id='" + inputId + "'></textarea></div>",
              [published.DLG_OK, published.DLG_CANCEL],
              function (sender, result) {
                if (result === published.DLG_OK) {
                  var val = WAVE.id(inputId).value;
                  var res = WAVE.tryParseJSON(val);
                  if (res.ok){
                      if (WAVE.checkKeysUnique(res.obj)){
                        published.toast(WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, "JSON is valid, but some keys are duplicated"), "error");
                        return false;
                      }
                      editor.content(val);
                    }
                  else {
                    published.toast(WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, "Invalid JSON"), "error");
                    return false;
                  }
                }
                return true;
              },
              { btnCls: fClasses.modalBtnCls });
            WAVE.id(inputId).value = editor.content();
            WAVE.id(inputId).focus();
          };
        }

        if (fCanDel) {
          WAVE.id(fControlIds.btnClearId).onclick = function () {
            published.showConfirmationDialog(
              pfxTitle() + fTexts.clearConfirmTitle,
              "<div>"+fTexts.clearConfirmBody+"</div>",
              [published.DLG_OK, published.DLG_CANCEL],
              function (sender, result) {
                if (result === published.DLG_OK) {
                  editor.clear();
                }
                return true;
              },
              { btnCls: fClasses.modalBtnCls });
          };
        }
      }

      function getLangContainerId(iso){
        return  fSeed + "_" + iso;
      }

      function buildLang(iso) {
        var langId = getLangContainerId(iso);
        var langEditors = WAVE.id(fControlIds.divEditorsContainerId);
        var delButtonId = fControlIds.divDeleteButtonId + "_" + iso;
        var labelId = fControlIds.divLangNameId + "_" + iso;
        var inputId = fControlIds.inputId + "_" + iso;
        var controlContainerId = fControlIds.divTextTemplateId + "_" + iso;

        function uName(filedName, id){
          return filedName + id;
        }

        var html =
          "<fieldset id='@langId@'>" +      
          "<legend id='@isoLabelId@' class='@langClass@'></legend>" +
          "<div class='@textEditorsContainerClass@' id='@controlContainerId@'>" +
          "</div>" + 
          (fCanDel && !fReadOnly && !fDisable ? "<input type='button' id='@deleteButtonId@' class='@delCls@' value='@delText@'/>" : "") +
          "</fieldset>";

        var params = {
            langId: langId,
            controlContainerId: controlContainerId,
            deleteButtonId: delButtonId,
            isoLabelId: labelId,
            langClass: published.CLS_PS_DIV_LANG_NAME,
            labelContainerClass: published.CLS_PS_LABEL_CONTAINER,
            textEditorsContainerClass: published.CLS_PS_INPUT_CONTAINER,
            delCls: fClasses.delBtnCls,
            disabled: fDisable ? "disabled" : "",
            readonly: fReadOnly ? "readonly" : "",

            delText: fTexts.btnDel
          };

        langEditors.insertAdjacentHTML("beforeend", WAVE.strHTMLTemplate(html, params));
        WAVE.id(labelId).innerText = iso;

        var ed = new published.ObjectEditor({
          DIV: WAVE.id(controlContainerId),
          meta: {
            type: "object",
            schema: fSchema
          },
          content: fContent[iso],
          disabled : fDisable,
          readonly : fReadOnly
        });

        ed.eventBind(WAVE.GUI.EVT_OBJECT_EDITOR_UPDATED, function (e, d) {
          fContent[iso] = d;
          editor.content(JSON.stringify(fContent), false);
        });

        if (fCanDel && !fReadOnly && !fDisable) {
          WAVE.id(delButtonId).onclick = function () {
            published.showConfirmationDialog(
              pfxTitle() + fTexts.delConfirmTitle + " : " + iso.toUpperCase(),
              "<div>"+fTexts.delConfirmBody+"</div>",
              [published.DLG_OK, published.DLG_CANCEL],
              function (sender, result) {
                if (result === published.DLG_OK) {
                  editor.deleteLang(iso);
                }
                return true;
              },
              { btnCls: fClasses.modalBtnCls });
          };
        }
      }

      function buildEditors() {
        if (WAVE.isObject(fContent))
          for (var iso in fContent) buildLang(iso);
      }

      function rebuild() {
        while (fDIV.hasChildNodes()) {
          fDIV.removeChild(fDIV.lastChild);
        }
        addMainControls();
        buildEditors();
        setHiddenValue();
      }

      function setHiddenValue(){
         if (fOutputFormFieldName !== null) WAVE.id(fControlIds.outputHiddenId).value = editor.content();
      }

      addMainControls();

      this.DIV = function() { return fDIV; };

      this.jsonContent = function () { return fContent; };

      this.content = function (value, rebuild) {
        if (typeof (value) === tUNDEFINED) 
          return WAVE.empty(fContent) ? "" : JSON.stringify(fContent, null, 2);
                  
        var parsed = WAVE.tryParseJSON(value);
        if (parsed.ok) {
          rebuild = WAVE.strAsBool(rebuild, true);
          if (rebuild) { 
            for (var iso in fContent) {
              WAVE.removeElem(getLangContainerId(iso));
            } 
          }

          fContent = WAVE.cloneEnsure(parsed.obj);
          
          if(rebuild) buildEditors();

          setHiddenValue();
          editor.eventInvoke(published.EVT_PS_EDITOR_UPDATED, fContent);
        }
        return value;
      };

      this.title = function(value) {
          if (typeof (value) === tUNDEFINED) return fTitle;
          fTitle = WAVE.strDefault(value);
      };

      function pfxTitle(){
        return WAVE.strEmpty(fTitle) ? "" : (fTitle + ": ");
      }

      this.enable = function (value) {
        if (typeof(value) === tUNDEFINED) return !fDisable;
        
        fDisable = !(value===true);
        rebuild();
        return value;
      };

      this.readOnly = function (value) {
        if (typeof(value) === tUNDEFINED) return fReadOnly;
        
        fReadOnly = value===true;
        rebuild();
        return value;
      };

      this.canAdd = function (value) {
        if (typeof(value) === tUNDEFINED) return fCanAdd;
        
        fCanAdd = value===true;
        rebuild();
        return value;
      };

      this.canDel = function (value) {
        if (typeof(value) === tUNDEFINED) return fCanDel;
        
        fCanDel = value===true;
        rebuild();
        return value;
      };

      this.canRaw = function (value) {
        if (typeof (value) === tUNDEFINED) return fCanRaw;
        
        fCanRaw = value===true;
        rebuild();
        return value;
      };

      this.addLang = function (iso) {
        if (WAVE.strEmpty(iso)) return false;

        iso = iso.toLowerCase();
        if (WAVE.has(fContent, iso)) return false;

        fContent[iso] = {};
        buildLang(iso);

        editor.content(JSON.stringify(fContent));
        return true;
      };

      this.deleteLang = function (iso) {
        if (WAVE.strEmpty(iso)) return false;

        iso = iso.toLowerCase();
        if (!WAVE.has(fContent, iso)) return false;

        delete fContent[iso];
        WAVE.removeElem(getLangContainerId(iso));
        if (!fReadOnly && !fDisable) WAVE.id(fControlIds.btnAddId).disabled = false;
        editor.content(JSON.stringify(fContent));
        return true;
      };

      this.clear = function () {
        if (!fReadOnly && !fDisable) WAVE.id(fControlIds.btnAddId).disabled = false;
        editor.content("{}");
      };

      rebuild();
      return editor;
    };//PropSetEditor

    var fObjectEditorIdSeed = 0;
    //{
    //  DIV: div,
    //  content: {}
    //  type : {
    //    id: { type: "text" },
    //    nls: {
    //      type: "nls",
    //      options: {n: {}}
    //    }
    //  }
    //}
    published.ObjectEditor = function(init) {
      if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "ObjectEditor.ctor(init.DIV)";
      
      var editor = this;
      WAVE.extend(editor, WAVE.EventManager);

      fObjectEditorIdSeed++;
      var fSeed = "wvObjEditor_" + fObjectEditorIdSeed;

      var fDIV = init.DIV;
      var fMeta = WAVE.get(init, "meta", {});
      var fType = WAVE.strDefault(fMeta.type, "text");
      var fSchema = WAVE.get(fMeta, "schema", {});
      var fPlh = WAVE.strDefault(fMeta.plh);
      var fSets = WAVE.mergeArrays([], fMeta.sets, function(a, b) { return a.toLowerCase() === b.toLowerCase(); }, function(a) { return a.toLowerCase(); });
      var fDflt = fMeta.dflt;
      var fContent = WAVE.get(init, "content", typeof(fDflt) !== tUNDEFINED ? WAVE.clone(fDflt) : {});

      var fReadOnly = WAVE.strAsBool(init.readonly, false);
      var fDisabled = WAVE.strAsBool(init.disabled, false);
      var fCanRaw = WAVE.strAsBool(init.canRaw, false) && !fReadOnly && !fDisabled;
      var fCanDel = WAVE.strAsBool(init.canDel, true) && !fReadOnly && !fDisabled;
      var fCanAdd = WAVE.strAsBool(init.canAdd, true) && !fReadOnly && !fDisabled;
      var fMaxLength = WAVE.intValidPositive(fMeta.maxLength) ? fMeta.maxLength : 2;
      var fOutputFormFieldName = WAVE.strDefault(init.outputFormFieldName, "");

      var fCurrentLocal = WAVE.strDefault(init.local, "eng");
      var fLocalizerSchema = "ObjectEditor";
      var fLocalizerField = "objEd";
      var fValidators = [];

      var fControlsIds = {
        editorContainerId : "edit_cont_" + fSeed,
        inputId : "edit_input_" + fSeed,
        outputHiddenId: "edit_conf_out_" + fSeed,

        btnRawId: "edit_raw_" + fSeed,
        btnDelId: "edit_del_" + fSeed
      }; 

      var fTexts = WAVE.get(init, "texts", {});
        fTexts.addTitle = getText("add-title", "Add New Language");
        fTexts.addBody = getText("add-body", "");
        fTexts.rawTitle= getText("raw-title", "Raw Edit");
        fTexts.rawBody = getText("raw-body");
        fTexts.clearConfirmTitle = getText("clear-confirm-title", "Confirm Clear");
        fTexts.clearConfirmBody = getText("clear-confirm-body", "Clear all language data?");
        fTexts.delConfirmTitle = getText("del-confirm-title", "Confirm Delete");
        fTexts.resetConfirmTitle = getText("reset-confirm-title", "Confirm Reset");
        fTexts.delConfirmBody = getText("del-confirm-body");
        fTexts.resetConfirmBody = getText("reset-confirm-body");
        fTexts.btnDel = getText("btn-del", "Delete");
        fTexts.btnAdd = getText("btn-add", "Add");
        fTexts.btnRaw = getText("btn-raw", "Edit Raw");
        fTexts.btnClear = getText("btn-clear", "Clear");
        fTexts.btnReset = getText("btn-reset", "Reset");

      function getText(n, dflt){
        return localize(fTexts[n], dflt);
      }

      function localize(text, dflt){
        return WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, WAVE.strDefault(text, dflt));
      } 

      var fClasses = WAVE.get(init, "btnClasses", {});
        fClasses.addBtnCls = WAVE.strDefault(fClasses.addBtnCls, published.CLS_PS_BTN_ADD);
        fClasses.delBtnCls = WAVE.strDefault(fClasses.delBtnCls, published.CLS_PS_BTN_DEL);
        fClasses.rawBtnCls = WAVE.strDefault(fClasses.rawBtnCls, published.CLS_PS_BTN_RAW);
        fClasses.clearBtnCls = WAVE.strDefault(fClasses.clearBtnCls, published.CLS_PS_BTN_CLEAR);
        fClasses.modalBtnCls = WAVE.strDefault(fClasses.modalBtnCls, "");

      function setHiddenValue(){
         if (fOutputFormFieldName !== "") WAVE.id(fControlsIds.outputHiddenId).value = editor.content();
      }

      this.content = function(value, rebuild) {
        if (typeof (value) === tUNDEFINED) 
          return WAVE.empty(fContent) ? "" : JSON.stringify(fContent, null, 2);
                  
        var parsed = WAVE.tryParseJSON(value);
        if (parsed.ok) { 
          fContent = WAVE.cloneEnsure(parsed.obj);

          setHiddenValue();
          editor.eventInvoke(published.EVT_OBJECT_EDITOR_UPDATED, fContent);
          if(WAVE.strAsBool(rebuild, false)) {
            build();
          }
        }
        return value;
      };

      this.jsonContent = function() {
        return fContent;
      };

      this.maxLength = function() {
        return fMaxLength;
      };

      this.DIV = function() {
        return fDIV;
      };

      this.validate = function() {
        var vals = [];
        for(var f in fValidators) {
          vals.push(fValidators[f]());
        }
        return vals;
      };

      this.validators = function() {
        return fValidators;
      };

      function validateField(func, control ,value, idx) {
        var res = func(value, idx);
        if (res.ok) { 
          if (WAVE.isFunction(res.hideError)) {
            res.hideError(control);
          }
          editor.eventInvoke(published.EVT_OBJECT_EDITOR_VALIDATION_SUCCESS, { phase: published.EVT_PHASE_AFTER, target: control });
        } else {
          if (WAVE.isFunction(res.showError)) {
            res.showError(control);
          } else {
            published.toast(WAVE.strDefault(res.msg, "Error"), "error");
          }
          editor.eventInvoke(published.EVT_OBJECT_EDITOR_VALIDATION_ERROR, { phase: published.EVT_PHASE_AFTER, target: control });
        }
        return res;
      }

      function build() {
        fValidators = [];
        fDIV.innerText = "";
        function uName(filedName, id){
          return filedName + id;
        }

        if (fType === "array") {
          fDIV.insertAdjacentHTML("beforeend", "<div id='" + fControlsIds.editorContainerId + "'></div>");
          if(WAVE.isArray(fContent)){
            for(var i = 0; i < fContent.length; i++){
              (function(idx){
                var m = {};
                for(var mn in fMeta) {
                  m[mn] = fMeta[mn];
                }
                m.type= "object";
                for(var nm in m.schema){
                  m.schema[nm].index = idx;
                }
                var id = fControlsIds.editorContainerId + "_" + idx;
                var delButtonId = "del" + id;
                fDIV.insertAdjacentHTML("beforeend", "<div id='" + id + "'></div><input type='button' id=" + delButtonId + " class='" + fClasses.delBtnCls + "' value='" + fTexts.btnDel +"'/>");
                var ed = new published.ObjectEditor({
                  DIV: WAVE.id(id),
                  content: fContent[idx],  
                  meta: m,
                  disabled : fDisabled,
                  readonly : fReadOnly, 
                });
                ed.eventBind(WAVE.GUI.EVT_OBJECT_EDITOR_UPDATED, function (e, data) {
                  fContent[idx] = data;
                  editor.content(fContent);
                });
                ed.eventBind(WAVE.GUI.EVT_OBJECT_EDITOR_VALIDATION_ERROR,function(e, args) {
                  editor.eventInvoke(published.EVT_OBJECT_EDITOR_VALIDATION_ERROR, { phase: published.EVT_PHASE_AFTER, target: args.target });
                });  
                ed.eventBind(WAVE.GUI.EVT_OBJECT_EDITOR_VALIDATION_SUCCESS,function(e, args) {
                  editor.eventInvoke(published.EVT_OBJECT_EDITOR_VALIDATION_SUCCESS, { phase: published.EVT_PHASE_AFTER, target: args.target });
                });
                var vls = ed.validators();
                for(var vl in vls) {
                  fValidators.push(vls[vl]);
                }
                if (fCanDel && !fReadOnly && !fDisabled) {
                  WAVE.id(delButtonId).onclick = function () {
                    published.showConfirmationDialog(
                      fTexts.delConfirmTitle,
                      "<div>"+fTexts.delConfirmBody+"</div>",
                      [published.DLG_OK, published.DLG_CANCEL],
                      function (sender, result) {
                        if (result === published.DLG_OK) { 
                          var args = { phase: published.EVT_PHASE_BEFORE, editor: editor, index: idx, abort: false };
                          editor.eventInvoke(published.EVT_OBEJCT_EDITOR_ARRAY_REMOVE_ITEM, args);
                          if (args.abort === true) return;

                          fContent.splice(idx, 1);
                          editor.content(fContent, true);
                          editor.eventInvoke(published.EVT_OBEJCT_EDITOR_ARRAY_REMOVE_ITEM, { phase: published.EVT_PHASE_AFTER, editor: editor, index: idx });
                        }
                        return true;
                      },
                      { btnCls: fClasses.modalBtnCls });
                  };
                }
              })(i);
            }
          } 
          if(fCanAdd && !fReadOnly && !fDisabled && (!WAVE.isArray(fContent) || fContent.length < fMaxLength)){
            var buttonAddId = "add_" + fControlsIds.editorContainerId;
            fDIV.insertAdjacentHTML("beforeend", "<input type='button' value='" + fTexts.btnAdd + "' id='" + buttonAddId + "' class='" + fClasses.addBtnCls + "'/>");
            WAVE.id(buttonAddId).onclick = function () {
              if(!WAVE.isArray(fContent)) fContent = [];
              var args = { phase: published.EVT_PHASE_BEFORE, editor: editor, abort: false };
              editor.eventInvoke(published.EVT_OBEJCT_EDITOR_ARRAY_ADD_ITEM, args);
              if (args.abort === true) return;

              fContent.push({}); 
              editor.content(fContent, true);
              editor.eventInvoke(published.EVT_OBEJCT_EDITOR_ARRAY_ADD_ITEM, { phase: published.EVT_PHASE_AFTER, editor: editor });
              editor.validate();
            };
          }
        } else {
          for(var o in fSchema){
            (function(name){
              var meta = fSchema[name];
              var DIV = WAVE.get(meta , "DIV", fDIV);
              var dflt = meta.dflt;
              var content = WAVE.get(fContent, name, dflt);
              var type = WAVE.strDefault(meta.type, "text");
              var schema = WAVE.get(meta, "schema", {});
              var plh = WAVE.strDefault(meta.plh, "");
              var fIdx = WAVE.intValidPositiveOrZero(meta.index) ? meta.index : -1;
              var sets = WAVE.mergeArrays([], meta.sets, function(a, b) { return a.toLowerCase() === b.toLowerCase(); }, function(a) { return a.toLowerCase(); });
              var items = WAVE.isObject(meta.items) ? meta.items : {};
              var valid = WAVE.isFunction(meta.validate) ? meta.validate : function(v, inIndex) { return {ok: true, value: v, index: inIndex} };

              var disabled = WAVE.strAsBool(meta.disabled, fDisabled);
              var readonly = WAVE.strAsBool(meta.readonly, fReadOnly);

              var canRaw = WAVE.strAsBool(meta.canRaw, false);
              var canDel = WAVE.strAsBool(meta.canDel, true);
              var canAdd = WAVE.strAsBool(meta.canAdd, true);

              var divId = uName(name, fControlsIds.editorContainerId);
              var inputId = uName(name, fControlsIds.inputId);
              if(type === "textarea" || type === "object" || type === "ps" || type === "array"){
                DIV.insertAdjacentHTML("beforeend", "<div id='" + divId + "'></div>");
              } else if (type === "text") {
                DIV.insertAdjacentHTML(
                  "beforeend", 
                  "<div>" + 
                    "<label for=" + inputId + ">" + WAVE.strDefault(plh, key) + "</label>" +
                    "<input id='"+ inputId +"' " + (disabled ? "disabled" : "") + " " + (readonly ? "readonly" : "") + 
                    " type='text' placeholder='" + plh + "' title='" + plh + "' value='" + WAVE.strDefault(content) + "'/></div>"
                );
              } else if (type === "check") {
                DIV.insertAdjacentHTML(
                  "beforeend", 
                  "<div><label for='" + inputId + "'>" + plh + "</label><input id='"+ inputId +"' " + (disabled ? "disabled" : "") + " " + (readonly ? "disabled" : "") + " type='checkbox'/></div>"
                );
              } else if (type === "multiple") {
                var html = "<div>";
                for(var key in items){
                  html += "<label for=" + inputId + key + ">" + WAVE.strDefault(items[key], key) + "</label>";
                  html += "<input type='checkbox' " + ((WAVE.isArray(content) && WAVE.inArray(content, key)) ? "checked='checked'" : "") + 
                    " name='" + inputId + "' value='" + key + "' id='" + inputId + key + "' " + (disabled ? "disabled" : "") + " " + (readonly ? "disabled" : "") + "/>";
                }
                html+= "</div>";
                DIV.insertAdjacentHTML("beforeend", html);
              } else if (type === "radio") {
                var html = "<div>";
                for(var key in items){
                  html += "<label for=" + inputId + key + ">" + WAVE.strDefault(items[key], key) + "</label>";
                  html += "<input type='radio' " + (content === key ? "checked='checked'" : "") + 
                    " name='" + inputId + "' value='" + key + "' id='" + inputId + key + "' " + (disabled ? "disabled" : "") + " " + (readonly ? "disabled" : "") + "/>";
                }
                html+= "</div>";
                DIV.insertAdjacentHTML("beforeend", html);
              } else if (type === "combo") {
                var html = "<div><select id='" + inputId + "' " + (disabled ? "disabled" : "") + " " + (readonly ? "disabled" : "") + ">";
                for(var key in items){
                  html += "<option " + (content === key ? "selected" : "") + " value='" + key + "'>" + WAVE.strDefault(items[key], key) + "</option>";
                }
                html += "</select></div>";
                DIV.insertAdjacentHTML("beforeend", html);
              }

              if(type === "textarea"){
                  var ed = new published.MultiLineTextBox({
                    DIV : WAVE.id(divId),
                    id : inputId,
                    value : WAVE.strDefault(content),
                    disabled : disabled,
                    readonly : readonly,
                    placeholder : plh
                  });

                  var vFunc = function() {
                    var ctrl = WAVE.id(inputId);
                    var vl = ctrl.value;
                    return validateField(valid, ctrl, vl, fIdx);
                  };
                  fValidators.push(vFunc);

                  ed.eventBind(WAVE.GUI.EVT_MULTI_LINE_TEXT_UPDATED, function (e, args) {
                    fContent = WAVE.isObject(fContent) ? fContent : {};

                    var res = vFunc();
                    if (res.ok) { 
                      fContent[name] = String(res.value).replace(/'/g, '&apos;');
                      editor.content(fContent);
                    }
                  });
              } else if(type === "object") {
                  var ed = new published.ObjectEditor({
                    DIV: WAVE.id(divId),
                    meta: meta,
                    disabled : disabled,
                    readonly : readonly, 
                    content: content
                  });

                  ed.eventBind(WAVE.GUI.EVT_OBJECT_EDITOR_UPDATED, function (e, d) {
                    fContent = WAVE.isObject(fContent) ? fContent : {};
                    fContent[name] = d;
                    editor.content(fContent);
                  });
              } else if(type === "ps") {
                var ed = new published.PropSetEditor({
                  DIV: WAVE.id(divId),
                  sets: sets,
                  shema: schema,
                  content: content,
                  disabled : disabled,
                  readonly : readonly,
                  canRaw : canRaw,
                  canAdd : canAdd,
                  canDel : canDel
                });

                ed.eventBind(WAVE.GUI.EVT_PS_EDITOR_UPDATED, function (e, d) {
                  fContent = WAVE.isObject(fContent) ? fContent : {};
                  fContent[name] = d;
                  editor.content(fContent);
                });
              } else if(type === "check") {
                var nI = WAVE.id(inputId);
                nI.checked = WAVE.strAsBool(content, false);
                nI.onclick = function change(e) {
                  fContent = WAVE.isObject(fContent) ? fContent : {};
                  fContent[name] = e.target.checked;
                  editor.content(fContent);
                };
              } else if(type === "multiple") {
                var radios = document.getElementsByName(inputId);
                for(var r = 0; r < radios.length; r++) {
                  radios[r].onclick = function() {
                    fContent = WAVE.isObject(fContent) ? fContent : {};
                    fContent[name] = [];
                    for (var i = 0, length = radios.length; i < length; i++) {
                      if (radios[i].checked) {
                        fContent[name].push(radios[i].value);
                      }
                    }
                    editor.content(fContent);
                  };
                }
              } else if (type === "radio") {
                var radios = document.getElementsByName(inputId);
                for(var r = 0; r < radios.length; r++) {
                  radios[r].onclick = function() {
                    fContent = WAVE.isObject(fContent) ? fContent : {};
                    fContent[name] = [];
                    for (var i = 0, length = radios.length; i < length; i++) {
                      if (radios[i].checked) {
                        fContent[name] = radios[i].value;
                        break;
                      }
                    }
                    editor.content(fContent);
                  };
                }
              }
              else if(type === "combo") {
                WAVE.id(inputId).onchange = function change(e) {
                  fContent = WAVE.isObject(fContent) ? fContent : {};
                  fContent[name] = e.target.value;
                  editor.content(fContent);
                };
              } else if(type === "text") {
                var vFunc = function() {
                  var ctrl = WAVE.id(inputId);
                  var vl = ctrl.value;
                  return validateField(valid, ctrl, vl, fIdx);
                };
                fValidators.push(vFunc);

                WAVE.id(inputId).onchange = function change(e) {
                  fContent = WAVE.isObject(fContent) ? fContent : {};
                  var res = vFunc();
                  if (res.ok) { 
                    fContent[name] = String(res.value).replace(/'/g, '&apos;');
                    editor.content(fContent);
                  }
                };
              } else if (type === "array") {
                DIV.insertAdjacentHTML("beforeend", "<div id='" + divId + "'></div>");
                if(WAVE.isArray(content)){
                  for(var i = 0; i < content.length; i++){
                    (function(idx){
                      var m = WAVE.clone(meta);
                      m.type= "object";
                      var id = divId + "_" + idx;
                      var delButtonId = "del" + id;
                      DIV.insertAdjacentHTML("beforeend", "<div id='" + id + "'></div><input type='button' id=" + delButtonId + " value='" + fTexts.btnDel +"' class='" + fClasses.delBtnCls + "'/>");
                      var ed = new published.ObjectEditor({
                        DIV: WAVE.id(id),
                        content: content[idx],  
                        meta: m,
                        disabled : disabled,
                        readonly : readonly, 
                      });

                      ed.eventBind(WAVE.GUI.EVT_OBJECT_EDITOR_UPDATED, function (e, data) {
                        fContent[name][idx] = data;
                        editor.content(fContent);
                      });
                      if (canDel && !readonly && !disabled) {
                        WAVE.id(delButtonId).onclick = function () {
                          published.showConfirmationDialog(
                            fTexts.delConfirmTitle,
                            "<div>"+fTexts.delConfirmBody+"</div>",
                            [published.DLG_OK, published.DLG_CANCEL],
                            function (sender, result) {
                              if (result === published.DLG_OK) {
                                var args = { phase: published.EVT_PHASE_BEFORE, editor: editor, index: idx, abort: false };
                                editor.eventInvoke(published.EVT_OBEJCT_EDITOR_ARRAY_REMOVE_ITEM, args);
                                if (args.abort === true) return;

                                fContent[name].splice(idx, 1);
                                editor.content(fContent, true);
                                editor.eventInvoke(published.EVT_OBEJCT_EDITOR_ARRAY_REMOVE_ITEM, { phase: published.EVT_PHASE_AFTER, editor: editor, index: idx });
                              }
                              return true;
                            },
                            { btnCls: fClasses.modalBtnCls });
                        };
                      }
                    })(i);
                  }
                } 
                if(fCanAdd && !fReadOnly && !fDisabled && (!WAVE.isArray(fContent[name]) || fContent[name].length < fMaxLength)){
                  var buttonAddId = "add_" + fControlsIds.editorContainerId;
                  fDIV.insertAdjacentHTML("beforeend", "<input type='button' value='" + fTexts.btnAdd + "' id='" + buttonAddId + "'/>");
                  WAVE.id(buttonAddId).onclick = function () {
                    fContent = WAVE.isObject(fContent) ? fContent : {};
                    if(!WAVE.isArray(fContent[name])) fContent[name] = [];
                    var args = { phase: published.EVT_PHASE_BEFORE, editor: editor, abort: false };
                    editor.eventInvoke(published.EVT_OBEJCT_EDITOR_ARRAY_ADD_ITEM, args);
                    if (args.abort === true) return;

                    fContent[name].push({}); 
                    editor.content(fContent, true);
                    editor.eventInvoke(published.EVT_OBEJCT_EDITOR_ARRAY_ADD_ITEM, { phase: published.EVT_PHASE_AFTER, editor: editor });
                  };
                }
              }
            })(o);
          }
        }
        if (fCanRaw) {
          fDIV.insertAdjacentHTML("beforeend", "<input type='button' value='" + fTexts.btnRaw + "' id='" + fControlsIds.btnRawId + "'/>");
          WAVE.id(fControlsIds.btnRawId).onclick = function () {
            var inputId = "raw" + fSeed;
            published.showConfirmationDialog(
              fTexts.rawTitle,
              "<div>" + fTexts.rawBody + "</div>" +
              "<div><textarea style='width: 70vw; height: 70vh;' class='" + published.CLS_PS_TEXTAREA_RAW_EDITOR + "' type='text' id='" + inputId + "'></textarea></div>",
              [published.DLG_OK, published.DLG_CANCEL],
              function (sender, result) {
                if (result === published.DLG_OK) {
                  var val = WAVE.id(inputId).value;
                  var res = WAVE.tryParseJSON(val);
                  if (res.ok){
                      if (fType != 'array' && WAVE.checkKeysUnique(res.obj)){
                        published.toast(WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, "JSON is valid, but some keys are duplicated"), "error");
                        return false;
                      }
                      editor.content(val, true);
                    }
                  else {
                    published.toast(WAVE.strLocalize(fCurrentLocal, fLocalizerSchema, fLocalizerField, "Invalid JSON"), "error");
                    return false;
                  }
                }
                return true;
              },
              { btnCls: fClasses.modalBtnCls });
            WAVE.id(inputId).value = editor.content();
            WAVE.id(inputId).focus();
          };
        }
      }
      build();
    };//ObjectEditor


    var fTabsIdSeed = 0;
    //{
    //  "DIV" : DIV,
    //  "unswitchable" : false,
    //  "tabs" : [ 
    //    {
    //      "title" : "text",
    //      "content" : "text or hml",
    //      "isHtml" : "true",
    //      "isActive" : "false"
    //    }
    //  ]
    //}
    published.Tabs = function (init) {
      if (typeof (init) === tUNDEFINED || init === null || typeof (init.DIV) === tUNDEFINED || init.DIV === null) throw "Tabs.ctor(init.DIV)";

      var tabs = this;
      WAVE.extend(tabs, WAVE.EventManager);
      fTabsIdSeed++;
      var fSeed = "wvTabs_" + fTabsIdSeed;

      var fDIV = init.DIV;
      var fUnswitchable = WAVE.strAsBool(init.unswitchable, false);
      var fTabs = WAVE.isArray(init.tabs) ? init.tabs : [];
      var fTSeed = 0;
      for(var i = 0; i < fTabs.length; i++){
         fTabs[i] = ensureTab(fTabs[i]);
      }

      var fControlsIds = {
        divTabsContId : "tabs_cont_" + fSeed,
        divContentContId : "content_cont_" + fSeed,
        ulTabsId : "ul_tabs_ " + fSeed
      };
      var fTexts = {};
      var fClasses = {};

      function ensureTab(obj){
        if (!WAVE.isObject(obj)) obj = {};

        fTSeed++;
        obj.title = WAVE.strDefault(obj.title, fTSeed.toString());
        obj.content = WAVE.strDefault(obj.content);
        obj.isHtml = WAVE.strAsBool(obj.isHtml, true);
        obj.isActive = WAVE.strAsBool(obj.isActive, false);
        obj.id = fSeed + "_" + fTSeed;
        obj.name = WAVE.strDefault(obj.Name, fTSeed);
        return obj;
      }

      function buildTab(tab){
        var ulTabs = WAVE.id(fControlsIds.ulTabsId);
        var divTabs = WAVE.id(fControlsIds.divContentContId);

        var divId = getTabContentId(tab.id);
        ulTabs.insertAdjacentHTML("beforeend", "<li id='" + tab.id + "'></li>");
        divTabs.insertAdjacentHTML("beforeend", "<div id='" + divId + "' class='" + published.CLS_TABS_CONTENT_DIV + "'></div>");

        var t = WAVE.id(tab.id);
        var div = WAVE.id(divId);
        t.innerText = tab.title;
        t.onclick = function(){
          tabs.activeTab(tab.name);
        };

        if(tab.isHtml) 
          div.innerHTML = tab.content;
        else
          div.innerText = tab.content;
      }

      function getTabContentId(tabId){
        return "content_" + tabId;
      }

      function activate(name){
        var result = null;
        for(var i = 0; i < fTabs.length; i++){
          var tab = fTabs[i];
          var hTab = WAVE.id(tab.id);
          var div = WAVE.id(getTabContentId(tab.id));
          if(tab.name === name){
              tab.isActive = true;
              result = tab.name;
              div.style.display = "block";
              hTab.className += " " + published.CLS_TABS_ACTIVE_TAB;
          } else {
            tab.isActive = false;
            div.style.display = "none";
            WAVE.removeClass(hTab, published.CLS_TABS_ACTIVE_TAB);
          }
        }
        return result;
      }

      this.DIV = function() { return fDIV; };

      this.unswitchable = function(value) {
        if (typeof(value) === tUNDEFINED) return fUnswitchable;
        
        fUnswitchable = value === true;
        for(var i = 0; i < fTabs.length; i++){
          var tab = WAVE.id(fTabs[i].id);
          if(fUnswitchable){
            if(!fTabs[i].isActive)
              tab.className += " " + published.CLS_TABS_LI_DISABLED;
          }
          else 
            WAVE.removeClass(tab, published.CLS_TABS_LI_DISABLED);
        }
        return value;
      };

      this.addTab = function(tab){
        var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, tabControl: tabs, tab: tab, abort: false };
        tabs.eventInvoke(published.EVT_TABS_TAB_ADD, evtArgsBefore);
        if (evtArgsBefore.abort === true) return;

        tab = ensureTab(tab);
        fTabs.push(tab);
        buildTab(tab, true);
        activate(tab.name);
        tabs.eventInvoke(published.EVT_TABS_TAB_CHANGED, tabs.activeTab());

        tabs.eventInvoke(published.EVT_TABS_TAB_ADD, { phase: published.EVT_PHASE_AFTER, tabControl: tabs, tab: tab });

        return tab.name;
      };

      this.deleteTab = function(name){
        if(WAVE.strEmpty(name)) return false;

        var res = false;
        for(var i = 0; i < fTabs.length; i++){
          if(fTabs[i].name === name){
            var t = fTabs[i];

            var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, tabControl: tabs, tab: t, abort: false };
            tabs.eventInvoke(published.EVT_TABS_TAB_REMOVE, evtArgsBefore);
            if (evtArgsBefore.abort === true) return res;

            fTabs.splice(i, 1);
            if(fTabs.length > 0){
              tabs.activeTab(fTabs[0].name);
            }
            res = (WAVE.removeElem(t.id) && WAVE.removeElem(getTabContentId(t.id)));
          }
        }

        tabs.eventInvoke(published.EVT_TABS_TAB_REMOVE, { phase: published.EVT_PHASE_AFTER, tabControl: tabs, name: name, result: res });
        return res;
      };

      this.activeTab = function(name){
        var result = null;
        if(WAVE.strEmpty(name)) {
          for(var i = 0; i < fTabs.length; i++) {
            if(fTabs[i].isActive)
              result = fTabs[i].name;
          }
        } else {
          if(!fUnswitchable)
            if(tabs.activeTab() !== name) {
              result = activate(name);
              tabs.eventInvoke(published.EVT_TABS_TAB_CHANGED, tabs.activeTab());
            }
        }
        return result;
      };

      (function build() {
        var html = 
          "<div id='@divTabsId@' class='@ulContainer@'>" +
            "<ul id='@ulId@' class='@ulClass@'></ul>" +
          "</div>" +
          "<div id='@divContId@' class='@divClass@'></div>";

        fDIV.innerHTML = WAVE.strHTMLTemplate(html, 
        {
          divTabsId : fControlsIds.divTabsContId,
          divContId : fControlsIds.divContentContId,
          ulId : fControlsIds.ulTabsId,
          ulClass : published.CLS_TABS_UL,
          divClass : published.CLS_TABS_CONTENT_CONTAINER,
          ulContainer : published.CLS_TABS_UL_CONTAINER
        });

        if(fTabs.length === 0) return;

        var activeTab = null;
        var hit = false;
        for(var i = 0; i < fTabs.length; i++) {
          var tab = fTabs[i];
          if(tab.isActive){
            if(!hit){
              activeTab = tab.name;
              hit = true;
            } else {
              tab.isActive = false;
            }
          }
          buildTab(tab);
        }
        if(!hit){
          activeTab = fTabs[0].name;
          fTabs[0].isActive = true;
        }
        activate(activeTab);
      })();

      return tabs;
    };//Tabs

    return published;
}());//WAVE.GUI



WAVE.RecordModel.GUI = (function(){

    var tUNDEFINED = "undefined";

    var published = { 
        CLS_ERROR: 'wvError',
        CLS_REQ: 'wvRequired',
        CLS_MOD: 'wvModified',
        CLS_PUZZLE: 'wvPuzzle'
    };

        var AIKEY = "__wv.rm.gui";
        var fErroredInput = null;
        
        function buildTestControl(fldView){
          fldView.DIV().innerHTML = WAVE.strHTMLTemplate("<div>@about@</div><input type='text' name='@fName@' value='@fValue@'/>",
                                                        {about: fldView.field().about(), fName: fldView.field().name(), fValue: fldView.field().value()});
        }

        function genIDSeed(fldView){
          var id = WAVE.genAutoincKey(AIKEY);
          return "_"+fldView.recView().ID()+"_"+id;
        }

        
        function buildTextBox(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "tb"+ids;

          var html = WAVE.strHTMLTemplate("<label for='@tbid@' class='@cls@'>@about@</label>",
                                      {
                                        tbid: idInput, 
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          var itp;
          var fk = field.kind();
          if (field.password()) itp = "password";
          else if (fk===WAVE.RecordModel.KIND_SCREENNAME) itp = "text";
          else itp = fk;

          html+= WAVE.strHTMLTemplate("<input id='@id@' type='@tp@' name='@name@' @disabled@ @maxlength@ @readonly@ value='@value@' placeholder='@placeholder@' @required@>",
                                      {
                                        id: idInput,
                                        tp: itp,
                                        name: field.name(),
                                        disabled: field.isEnabled() ? "" : "disabled",
                                        maxlength: field.size() <= 0 ? "" : "maxlength="+field.size(),
                                        readonly: field.readonly() ? "readonly" : "",
                                        value: field.isNull()? "" : field.displayValue(),
                                        placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
                                        required: field.required() ? "required" : ""
                                      });

          divRoot.innerHTML = html;
          WAVE.id(idInput).__fieldView = fldView;
          $("#"+idInput).change(function(){
            var val = this.value;
            try
            {
                this.__fieldView.field().setGUIValue(val);
                fErroredInput = null;
            }
            catch(e)
            {
                WAVE.GUI.toast("Wrong value for field '"+this.__fieldView.field().about()+"'. Please re-enter or undo", "error");
                var self = this;
                if (fErroredInput===self || fErroredInput===null)
                {
                    fErroredInput = self;
                    setTimeout(function() 
                           {
                             if (!self.ERRORED) $(self).blur(function(){$(self).change();});
                             self.focus();
                             self.ERRORED = true;
                           }, 50);
                }
            }
          });
        }


        function buildTextArea(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "tb"+ids;

          var html = WAVE.strHTMLTemplate("<label for='@tbid@' class='@cls@'>@about@</label>",
                                      {
                                        tbid: idInput, 
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          html+= WAVE.strHTMLTemplate("<textarea id='@id@' name='@name@' @disabled@ @maxlength@ @readonly@ placeholder='@placeholder@' @required@>@value@</textarea>",
                                      {
                                        id: idInput,
                                        name: field.name(),
                                        disabled: field.isEnabled() ? "" : "disabled",
                                        maxlength: field.size() <= 0 ? "" : "maxlength="+field.size(),
                                        readonly: field.readonly() ? "readonly" : "",
                                        value: field.isNull()? "" : field.displayValue(),
                                        placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
                                        required: field.required() ? "required" : ""
                                      });

          divRoot.innerHTML = html;
          WAVE.id(idInput).__fieldView = fldView;
          $("#"+idInput).change(function(){
            var val = this.value;
            try
            {
                this.__fieldView.field().setGUIValue(val);
                fErroredInput = null;
            }
            catch(e)
            {
                WAVE.GUI.toast("Wrong value for field '"+this.__fieldView.field().about()+"'. Please re-enter or undo", "error");
                var self = this;
                if (fErroredInput===self || fErroredInput===null)
                {
                    fErroredInput = self;
                    setTimeout(function() 
                           {
                             if (!self.ERRORED) $(self).blur(function(){$(self).change();});
                             self.focus();
                             self.ERRORED = true;
                           }, 50);
                }
            }
          });
        }


        function buildRadioGroup(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var dict = field.lookupDict();
          var keys = Object.keys(dict);
          
          var html = "<fieldset>";
          html+= WAVE.strHTMLTemplate("<legend class='@cls@'>@about@</legend>",{about: field.about(), 
                                                                                cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                                                               });

          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          var ids = genIDSeed(fldView);

          for(var i in Object.keys(dict)){
             var key = keys[i];
             var keyDescr = dict[key];

              var idInput = "rbt"+ids+"_"+i;
         
              html+= WAVE.strHTMLTemplate("<input id='@id@' type='radio' name='@name@' @disabled@ @readonly@ value='@value@' @required@ @checked@>",
                                          {
                                            id: idInput,
                                            name: field.name(),
                                            disabled: field.isEnabled() ? "" : "disabled",
                                            readonly: field.readonly() ? "readonly" : "",
                                            value: key,
                                            required: field.required() ? "required" : "",
                                            checked: WAVE.strSame(key, field.value()) ? "checked" : ""
                                          });

              html+= WAVE.strHTMLTemplate("<label for='@rbtid@'>@about@</label>",
                                          {
                                            rbtid: idInput, 
                                            about: keyDescr  
                                          });


              
          }//for

          html+= "</fieldset>";

          divRoot.innerHTML = html;
           
          //bind events
          function rbtChange(evt){
            var val = evt.target.value;
            var fld = evt.target.__fieldView.field();
            if (!fld.readonly()) 
                fld.value(val, true);//from GUI
            else
                rebuildControl(evt.target.__fieldView);
          }

          for(var j in Object.keys(dict)){ 
           var idInp = "rbt"+ids+"_"+j;
           WAVE.id(idInp).__fieldView = fldView;

           $("#"+idInp).change(rbtChange);
          }
        }

        function buildCheck(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "chk"+ids;

          var html = "";
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});


          html+= WAVE.strHTMLTemplate("<input id='h_@id@' type='hidden'  value='@val@' name='@name@'>",
                                      {
                                        id: idInput,
                                        name: field.name(),
                                        val: WAVE.strAsBool(field.value()) ? "true" : "false"
                                      });


          html+= WAVE.strHTMLTemplate("<label for='@chkid@' class='@cls@'>@about@</label>",
                                      {
                                        chkid: idInput, 
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });
          html+= WAVE.strHTMLTemplate("<input id='@id@' type='checkbox' @disabled@ @readonly@  @required@ @checked@>",
                                      {
                                        id: idInput,
                                        disabled: field.isEnabled() ? "" : "disabled",
                                        readonly: field.readonly() ? "readonly" : "",
                                        required: field.required() ? "required" : "",
                                        checked: WAVE.strAsBool(field.value()) ? "checked" : ""
                                      });

          divRoot.innerHTML = html;
          WAVE.id(idInput).__fieldView = fldView;
          $("#"+idInput).change(function(){
               var val = this.checked;
               $("#h_"+idInput).val( val ? "true" : "false");
               var fld = this.__fieldView.field();
               if (!fld.readonly()) 
                 fld.value(val, true);//from GUI
               else
                 rebuildControl(this.__fieldView);
          });
        }

        function buildPSEditor(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var genIdKey = "@#$PS_EDITOR_GEN_ID$#@";
          var ids = WAVE.get(fldView, genIdKey, null);
          if (ids === null) {
            ids = genIDSeed(fldView);
            fldView[genIdKey] = ids;
          }
          var idPSDiv = "divps_"+ids;
          var idLabelDiv =  "labelCont_"+ids;

          var labelCont = WAVE.id(idLabelDiv);
          var editorCont = WAVE.id(idPSDiv);
          if (labelCont === null || editorCont === null){
             divRoot.innerHTML = WAVE.strHTMLTemplate("<div id='@idLabelDiv@'></div><div id='@idPSDiv@'></div>", {idLabelDiv:idLabelDiv, idPSDiv:idPSDiv});
             labelCont = WAVE.id(idLabelDiv);
             editorCont = WAVE.id(idPSDiv);
          }

          var html = "";
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});
          html+= WAVE.strHTMLTemplate("<label class='@cls@'>@about@</label>",
                                      {
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });

          labelCont.innerHTML = html;

          var json = field.isNull()? "" : field.value();
          var ekey = "@#$PS_EDITOR$#@";
          var editor = WAVE.get(fldView, ekey, null);
          if (editor===null)
          {
               editor =  new WAVE.GUI.PropSetEditor({ 
                                    DIV: editorCont,
                                    outputFormFieldName: field.name(),
                                    langs: WAVE.LOCALIZER.allLanguageISOs(), 
                                    content: json,
                                    disable: field.isEnabled() ? false : true,
                                    readonly: field.readonly(),
                                    title: field.about()
                                });

               fldView[ekey] = editor;
             
               editor.eventBind(WAVE.GUI.EVT_PS_EDITOR_UPDATED, 
                               function (e, d){
                                 field.value(d, true);//from GUI
                               });
         }
        }

        function buildComboBox(fldView){
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "cbo"+ids;

          var html = WAVE.strHTMLTemplate("<label for='@cbid@' class='@cls@'>@about@</label>",
                                      {
                                        cbid: idInput, 
                                        about: field.about(),
                                        cls: (field.required() ? published.CLS_REQ : "") +" "+ (field.isGUIModified() ? published.CLS_MOD : "")
                                      });

          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          html+= WAVE.strHTMLTemplate("<select id='@id@' name='@name@' @disabled@ @readonly@ value='@value@' placeholder='@placeholder@' @required@>",
                                      {
                                        id: idInput,
                                        name: field.name(),
                                        disabled: field.isEnabled() ? "" : "disabled",
                                        readonly: field.readonly() ? "readonly" : "",
                                        value: field.value(),
                                        placeholder: WAVE.strEmpty(field.placeholder()) ? "" : field.placeholder(),
                                        required: field.required() ? "required" : ""
                                      });
          
          var dict = field.lookupDict();
          var keys = Object.keys(dict);
          html += "<option value=''></option>";//add Blank
          for(var i in Object.keys(dict)){
              html+= WAVE.strHTMLTemplate("<option value='@value@' @selected@>@descr@</option>",
                                          {
                                            value: keys[i],
                                            selected: WAVE.strSame(keys[i], field.value()) ? "selected" : "",
                                            descr: dict[keys[i]]
                                          });
          }//for options

          html+="</select>";
          divRoot.innerHTML = html;
          WAVE.id(idInput).__fieldView = fldView;
          $("#"+idInput).change(function(){
            var val = this.value;
           this.__fieldView.field().value(val, true);//from GUI
          });
        }

        function buildPuzzle(fldView){
          
          if (fldView.PUZZLE) return;
       
          var field = fldView.field();
          var divRoot = fldView.DIV();

          var ids = genIDSeed(fldView);
          var idInput = "divPuzzle"+ids;
          var idHiddenInput = "hiddenPuzzle"+ids;

          var html = "";
          var ve = field.validationError();
          if (ve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: ve});

          html+= WAVE.strTemplate("<div id='@id@' class='@cls@'></div>"+
                                  "<input id='@idh@' type='hidden' name=@fname@ value=''></input>",
                                      {
                                        id: idInput,
                                        idh: idHiddenInput,
                                        fname: field.name(),
                                        cls: published.CLS_PUZZLE
                                      });

          divRoot.innerHTML = html;

          var fv = field.value();
          var pk = new WAVE.GUI.PuzzleKeypad(
                   {
                     DIV: WAVE.id(idInput),
                     Image: WAVE.strDefault(fv.Image, ""),
                     Help:  WAVE.strDefault(fv.Help, ""),
                     Question: WAVE.strDefault(fv.Question, ""),
                   });

          var hidden = WAVE.id(idHiddenInput);

          pk.eventBind(WAVE.GUI.EVT_PUZZLE_KEYPAD_CHANGE, function(kpad){
                          field.value().Answer = kpad.value();
                          hidden.value = JSON.stringify(field.value());
                       });

          fldView.PUZZLE = pk;
        }


        function buildHidden(fldView){
          fldView.DIV().innerHTML = WAVE.strHTMLTemplate("<input type='hidden' name='@fName@' value='@fValue@'>",
                                                        {fName: fldView.field().name(), fValue: fldView.field().displayValue()});
        }


        function buildErrorRec(fldView, summary){
          var record = fldView.record();
          var divRoot = fldView.DIV();


          var html = "";
          if (summary)
          {
            var allErrors = record.allValidationErrors();
            for(var i in allErrors)
            {
               var err = allErrors[i];
               if (err!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: err});
            }
            
          }
          else
          {
            var rve = record.validationError();
            if (rve!==null) html+=WAVE.strHTMLTemplate("<div class='@ec@'>@error@</div>", {ec: published.CLS_ERROR, error: rve});
          }

          divRoot.innerHTML = html;
        }


        function rebuildControl(fldView){
          var ct = published.getControlType(fldView);

          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_CHECK))    { ensureField(fldView, ct); buildCheck(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_RADIO))    { ensureField(fldView, ct); buildRadioGroup(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_NLS))      { ensureField(fldView, ct); buildPSEditor(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_COMBO))    { ensureField(fldView, ct); buildComboBox(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_PUZZLE))   { ensureField(fldView, ct); buildPuzzle(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_TEXTAREA)) { ensureField(fldView, ct); buildTextArea(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_HIDDEN))   { ensureField(fldView, ct); buildHidden(fldView);}
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_ERROR_REC)) buildErrorRec(fldView, false);
          else
          if (WAVE.strSame(ct, WAVE.RecordModel.CTL_TP_ERROR_SUMMARY)) buildErrorRec(fldView, true);
          else
          {
            ensureField(fldView, ct);
            buildTextBox(fldView);
          }
        }

        function ensureField(fldView, ct) {
          if (fldView.field()===null)
           throw "The control type '"+ct+"' requires the field binding, but was bound to record";
        }



    //Gets the appropriate(for this GUI lib) control type if the one is specified in field schema, or infers one from field definition
    published.getControlType = function(fldView){
      return fldView.getOrInferControlType();
    };

    //Builds control from scratch
    published.buildFieldViewAnew = function(fldView){
     rebuildControl(fldView);
    };

    //Updates control in response to underlying bound field change
    published.eventNotifyFieldView = function(fldView, evtName, sender, phase){
      rebuildControl(fldView);
    };

    return published;
}());//WAVE.RecordModel.GUI