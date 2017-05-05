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
        CLS_DIALOG_CONTENT: "wvDialogContent",
        CLS_DIALOG_HEADER: "wvDialogHeader",
        CLS_DIALOG_BODY: "wvDialogBody",
        CLS_DIALOG_FOOTER: "wvDialogFooter",
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
        CLS_TABS_CONTENT_DIV_HIDDEN: "wvTabsContentDivHidden",
        CLS_TABS_CONTENT_DIV_SHOWN: "wvTabsContentDivShown",
        CLS_TABS_CONTENT_CONTAINER: "wvTabsContentContainer",
        CLS_TABS_UL_CONTAINER: "wvTabsUlContainer",
        CLS_TABS_LI_DISABLED: "wvTabDisabled",

        EVT_TABS_TAB_CHANGED: "wv-active-tab-changed",
        EVT_TABS_TAB_ADD: "wv-tab-add",
        EVT_TABS_TAB_REMOVE: "wv-tab-remove",

        //Multiline
        EVT_MULTI_LINE_TEXT_UPDATED: "wv-multiline-updated",

        //ObjectEditor
        CLS_OBJECT_EDITOR_NODE_CONTAIER: "wvObjEditNode",
        CLS_OBJECT_EDITOR_ARRAY_ITEM: "wvObjEditItem",
        CLS_OBJECT_EDITOR_ARRAY_ITEM_SEPARATOR: "wvObjEditItemSeparator",
        CLS_OBJECT_EDITOR_ARRAY_CONTAIER: "wvObjEditArrDiv",
        CLS_OBJECT_EDITOR_OBJECT_CONTAIER: "wvObjEditObjDiv",
        CLS_OBJECT_EDITOR_INPUT_CONTAIER: "wvObjEditInputDiv",

        EVT_OBJECT_EDITOR_UPDATED: "wv-objectEditor-updated",
        EVT_OBJECT_EDITOR_VALIDATION_ERROR: "wv-objectEditor-validation-error",
        EVT_OBJECT_EDITOR_VALIDATION_SUCCESS: "wv-objectEditor-validation-success",
        //

        //Gallery {
        CLS_IMAGE_BOX_LOADING: "wvImageBox_loading",
        CLS_IMAGE_BOX_ERROR:   "wvImageBox_error",
        CLS_IMAGE_BOX_WIDER:   "wvImageBox_wider",
        CLS_IMAGE_BOX_HIGHER:  "wvImageBox_higher",
        CLS_IMAGE_VIEW:        "wvImageView",
        CLS_IMAGE_VIEW_ZOOM:   "wvImageView_zoom",
        CLS_IMAGE_VIEW_IMAGE:  "wvImageView_image",
        CLS_GALLERY:           "wvGallery",
        CLS_GALLERY_PREV:      "wvGallery_prev",
        CLS_GALLERY_NEXT:      "wvGallery_next",
        CLS_GALLERY_DISABLED:  "wvGallery_disabled",
        CLS_GALLERY_LIST:      "wvGallery_list",
        CLS_GALLERY_ITEM:      "wvGallery_item",
        CLS_GALLERY_THUMBNAIL: "wvGallery_thumbnail",
        CLS_GALLERY_CONTENT:   "wvGallery_content",
        CLS_GALLERY_IMAGE:     "wvGallery_image",
        CLS_GALLERY_SELECTED:  "wvGallery_selected",
        CLS_GALLERY_SIMPLE_THUMBNAILS: "wvGallerySimple_thumbnails",
        CLS_GALLERY_SIMPLE_PREVIEW:    "wvGallerySimple_preview",

        EVT_GALLERY_ADD:       "wv-gallery-add",
        EVT_GALLERY_REMOVE:    "wv-gallery-remove",
        EVT_GALLERY_CHANGE:    "wv-gallery-change",
        //} Gallery

        //Details control
        CLS_DETAILS: "wvDetails",
        CLS_DETAILS_TITLE: "wvDetailsTitle",
        CLS_DETAILS_TITLE_SHOWN: "wvDetailsTitleShown",
        CLS_DETAILS_TITLE_HIDDEN: "wvDetailsTitleHidden",
        CLS_DETAILS_CONTENT: "wvDetailsContent",
        CLS_DETAILS_CONTENT_VISIBLE: "wvDetailsContentVisible",
        CLS_DETAILS_CONTENT_HIDDEN: "wvDetailsContentHidden",
        CLS_DETAILS_MODAL: "wvDetailsModal",

        //ChainSelector control
        CLS_CHAIN_SELECTOR: "wvChainSelector",
        CLS_CS_DIV_INFO: "wvChainSelectorInfo",
        CLS_CS_DIV_NAME: "wvChainSelectorName",
        CLS_CS_DIV_DESCR: "wvChainSelectorDescription",
        CLS_CS_DIV_COMBO_WRAP: "wvChainSelectorComboWrap",
        CLS_CS_DIV_ARROW: "wvChainSelectorComboArrow",

        EVT_CHAIN_SELECTOR_UPDATED: "chain-selector-updated",

        EVT_DETAILS_SHOW: "wv-details-show",
        EVT_DETAILS_HIDE: "wv-details-hide",

        CLS_SELECT_MODE_MULTI: "wvMulti",
        CLS_SELECT_MODE_SINGLE: "wvSingle",
        CLS_SELECT_CONTAINER: "wvSelectContainer",
        CLS_SELECT_COMBOBOX: "wvSelectCombobox",
        CLS_SELECT_DROPDOWN: "wvSelectDropdown",
        CLS_SELECT_DROPDOWN_SHOWN: "wvShown",
        CLS_SELECT_DROPDOWN_HIDDEN: "wvHidden",
        CLS_SELECT_DISPLAY: "wvSelectDisplay",
        CLS_SELECT_ARROW: "wvSelectArrow",
        CLS_SELECT_AUTOCOMPETE: "wvSelectAutocomplete",
        CLS_SELECT_AUTO_CONTAINER: "wvSelectAutoContainer",
        CLS_SELECT_CHOICES_CONTAINER: "wvSelectChoicesContainer",

        EVT_SELECT_CHANGE: "wv-select-change",
        EVT_SELECT_OPEN: "wv-select-opan",
        EVT_SELECT_CLOSE: "wv-select-close",
        EVT_SELECT_SELECT: "wv-select-select",
        EVT_SELECT_UNSELECT: "wv-select-unselect",
        SELECT_MODES: { "Single": 0, "Multi": 1 }
    };

    var CURTAIN_ZORDER = 500000;
    var fCurtains = [];

    var fDirty = false;

    //gets/sets dirty flag
    published.dirty = function (val) {
      if (typeof (val) === tUNDEFINED) return fDirty;
      fDirty = val;
    };

    //Returns true if dirty flag is set or dialog shown
    published.isDirty = function () {
      return fDirty || published.currentDialog() !== null;
    };

    //Set to true to bypass check on page unload
    published.SUPPRESS_UNLOAD_CHECK = false;

    window.onbeforeunload = function (e) {
      if (!published.SUPPRESS_UNLOAD_CHECK && (published.isDirty() || WAVE.RecordModel.isDirty()))
        (e || window.event).returnValue = "The page is in the middle of the data entry. If you navigate away/close the page now some changes will be lost. Are you sure?";
    };

    var fBodyScrollTop = 0,
        fBodyPosition = null,
        fBodyOverflowY = null,
        fBodyTop = null,
        fBodyPaddingRight = null,
        fScrollFixed = false;

    function preventFocusLeak(e) {
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
    }

    function tryMakeBodyUnscrollable() {
      if (!fScrollFixed && !WAVE.Platform.Mobile) {
        window.addEventListener("focus", preventFocusLeak, true);

        fBodyScrollTop = window.pageYOffset || document.documentElement.scrollTop;
        fBodyTop = document.body.style.top;
        fBodyPosition = document.body.style.position;
        fBodyOverflowY = document.body.style.overflowY;
        fBodyPaddingRight = document.body.style.paddingRight;

        if (document.body.parentElement.offsetHeight > window.innerHeight) {
          fScrollFixed = true;

          document.body.style.position = "fixed";
          document.body.style.paddingRight = ((fBodyPaddingRight + WAVE.getScrollBarWidth()) + "px");
          document.body.style.overflow = "hidden";
          document.body.style.top = (-(fBodyScrollTop) + "px");
        }
      }
    }

    function tryRetrunScrollingToBody() {
      if (fScrollFixed && !WAVE.Platform.Mobile) {
        window.removeEventListener("focus", preventFocusLeak, true);

        document.body.style.top = fBodyTop;
        document.body.style.position = fBodyPosition;
        document.body.style.overflowY = fBodyOverflowY;
        document.body.style.paddingRight = fBodyPaddingRight;
        document.documentElement.scrollTop = document.body.scrollTop = fBodyScrollTop;
        fScrollFixed = false;
      }
    }

// inc/ui/toast.js
    /***@ inc/ui/toast.js ***/

 // inc/ui/curtain.js
    /***@ inc/ui/curtain.js ***/

// inc/ui/dialog.js
    /***@ inc/ui/dialog.js ***/

// inc/ui/pzlk.js
    /***@ inc/ui/pzlk.js ***/

// inc/ui/ruleman.js
    /***@ inc/ui/ruleman.js ***/

// inc/ui/wincon.js
    /***@ inc/ui/wincon.js ***/

// inc/ui/tree.js
    /***@ inc/ui/tree.js ***/

// inc/ui/mtb.js
    /***@ inc/ui/mtb.js ***/

// inc/ui/pseditor.js
    /***@ inc/ui/pseditor.js ***/

// inc/ui/oeditor.js
    /***@ inc/ui/oeditor.js ***/

// inc/ui/tabs.js
    /***@ inc/ui/tabs.js ***/

// inc/ui/details.js
    /***@ inc/ui/details.js ***/

// inc/ui/gallery.js
    /***@ inc/ui/gallery.js ***/

// inc/ui/chain.js
    /***@ inc/ui/chain.js ***/

// inc/ui/select.js
    /***@ inc/ui/select.js ***/

    return published;
}());//WAVE.GUI

/***@ wv.rmgui.ntc.js ***/