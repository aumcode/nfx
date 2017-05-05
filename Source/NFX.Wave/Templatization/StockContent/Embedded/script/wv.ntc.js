"use strict";
/*jshint devel: true,browser: true, sub: true */
/*global escape: true */

/*!
 * Wave Java Script Library Core v2.0.0
 *
 * Based on IT Adapter JS Library 2002-2011
 * License: Unrestricted use/modification with mandatory reference to IT Adapter Corp. as the original author
 * (c) 2002-2011, 2013-2014 IT Adapter Corp.
 * http://www.itadapter.com/
 * Authors: Dmitriy Khmaladze,
 *          Timur Shemsedinov
 * Revision Epoch:  May 1, 2014; Jan 8, 2011; 2007-2009; Mar-Apr 2006; Jan 2005...
 */
var WAVE = (function(){

    var tUNDEFINED = "undefined";

    if (!Date.now) {
      Date.now = function() { return new Date().getTime(); };
    }

    var published = {
      TUNDEFINED: tUNDEFINED,
      CONTENT_TYPE_JSON: "application/json",
      CONTENT_TYPE_JSON_UTF8: "application/json;charset=utf-8",
      CONTENT_TYPE_MULTIPART: "multipart/form-data"
    };

    published.falseness = function() { return false; };
    published.falsefunc = function() { return false; };

//inc/array.js
    /***@ inc/array.js ***/

//inc/type.js
    /***@ inc/type.js ***/

//inc/str.js
    /***@ inc/str.js ***/

//inc/markup.js
    /***@ inc/markup.js ***/

//inc/dom.js
    /***@ inc/dom.js ***/

//inc/localizer.js
    /***@ inc/localizer.js ***/

//inc/web.js
    /***@ inc/web.js ***/

//inc/evt.js
    /***@ inc/evt.js ***/

//inc/utest.js
    /***@ inc/utest.js ***/

//inc/geometry.js
    /***@ inc/geometry.js ***/

//inc/walkable.js
    /***@ inc/walkable.js ***/

//inc/svg.js
    /***@ inc/svg.js ***/

//inc/rndk.js
    /***@ inc/rndk.js ***/

    return published;
}());//WAVE


WAVE.RecordModel = (function(){

    var tUNDEFINED = "undefined";

    var published = {

        FIELDDEF_DEFAULTS: {
            Description: '',
            Placeholder: '',
            Type:        'string',
            Key:         false,
            Kind:        'text',
            Case:        'asis',
            Stored:     true,
            Required:   false,
            Applicable: true,
            Enabled:    true,
            ReadOnly:   false,
            Visible:    true,
            Password:   false,
            MinValue:   null,
            MaxValue:   null,
            MinSize:    0,
            Size:       0,
            ControlType:  'auto',
            ScriptType: null,
            DefaultValue: null,
            Hint:       null,
            Marked:     false,
            LookupDict: {}, //{key:"description",...}]
            Lookup:     {}, //complex lookup
            DeferValidation: false,
            Config:     null
        },
        EVT_VALIDATION_DEFINITION_CHANGE: "validation-definition-change",
        EVT_INTERACTION_CHANGE: "interaction-change",
        EVT_DATA_CHANGE: "data-change",

        EVT_RECORD_LOAD: "record-load",
        EVT_VALIDATE:    "validate",
        EVT_VALIDATED:   "validated",
        EVT_FIELD_LOAD:  "field-load",
        EVT_FIELD_RESET: "field-reset",
        EVT_FIELD_DROP:  "field-drop",

        EVT_PHASE_BEFORE: "before",
        EVT_PHASE_AFTER:  "after",

        CTL_TP_AUTO:    "auto",
        CTL_TP_CHECK:   "check",
        CTL_TP_RADIO:   "radio",
        CTL_TP_COMBO:   "combo",
        CTL_TP_TEXT:    "text",
        CTL_TP_NLS:     "nls",
        CTL_TP_SCRIPT:  "script",
        CTL_TP_TEXTAREA:"textarea",
        CTL_TP_HIDDEN:  "hidden",
        CTL_TP_PUZZLE:  "puzzle",
        CTL_TP_ERROR_REC:   "error-rec",
        CTL_TP_ERROR_SUMMARY:  "error-summary",
        CTL_TP_CHAIN_SELECTOR: "chain-selector",

        KIND_TEXT:  'text',
        KIND_SCREENNAME:  'screenname',
        KIND_COLOR: 'color',
        KIND_DATE:  'date',
        KIND_DATETIME: 'datetime',
        KIND_DATETIMELOCAL: 'datetime-local',
        KIND_EMAIL: 'email',
        KIND_MONTH: 'month',
        KIND_NUMBER: 'number',
        KIND_RANGE:  'range',
        KIND_SEARCH: 'search',
        KIND_TEL:    'tel',
        KIND_TIME:   'time',
        KIND_URL:    'url',
        KIND_WEEK:   'week',

        CASE_ASIS:  'asis',
        CASE_UPPER: 'upper',
        CASE_LOWER: 'lower',
        CASE_CAPS:  'caps',
        CASE_CAPSNORM: 'capsnorm',

        DATA_RECVIEW_ID_ATTR: 'data-wv-rid',
        DATA_FIELD_NAME_ATTR: 'data-wv-fname',
        DATA_CTL_TP_ATTR: 'data-wv-ctl'
    };

    var fRecords = [];

    //Returns the copy of list of record instances
    published.records = function(){
        return WAVE.arrayShallowCopy(fRecords);
    };

    //Returns true when there is at least one record instance with user-made modifications
    published.isDirty = function(){
        for(var i in fRecords)
          if (fRecords[i].isGUIModified()) return true;
        return false;
    };

    //inc/rm/record.js
    /***@ inc/rm/record.js ***/

    //inc/rm/recordview.js
    /***@ inc/rm/recordview.js ***/

    return published;
}());//WAVE.RecordModel


WAVE.Pay = (function () {
  var tUNDEFINED = "undefined";
  var published = { Providers: { } };

  /***@ inc/pay.js ***/

  return published;
}()); // WAVE.Pay
