using System;
using System.Net;
using System.Net.Sockets;

namespace SharpNetwork
{
    public struct SocketDescription
    {
        public IPEndPoint LocalEndPoint { get; }

        public IPEndPoint RemoteEndPoint { get; }

        public SocketDescription(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            LocalEndPoint = socket.LocalEndPoint as IPEndPoint;

            RemoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
        }
    }
}