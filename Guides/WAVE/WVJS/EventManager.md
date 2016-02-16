# WAVE.EventManager
Mixin that implements the event-handler mechanism and can be added to any class.

##### eventInvocationSuspendCount: 0
Increase to disable event firing for all events, decrease to enable, events are enabled again when value is <=0. This property is useful for batch updates to suppress many event firings that are not needed.

##### eventBind(evtName, func)
Binds a function to the named event handler.

##### eventUnbind(evtName, func)
Un-Binds a function from the named event handler.

##### eventClear(evtName)
Clears all functions from the named event handler.

##### eventInvoke(evtName)
Invokes all functions bound to the named event handler.

##### eventSinkBind(sink)
Binds a sink instance (an object) that will receive all events dispatched by this manager. The `sink` must have a function called `eventNotify(evtName, sender, args)` that will be invoked.
    
##### eventSinkUnbind(sink)
Un-Binds an object that received all events from this manager.

##### eventSinkClear()
Clears all objects that cat as event sinks bound to this instance.

##### eventSinks()
Returns a list of sink object that receive event notifications from this manager.
