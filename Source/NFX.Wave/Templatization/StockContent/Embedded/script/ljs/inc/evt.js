var any_event = "*";

var eventListName = "@!WAVE EVENT FUN LIST";
var eventSinkListName = "@!WAVE EVENT SINK LIST";

function deleteEventList(obj) {
  obj[eventListName] = {};
}

function deleteSinkList(obj) {
  obj[eventSinkListName] = [];
}

function ensureEventList(obj, evtName) {
  if (!(eventListName in obj)) obj[eventListName] = {};
  var el = obj[eventListName];
  if (!(evtName in el)) el[evtName] = [];
  return el[evtName];
}

function ensureSinkList(obj) {
  if (!(eventSinkListName in obj)) obj[eventSinkListName] = [];
  return obj[eventSinkListName];
}

//Event Manager Mixin - keeps track of event subscriptions and invocations
published.EventManager = {

  ANY_EVENT: any_event,

  //Binds a function to the named event handler
  eventBind: function(evtName, fun){
    if (published.strEmpty(evtName) || !fun) return false;

    var el = ensureEventList(this, evtName);
    if (published.inArray(el, fun)) return false;
    el.push(fun);
    return true;
  },

  //Un-Binds a function from the named event handler
  eventUnbind: function(evtName, fun){
    if (published.strEmpty(evtName)|| !fun) return false;

    var el = ensureEventList(this, evtName);
    return published.arrayDelete(el, fun);
  },

  //Clears all functions from the named event handler
  eventClear: function(evtName){
    if (published.strEmpty(evtName))
    {
      deleteEventList(this);
      return true;
    }

    var el = ensureEventList(this, evtName);
    published.arrayClear(el);

    return true;
  },

  //Increase to disable event firing for all events, decrease to enable, events are enabled again when value is <=0
  //This property is usefull for batch updates to suppress many event firings that are not needed
  eventInvocationSuspendCount: 0,

  //Invokes all functions bound to the named event handler
  eventInvoke: function(evtName){
    if (published.strEmpty(evtName)) return false;

    if (this.eventInvocationSuspendCount!==0) return false;

    //variadic params, remove 'evtName'
    var params = Array.prototype.slice.call(arguments, 1);

    //insert 'sender'
    params.splice(0,0,this);

    var el = ensureEventList(this, evtName);

    var i;
    //call all events for the named event
    for(i in el)
      el[i].apply(this, params);

    //insert 'evtName' for any event
    params.splice(0,0,evtName);

    el = ensureEventList(this, any_event);
    //call all "ANY" events for the named event
    for(i in el)
      el[i].apply(this, params);

    var sl = ensureSinkList(this);
    //call all "ANY" events for the named event
    for(i in sl){
      var sink = sl[i];
      var fun = sink.eventNotify;
      if (WAVE.isFunction(fun))
      fun.apply(sink, params);
    }

    return true;
  },

  //Binds a sink instance (an object) that will receive all events dispatched by this manager.
  //The sink must have a function called "eventNotify(evtName, sender,...args)" that will ge invoked
  eventSinkBind: function(sink){
    if (sink===null) return false;

    var sl = ensureSinkList(this);
    if (published.inArray(sl, sink)) return false;
    sl.push(sink);
    return true;
  },

  //Un-Binds an object that received all events from this manager
  eventSinkUnbind: function(sink){
    if (sink===null) return false;

    var sl = ensureSinkList(this);
    return published.arrayDelete(sl, sink);
  },

  //Clears all objects that act as event sinks bound to this instance
  eventSinkClear: function(){
    deleteSinkList(this);
  },

  //Returns a list of sink object that receive event notifications from this manager
  eventSinks: function(){
    return ensureSinkList(this);
  }
};
