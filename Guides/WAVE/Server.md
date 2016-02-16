# NFX.Wave Server

WAVE is a "WebAppViewEnhanced" web server which provides DYNAMIC web site services. This server is not meant to be exposed directly to the public Internet, rather it should be used as an application server behind the reverse proxy, such as NGINX. This server is designed to serve dynamic data-driven requests/APIs and not meant to be used for serving static content files (although it can).

### Pipeline
Every request goes through a pipeline where the server checks if the incoming request is allowed, performs additional processing like logging or adding session state and finally serves the request. The pipeline consists of the following parts:
* Dispatcher - organize a process of request's passing through pipeline.
* Gate - allows or rejects a request based on a set of rules.
* Filters - performs actions like logging or handling session state. 
* Handlers - where all actual work is done.

The gate and filters are optional but the server must have at least one handler which may have its own set of filters specific to the request. The server matches handlers against a request based on URI. However, this may be overridden.

### Configuration
Below you can see typical scheme that presents how server is configured usually. According to that scheme as example a some request may go through the following path:
* dispatcher passes it through filters F1, F2, F3
* and finds a handler H1 which matches the request
* the handler applies its own filters and performs the actual work.

```
nfx 
{
  wave {
    server {
      prefix { name=”http://+:8080” }
      dispatcher
      {
        gate    { name=”G1” ... }
        filter  { name=”F2” order=1 type=”F2” }
        filter  { name=”F3” order=2 type=”F3” }
                
        handler { 
          name=”H1” order=0 type=”H1” 
          filter { name=”F4” order=0 type=”F4” }  
          filter { name=”F5” order=1 type=”F5” }        
        }        
        handler { name=”H2” order=0 type=”H2” }
        handler { name=”H3” order=0 type=”H3” }
      }
    }
  }
}
```
The node prefix is required. If a handler has no filters it immediately executes the work.

#### Example
Create a console project in Visual Studio and add a reference to the NFX and NFX.Wave DLLs. Create a new handler TestHandler, derive it from the WorkHandler class and override the DoHandleWork method.
```C#
using System;
using NFX.Wave;
using NFX.Environment;

namespace WaveTest {
  class TestHandler : WorkHandler {
    public TestHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
      : base(dispatcher, name, order, match) {
    }

    public TestHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode)
      : base(dispatcher, confNode) {
    }

    protected override void DoHandleWork(WorkContext work) {
      work.Response.WriteLine("Hello, world!");
    }
  }
}
```
Start the server in the Main method.
```C#
using System;
using NFX;
using NFX.Wave;
using NFX.ApplicationModel;

namespace WaveTest {
  class Program {
    static void Main(string[] args) {
      try {
        using (new ServiceBaseApplication(args, null))
        using (var ws = new WaveServer()) {
          ws.Configure(null);
          ws.Start();
          Console.WriteLine("Web server started. Press <ENTER> to terminate...");
          Console.ReadLine();
       }
     }
     catch (Exception ex) {
       Console.WriteLine(ex.ToMessageWithType());
        System.Environment.ExitCode = -1;
     }
   }
  }
}
```
Create configuration file WaveTest.laconf and make sure to set it’s property "Copy to Output Directory" to "Copy always".
```
nfx {
  wave {
    server {
      prefix { name="http://+:8080/" }		
      dispatcher {
        handler { 
          name="Test" order=0 type="WaveTest.TestHandler, WaveTest" match{ } 
        }
      }
    }
  }
}
```
Now the server will respond with "Hello, world!" to every request from localhost:8080.


## Architecture

### WaveServer
The implementation is based on a lightweight **HttpListener** that processes incoming HTTP requests via an injectable WorkDispatcher which governs the threading and WorkContext life cycle. The server processing pipeline is purposely designed to be synchronous-blocking (thread per call) which does not mean that the server is inefficient, to the contrary - this server design is specifically targeting short-called methods relying on a classical thread call stack. This approach obviates the need to create surrogate message loops/synchro contexts, tasks and other objects that introduce extra GC load. The server easily support "dangling"(left open indefinitely) WorkContext instances that can stream events (i.e. SSE/Server Push) and WebSockets from specially-purposed asynchronous notification threads.


### WorkContext
Represents a context for request/response server processing in WAVE framework which includes:

##### Server : WaveServer
The server this context is under.

##### Request : HttpListenerRequest
Returns `HttpListenerRequest` object for this context.

##### Response
A wrapper for `HttpListenerResponse` which represents Response object used to generate web responses to client.

##### Session : WaveSession
Session that this context is linked with.

##### SessionFilter
Returns the first session filter which was injected in the processing line. It is the filter that manages the session state for this context.

##### Handler : WorkHandler
A work handler instance that was matched to perform work on this context or null if the match has not been made yet.

##### GeoEntity
Gets sets geolocation information as detected by GeoLookupHandler. If Session context is injected then get/set passes through into session object.

##### Items
Provides a thread-safe dictionary of items. The underlying collection is lazily allocated.

##### LastError
WaveServer creates an instance of WorkContext for an incoming request and passes it to WorkDispatcher. WorkDispatcher passes the context though pipeline. If any error occurs during processing the request it will be stored in LastError property.

##### RequestedJSON
Returns true if client indicated in response that "application/json" is accepted.

##### Request method indicators:
* IsPOST
* IsGET
* IsPUT
* IsDELETE
* IsPATCH

##### Portal
Returns portal object for this request or null if no portal was injected. To make your web portal with NFX.WAVE library you can use Portal abstract class as a base. It represents a web portal that controls the mapping of types and themes within the site. Portals allow to host differently-looking/behaving sites in the same web application. And you can use Portal hub - a registry of portals. It establishes a context for portal inter-operation (i.e. so one portal may locate another by name) when some settings need to be cloned. This is an application-started singleton instance class.

##### Match : WorkMatch
Returns the work match instance that was made for this requested work or null if nothing was matched yet. WorkMatch decides whether the specifies WorkContext matches the requirements specified in the instance. The match may consider Request and Items properties of work context for match determination. Work matches do not belong to particular handler or filter, so they ARE STATELESS and their instances can be used by multiple different processors (i.e. handlers and filters).

##### Handled
Returns true when the work has been executed by the WorkHandler instance.

##### Aborted
Indicates whether the work context is logically finished and its nested processing (i.e. through Filters/Handlers) should stop. For example, when some filter detects a special  condition (judging by the request) and generates the response and needs to abort the work request so it does no get filtered/processed anymore, it can set this property to true. This mechanism performs much better than throwing exceptions.

##### ReleaseWorkSemaphore
Releases work semaphore that throttles the processing of WorkContext instances. The WorkContext is released automatically in destructor, however there are cases when the semaphore release may be needed sooner, i.e. in a HTTP streaming application where work context instances are kept open indefinitely it may not be desirable to consider long-living work context instances as a throttling factor. Returns true if semaphore was released, false if it was not released during this call as it was already released before.

##### Log
Facilitates context-aware logging.

##### NeedsSession 
Ensures that session is injected if session filter is present in processing chain. If session is already available (Session!=null) then does nothing, otherwise fills Session property with either NEW session if user supplied no session token, OR gets session from session store as defined by the first SessionFilter in the chain.


### NetGate
Represents a network gate - a logical filter of incoming network traffic. Network gate is somewhat similar to a firewall - it allows/denies the int/out traffic based on the set of rules.

### WorkDispatcher
Represents a default dispatcher that dispatches WorkContext calls on the same thread that calls Dispatch(work). May extend this class to implement custom dispatchers, i.e. the once that maintain their own work queue/worker threads.
##### Filters
Returns ordered registry of filters.
##### Handlers
Returns ordered registry of handlers.
##### RegisterFilter/UnRegisterFilter
Registers/unregister filter and returns true if the named instance has not been registered yet/has been removed. It is possible to call this method on active server that is - inject/remove filters while serving requests.
##### RegisterHandler/UnRegisterHandler
Registers handler and returns true if the named instance has not been registered yet/has been removed. It is possible to call this method on active server that is - inject/remove handlers while serving requests
##### Dispatch
Dispatches work into appropriate handler passing through filters. The default implementation processes requests on the calling thread and disposes WorkContext deterministically.
##### InvokeHandler
Finds appropriate handler and invokes it. Returns the appropriate handler or null if work was aborted or already handled. Throws if appropriate handler was not found.
##### GetWorkHandler
Finds the most appropriate work handler to do the work. The default implementation finds first handler with matching URI pattern or null.

### Handlers
Handlers are final work execution destination. Types of handlers:
* CompositeHandler - dispatches work to sub-handlers just like the dispatcher does,
* ContextDumpHandler - dumps WorkContext status - used for debugging purposes,
* EmbeddedSiteHandler - implements handler that serves content from assembly-embedded resources and class actions,
* FileDownloadHandler - downloads local files,
* MVCHandler - handles MVC-related requests,
* NOPHandler - implements handler that does nothing,
* StockContentSiteHandler - serves the embedded content of NFX.Wave library,
* TemplateHandler - implements handler that serves WaveTemplates,
* TypeLookupHandler<T> - represents a base handler for all handlers that dynamically resolve type that performs actual work,
* WorkHandler - abstract, represents a base for all work handlers.

### Filters
Unlike handlers, filters do not necessarily handle work rather augment the work context. Filter classes:
* BeforeAfterFilterBase - abstract, provides base for filters that have before/after semantics,
* ErrorFilter - intercepts error that arise during processing and displays an error page for exceptions and error codes,
* GeoLookupFilter - upon match, looks up user's geolocation based on a IP address,
* LoggingFilter - logs information extracted from WorkContext,
* PortalFilter - manages injection of portal into the work context,
* RedirectFilter - upon match, redirects client to the specified URL resource,
* SecurityFilter - checks permissions before doing work,
* SessionFilter - manages session state in work context,
* StopFilter - stops the processing of WorkContext by throwing exception upon match,
* WorkFilter - abstract, represents a base for all work filters.
Both WorkDispatcher and WorkHandler may have filters. In case of WorkDispatcher they would process every request, whereas located in WorkHandler they would filter specific cases.

##### SessionFilter and storages.
SessionFilter manages session state in work context using `ObjectStoreService` as a back-end store by default. `ObjectStoreService` stores objects in process's memory, asynchronously saving them to an external non-volatile storage upon change and synchronously saving objects upon service stop using file-based `FileObjectStoreProvider`. Applications can use their own providers in order to save session in a different store. Another option is to override `SessionFilter.StowSession`. Applications should call `WorkContext.NeedsSession` method to ensure the session is created. By default NFX.WAVE stores session ID in WV.CV cookie. In the following example the server returns “Hello, world!” on the first request, saves current date in the session and returns it on subsequent requests:
```
nfx {
  wave { ... }
  object-store {
    name="store"
    guid="14b24fd8-d478-44a2-a5e5-e61875068543"
    provider {
      name="file"
      type="NFX.ApplicationModel.Volatile.FileObjectStoreProvider"
      root-path=$'c:\nfx\wave\store\'
    }
  }
}
```
```C#
class TestHandler : WorkHandler {
  ...    
  protected override void DoHandleWork(WorkContext work) {
    work.NeedsSession();   
    if (work.Session.Items.ContainsKey(key)) {
      work.Response.WriteLine(work.Session[key]);
    }
    else {
      work.Session[key] = DateTime.Now;
      work.Response.WriteLine("Hello, world!");
    }
  }
}
```
FileObjectStoreProvider uses root-path and GUID specified in the configuration file as the store directory.

### RecordModelGenerator
Facilitates tasks of JSON generation for record models/rows as needed by `WV.RecordModel.Record(...)` constructor on the client side (see library wv.js).

### WAVE Exceptions
* MVCActionException - wraps inner exceptions capturing stack trace in inner implementing blocks,
* FilterPipelineException - thrown by filter pipeline,
* HTTPStatusException - thrown to indicate various HTTP status conditions,
* WaveException - base exception thrown by the WAVE framework.
