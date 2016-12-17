namespace SharpNetwork
{
    /// <summary>
    /// Specifies the reason for aborting socket's connection.
    /// </summary>
    public enum SocketStreamError
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Socket has been disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// <see cref="T:System.Net.Sockets.SocketException" /> has been thrown.
        /// </summary>
        SocketException,

        /// <summary>
        /// Socket has been closed (possibly by timeout or cancellation).
        /// </summary>
        SocketClosedOrTimedOut
    }
}