# NFX.Wave Server

WAVE is a "WebAppViewEnhanced" web server which provides DYNAMIC web site services.  

This server is not meant to be exposed directly to the public Internet, rather it should be used as an application server behind the reverse proxy, such as NGINX. This server is designed to serve dynamic data-driven requests/APIs and not meant to be used for serving static content files (although it can).

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
If a handler has no filters it immediately executes the work. The node prefix is required.

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
Create configuration file WaveTest.laconf and make sure to set it’s property “Copy to Output Directory” to “Copy always”.
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
Now the server will respond with “Hello, world!” to every request from localhost:8080.


## Architecture

### WaveServer
The implementation is based on a lightweight HttpListener that processes incoming HTTP requests via an injectable WorkDispatcher which governs the threading and WorkContext lifecycle. The server processing pipeline is purposely designed to be synchronous-blocking (thread per call) which does not mean that the server is inefficient, to the contrary - this server design is specifically targeting short-called methods relying on a classical thread call stack. This approach obviates the need to create surrogate message loops/synchro contexts, tasks and other objects that introduce extra GC load. The server easily support "dangling"(left open indefinitely) WorkContext instances that can stream events (i.e. SSE/Server Push) and WebSockets from specially-purposed asynchronous notification threads.


### WorkContext
Represents a context for request/response server processing in WAVE framework which includes:

##### WaveServer
The server this context is under.

##### Request
Instance of the HttpListenerRequest class.

##### Response
Represents Response object used to generate web responses to client.

##### WaveSession
Session that this context is linked with.

##### SessionFilter
Returns the first session filter which was injected in the processing line. It is the filter that manages the session state for this context.

##### WorkHandler
A work handler instance that was matched to perform work on this context or null if the match has not been made yet.

##### GeoEntity
Gets sets geolocation information as detected by GeoLookupHandler. If Session context is injected then get/set passes through into session object.

##### Items
A thread-safe dictionary.

##### LastError
WaveServer creates an instance of WorkContext for an incoming request and passes it to WorkDispatcher. WorkDispatcher passes the context though pipeline. If any error occurs during processing the request it will be stored in WorkContext.LastError property.

### NetGate
Represents a network gate - a logical filter of incoming network traffic. Network gate is somewhat similar to a firewall - it allows/denies the int/out traffic based on the set of rules.

### WorkDispatcher
Represents a default dispatcher that dispatches WorkContext calls on the same thread that calls Dispatch(work). May extend this class to implement custom dispatchers, i.e. the once that maintain their own work queue/worker threads.

### WorkHandler
Represents a base for all work handlers. Handlers are final work execution destination. Types of handlers:
* CompositeHandler - dispatches work to sub-handlers just like the dispatcher does
* ContextDumpHandler - dumps WorkContext status - used for debugging purposes
* EmbeddedSiteHandler - implements handler that serves content from assembly-embedded resources and class actions
* FileDownloadHandler - downloads local files
* MVCHandler - handles MVC-related requests
* NOPHandler - implements handler that does nothing
* StockContentSiteHandler - serves the embedded content of NFX.Wave library
* TemplateHandler - implements handler that serves WaveTemplates
* TypeLookupHandler<T> - represents a base handler for all handlers that dynamically resolve type that performs actual work.