using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SharpNetwork
{
    [Serializable]
    public class SocketStreamException : Exception
    {
        public SocketStreamError Error { get; }

        public SocketStreamException(SocketStreamError error) : this(error, null)
        {
        }

        public SocketStreamException(SocketStreamError error, Exception innerException) : base(null, innerException)
        {
            Error = error;
        }

        protected SocketStreamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Error = (SocketStreamError)info.GetInt32(nameof(Error));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(Error), (int)Error);

            base.GetObjectData(info, context);
        }
    }
}