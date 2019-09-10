# SharpNetwork
Classes for working with TCP sockets (.NET 4.5) in async/await style while supporting timeouts and cancellation.

Main class list:
* **AsyncTcpListener** - for listening for TCP-connections and processing the requests asynchronously (also supports cancellation via CancellationToken).
* **AsyncSocketStream** - a wrapper for a TCP-socket (inherits from System.IO.Stream) that enables async communication (cancellable ReadAsync() and WriteAsync() methods).
* **AsyncBufferedSocketStream** - inherits from AsyncSocketStream, but exposes read and write buffers as auto-expanding MemoryBuffer instances (from **SharpStructures** library).
